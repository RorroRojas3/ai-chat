import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
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
import { StoreService } from '../../store/store.service';
import { ConversationService } from '../../services/conversation.service';
import { ConversationDto } from '../../dtos/ConversationDto';
import { RenameModalComponent } from '../../pages/conversations/components/rename-modal/rename-modal.component';
import { DeleteModalComponent } from '../../pages/conversations/components/delete-modal/delete-modal.component';
import { UpdateConversationActionDto } from '../../dtos/actions/conversation/UpdateConversationActionDto';
import { NotificationService } from '../../services/notification.service';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-sidebar',
  imports: [
    FormsModule,
    RouterLink,
    RouterLinkActive,
    RenameModalComponent,
    DeleteModalComponent,
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements OnInit {
  readonly MAX_CONVERSATION_NAME_LENGTH = 40;
  readonly SEARCH_DEBOUNCE_MS = 600;

  public readonly storeService = inject(StoreService);
  private readonly conversationService = inject(ConversationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(MsalService);

  collapsed = signal(false);
  mobileOpen = signal(false);

  showRenameModal = false;
  showDeleteModal = false;

  private searchSubject = new Subject<string>();

  ngOnInit(): void {
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

  private performSearch(filter: string): void {
    this.storeService.setMenuConversationSearchFilter(filter);
    this.conversationService
      .loadMenuConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  toggleSidebar(): void {
    if (window.innerWidth < 768) {
      this.mobileOpen.set(false);
      return;
    }
    if (window.innerWidth < 1024) {
      return;
    }
    this.collapsed.update((v) => !v);
  }

  openMobileSidebar(): void {
    this.mobileOpen.set(true);
  }

  closeMobileSidebar(): void {
    this.mobileOpen.set(false);
  }

  trackByConversationId(_index: number, conversation: ConversationDto): string {
    return conversation.id;
  }

  getTruncatedConversationName(conversation: ConversationDto): string {
    return conversation.name.length > this.MAX_CONVERSATION_NAME_LENGTH
      ? conversation.name.slice(0, this.MAX_CONVERSATION_NAME_LENGTH) + '...'
      : conversation.name;
  }

  isCurrentConversation(conversationId: string): boolean {
    return this.storeService.conversation()?.id === conversationId;
  }

  onClickConversation(conversationId: string): void {
    const foundConversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (foundConversation) {
      this.storeService.conversation.set(foundConversation);
      this.router.navigate(['chat', 'conversation', foundConversation.id]);
      this.closeMobileSidebar();
    }
  }

  onClickCreateNewConversation(): void {
    this.storeService.clearForNewConversation();
    this.router.navigate(['chat']);
    this.closeMobileSidebar();
  }

  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  clearSearch(): void {
    this.storeService.clearMenuConversationSearchFilter();
    this.conversationService
      .loadMenuConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  onRenameConversation(conversationId: string, event: Event): void {
    event.stopPropagation();
    const conversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (conversation) {
      this.storeService.conversation.set(conversation);
      this.showRenameModal = true;
    }
  }

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
          this.storeService.conversation.set(response);
          this.notificationService.success('Chat renamed successfully.');

          const shouldReloadPageConversations = this.storeService
            .pageConversations()
            .some((c) => c.id === response.id);

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
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  onCloseRenameModal(): void {
    this.showRenameModal = false;
  }

  onDeleteConversation(conversationId: string, event: Event): void {
    event.stopPropagation();
    const conversation = this.storeService
      .menuConversations()
      .find((c) => c.id === conversationId);
    if (conversation) {
      this.storeService.conversation.set(conversation);
      this.showDeleteModal = true;
    }
  }

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

          const shouldReloadPageConversations = this.storeService
            .pageConversations()
            .some((c) => c.id === conversationId);

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
          this.onClickCreateNewConversation();
          return of(undefined);
        }),
        finalize(() => {
          this.onCloseDeleteModal();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  onCloseDeleteModal(): void {
    this.showDeleteModal = false;
  }

  /**
   * Gets the user's display name from the active MSAL account.
   */
  getUserDisplayName(): string {
    const account = this.authService.instance.getActiveAccount();
    return account?.name ?? 'User';
  }

  /**
   * Gets the user's initials from the active MSAL account display name.
   */
  getUserInitials(): string {
    const name = this.getUserDisplayName();
    const parts = name.split(' ').filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return (parts[0]?.[0] ?? 'U').toUpperCase();
  }
}
