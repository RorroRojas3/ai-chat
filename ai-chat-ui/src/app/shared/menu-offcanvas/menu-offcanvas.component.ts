import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StoreService } from '../../store/store.service';
import { ConversationService } from '../../services/conversation.service';
import { FormsModule } from '@angular/forms';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  EMPTY,
  finalize,
  forkJoin,
  of,
  Subject,
  switchMap,
} from 'rxjs';

import { Router } from '@angular/router';
import { ConversationDto } from '../../dtos/ConversationDto';
import { RenameModalComponent } from '../../pages/conversations/components/rename-modal/rename-modal.component';
import { UpdateConversationActionDto } from '../../dtos/actions/conversation/UpdateConversationActionDto';
import { NotificationService } from '../../services/notification.service';
import { DeleteModalComponent } from '../../pages/conversations/components/delete-modal/delete-modal.component';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [
    FormsModule,
    RenameModalComponent,
    DeleteModalComponent,
  ],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent implements OnInit {
  // Constants
  readonly MAX_CONVERSATION_NAME_LENGTH = 40;
  readonly SEARCH_DEBOUNCE_MS = 600;
  readonly OFFCANVAS_TRANSITION_DELAY_MS = 300;

  // Inject dependencies using Angular 19 pattern
  public readonly storeService = inject(StoreService);
  private readonly conversationService = inject(ConversationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);

  private searchSubject = new Subject<string>();
  showRenameModal = false;
  showDeleteModal = false;

  ngOnInit(): void {
    // Set up debounced search with automatic cleanup
    this.searchSubject
      .pipe(
        debounceTime(this.SEARCH_DEBOUNCE_MS),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((filter: string) => {
        this.performSearch(filter);
      });
  }

  /**
   * Performs search using the conversation service
   */
  private performSearch(filter: string): void {
    this.storeService.setMenuConversationSearchFilter(filter);
    this.conversationService
      .loadMenuConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  /**
   * TrackBy function for conversation list to improve performance
   */
  trackByConversationId(_index: number, conversation: ConversationDto): string {
    return conversation.id;
  }

  /**
   * Truncates conversation name if it exceeds max length
   */
  getTruncatedConversationName(conversation: ConversationDto): string {
    return conversation.name.length > this.MAX_CONVERSATION_NAME_LENGTH
      ? conversation.name.slice(0, this.MAX_CONVERSATION_NAME_LENGTH) + '...'
      : conversation.name;
  }

  /**
   * Checks if the given conversation ID matches the current active conversation
   */
  isCurrentConversation(conversationId: string): boolean {
    return this.storeService.conversation()?.id === conversationId;
  }

  /**
   * Handles the click event for selecting a conversation from the menu.
   * Finds the conversation in the menu conversations list, sets it as the active conversation,
   * and navigates to the conversation's chat page.
   *
   * @param conversationId - The unique identifier of the conversation to navigate to
   * @returns void
   */
  onClickConversation(conversationId: string): void {
    const foundConversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (foundConversation) {
      this.storeService.conversation.set(foundConversation);
      this.router.navigate(['chat', 'conversation', foundConversation.id]);
    }
  }

  /**
   * Handles the click event for creating a new conversation.
   * Clears the current conversation data using the store service.
   *
   * @returns void
   */
  onClickCreateNewConversation(): void {
    this.storeService.clearForNewConversation();
    this.router.navigate(['chat']);
  }

  /**
   * Handles search input changes
   */
  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  /**
   * Clears the search term
   */
  clearSearch(): void {
    this.storeService.clearMenuConversationSearchFilter();
    this.conversationService
      .loadMenuConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  /**
   * Navigates to the chat conversations page.
   *
   * @returns {void}
   */
  onClickGoToChats(): void {
    this.storeService.clearForNewConversation();
    this.router.navigate(['conversations']);
  }

  /**
   * Navigates to the projects page.
   *
   * This method is typically called when the user clicks on a menu item
   * to navigate to the projects route.
   *
   * @returns {void}
   */
  onClickGoToProjects(): void {
    this.storeService.clearForNewConversation();
    this.router.navigate(['projects']);
  }

  /**
   * Initiates the rename process for a specific conversation.
   *
   * Finds the conversation by ID, sets it as the active conversation in the store,
   * closes the offcanvas menu, and opens the rename modal.
   *
   * @param conversationId - The unique identifier of the conversation to rename
   * @param event - The click event to stop propagation
   * @returns void
   */
  onRenameConversation(conversationId: string, event: Event): void {
    event.stopPropagation();

    const conversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (conversation) {
      this.storeService.conversation.set(conversation);

      // Close the offcanvas before opening the modal
      this.toggleOffcanvas(false);

      // Small delay to ensure offcanvas closes before modal opens
      setTimeout(() => {
        this.showRenameModal = true;
      }, this.OFFCANVAS_TRANSITION_DELAY_MS);
    }
  }

  /**
   * Handles renaming the active conversation with proper error handling and cleanup.
   * Updates the conversation name, refreshes menu conversations, and optionally
   * reloads page conversations if the renamed conversation is present in the page view.
   *
   * @param newName - The new name for the conversation
   * @returns void
   *
   * @remarks
   * This method:
   * - Prevents memory leaks by using takeUntilDestroyed
   * - Shows error notifications if the rename operation fails
   * - Ensures the modal is closed in the finalize block regardless of success/failure
   * - Reopens the offcanvas menu after the operation completes
   * - Updates the active conversation in the store
   * - Reloads menu conversations to reflect the change
   * - Conditionally reloads page conversations if the conversation exists in that view
   */
  handleRenameConversation(newName: string): void {
    const currentConversation = this.storeService.conversation();
    if (!currentConversation) {
      this.onCloseRenameModal();
      return;
    }

    const request = new UpdateConversationActionDto(currentConversation.id, newName);

    this.conversationService
      .updateConversation(request)
      .pipe(
        catchError(() => {
          this.notificationService.error('Error renaming chat.');
          return EMPTY;
        }),
        switchMap((response) => {
          // Update the active conversation with the response
          this.storeService.conversation.set(response);
          this.notificationService.success('Chat renamed successfully.');

          // Check if we need to reload page conversations
          const shouldReloadPageConversations = this.storeService
            .pageConversations()
            .some((c) => c.id === response.id);

          // Reload menu conversations and conditionally reload page conversations
          return forkJoin({
            menuConversations: this.conversationService.loadMenuConversations(),
            pageConversations: shouldReloadPageConversations
              ? this.conversationService.loadPageConversations(
                  this.storeService.pageConversationSearchFilter(),
                  0,
                  this.storeService.CONVERSATION_PAGE_SIZE,
                )
              : of(undefined),
          });
        }),
        finalize(() => {
          this.onCloseRenameModal();
          this.toggleOffcanvas(true);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  /**
   * Closes the rename modal and resets the modal state.
   *
   * @returns void
   */
  onCloseRenameModal(): void {
    this.showRenameModal = false;
  }

  /**
   * Initiates the delete process for a specific conversation.
   *
   * Finds the conversation by ID, sets it as the active conversation in the store,
   * closes the offcanvas menu, and opens the delete confirmation modal.
   *
   * @param conversationId - The unique identifier of the conversation to delete
   * @param event - The click event to stop propagation
   * @returns void
   */
  onDeleteConversation(conversationId: string, event: Event): void {
    event.stopPropagation();

    const conversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (conversation) {
      this.storeService.conversation.set(conversation);

      this.toggleOffcanvas(false);

      setTimeout(() => {
        this.showDeleteModal = true;
      }, this.OFFCANVAS_TRANSITION_DELAY_MS);
    }
  }

  /**
   * Handles deleting the active conversation with proper error handling and cleanup.
   * Deactivates the conversation, clears it from the store, refreshes menu conversations,
   * and optionally reloads page conversations if the deleted conversation was present in the page view.
   *
   * @returns void
   *
   * @remarks
   * This method:
   * - Prevents memory leaks by using takeUntilDestroyed
   * - Shows error notifications if the delete operation fails
   * - Ensures the modal is closed in the finalize block regardless of success/failure
   * - Reopens the offcanvas menu after the operation completes
   * - Clears the active conversation from the store
   * - Reloads menu conversations to reflect the change
   * - Conditionally reloads page conversations if the conversation exists in that view
   * - Navigates away from the deleted conversation if currently viewing it
   */
  handleDeleteConversation(): void {
    const currentConversation = this.storeService.conversation();
    if (!currentConversation) {
      this.onCloseDeleteModal();
      return;
    }

    const conversationId = currentConversation.id;
    this.conversationService
      .deactivateConversation(conversationId)
      .pipe(
        catchError(() => {
          this.notificationService.error('Error deleting chat.');
          return EMPTY;
        }),
        switchMap(() => {
          this.storeService.conversation.set(null);
          this.notificationService.success('Chat deleted successfully.');

          // Check if we need to reload page conversations
          const shouldReloadPageConversations = this.storeService
            .pageConversations()
            .some((c) => c.id === conversationId);

          // Reload menu conversations and conditionally reload page conversations
          return forkJoin({
            menuConversations: this.conversationService.loadMenuConversations(),
            pageConversations: shouldReloadPageConversations
              ? this.conversationService.loadPageConversations(
                  this.storeService.pageConversationSearchFilter(),
                  0,
                  this.storeService.CONVERSATION_PAGE_SIZE,
                )
              : of(undefined),
          });
        }),
        switchMap(() => {
          // Navigate to chat page after conversations are reloaded
          this.onClickCreateNewConversation();
          return of(undefined);
        }),
        finalize(() => {
          this.onCloseDeleteModal();
          this.toggleOffcanvas(true);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  /**
   * Closes the delete confirmation modal and resets the modal state.
   *
   * @returns void
   */
  onCloseDeleteModal(): void {
    this.showDeleteModal = false;
  }

  /**
   * Programmatically toggles the offcanvas menu open or closed.
   * Uses Bootstrap's offcanvas API to show or hide the menu.
   *
   * @param open - True to open the offcanvas, false to close it
   * @returns void
   */
  private toggleOffcanvas(open: boolean): void {
    const offcanvasElement = document.getElementById('menu-offcanvas');
    if (offcanvasElement) {
      // Get or create Bootstrap's Offcanvas instance
      let bsOffcanvas = (window as any).bootstrap?.Offcanvas?.getInstance(
        offcanvasElement,
      );
      if (!bsOffcanvas) {
        bsOffcanvas = new (window as any).bootstrap.Offcanvas(offcanvasElement);
      }

      if (open) {
        bsOffcanvas.show();
      } else {
        bsOffcanvas.hide();
      }
    }
  }
}
