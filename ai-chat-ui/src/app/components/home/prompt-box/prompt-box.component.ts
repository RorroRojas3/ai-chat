import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';
import {
  firstValueFrom,
  Subject,
  Subscription,
  interval,
  takeUntil,
  switchMap,
  takeWhile,
} from 'rxjs';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MessageDto } from '../../../dtos/MessageDto';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import { SessionService } from '../../../services/session.service';
import { DocumentService } from '../../../services/document.service';
import { ModelDto } from '../../../dtos/ModelDto';
import { AiServiceType } from '../../../dtos/const/AiServiceType';
import { JobDto } from '../../../dtos/JobDto';
import { JobStatusDto } from '../../../dtos/JobStatusDto';
import { FileUploadDto } from '../../../dtos/FileUploadDto';
import { JobStatus } from '../../../dtos/const/JobStatus';
import { JobState } from '../../../dtos/const/JobState';

@Component({
  selector: 'app-prompt-box',
  imports: [FormsModule],
  templateUrl: './prompt-box.component.html',
  styleUrl: './prompt-box.component.scss',
})
export class PromptBoxComponent implements OnDestroy {
  constructor(
    public storeService: StoreService,
    private chatService: ChatService,
    private sanitizer: DomSanitizer,
    private sessionService: SessionService,
    private documentService: DocumentService
  ) {
    this.md = new markdownit({
      html: true,
      linkify: true,
      typographer: true,
    }).use(markdown_it_highlightjs, { hljs });
  }

  prompt: string = '';
  message?: string = '';
  md: markdownit;
  sseSubscription: Subscription | undefined;
  private destroy$ = new Subject<void>();
  @Output() markdown = new EventEmitter<SafeHtml>();
  sanitizeHtml!: SafeHtml;

  // File handling properties
  attachedFiles: FileUploadDto[] = [];
  showAttachedFiles: boolean = true;
  isDragOver: boolean = false;
  private jobPollingSubscriptions: Map<string, Subscription> = new Map();

  async onClickCreateSession(): Promise<void> {
    if (!this.prompt.trim() || this.storeService.disablePromptButton()) {
      return;
    }

    try {
      this.storeService.stream.set('');
      this.storeService.disablePromptButton.set(true);

      if (this.storeService.sessionId() === '') {
        const session = await firstValueFrom(
          this.sessionService.createSession()
        );
        this.storeService.sessionId.set(session.id);
      }

      this.showAttachedFiles = false;
      this.storeService.messages.update((messages) => [
        ...messages,
        new MessageDto(this.prompt, true, undefined),
      ]);

      // Upload attached files to the session
      if (this.attachedFiles.length > 0) {
        const jobs = await this.uploadAttachedFiles();
        // Start polling for job status
        await this.pollJobStatuses(jobs);
        this.clearAllFiles();
      }

      this.sseSubscription = this.chatService
        .getServerSentEvent(this.prompt)
        .subscribe({
          next: (message) => {
            if (!this.storeService.isStreaming()) {
              this.prompt = '';
              this.storeService.isStreaming.set(true);
            }
            this.storeService.stream.update((stream) => stream + message);
            const html = this.md.render(this.storeService.stream());
            this.sanitizeHtml = this.sanitizer.bypassSecurityTrustHtml(html);
            this.markdown.emit(this.sanitizeHtml);
          },
          complete: () => {
            this.storeService.stream.set('');
            this.storeService.messages.update((messages) => [
              ...messages,
              new MessageDto('', false, this.sanitizeHtml),
            ]);
            this.storeService.disablePromptButton.set(false);
            this.storeService.isStreaming.set(false);
            this.storeService.streamMessage.set(
              new MessageDto('', false, undefined)
            );
            // Refresh sessions based on current search state
            if (this.storeService.hasSearchFilter()) {
              this.sessionService
                .searchSessions(this.storeService.searchFilter())
                .subscribe((sessions) => {
                  this.storeService.sessions.set(sessions);
                });
            } else {
              this.sessionService.searchSessions('').subscribe((sessions) => {
                this.storeService.sessions.set(sessions);
              });
            }
          },
          error: (error) => {
            this.storeService.stream.set('');
            this.storeService.disablePromptButton.set(false);
            this.storeService.isStreaming.set(false);
            this.storeService.streamMessage.set(
              new MessageDto('', false, undefined)
            );
          },
        });
    } catch (error) {
      // Handle errors appropriately
      console.error('Error in session creation:', error);
      // You might want to show an error message to the user
    } finally {
      this.prompt = '';
      this.showAttachedFiles = true;
      this.clearAllFiles();
    }
  }

  /**
   * Handles model selection change events and updates the selected model ID in the store.
   *
   * @param event - The ID of the newly selected model
   */
  onModelChange(event: ModelDto): void {
    this.storeService.selectedModel.set(event);
  }

  // File selection handler
  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(input.files);
    }
    // Reset the input value to allow selecting the same file again
    input.value = '';
  }

  // Handle multiple files
  private handleFiles(fileList: FileList): void {
    Array.from(fileList).forEach((file) => {
      // Only accept PDF files
      if (
        file.type === 'application/pdf' ||
        file.name.toLowerCase().endsWith('.pdf')
      ) {
        const attachedFile: FileUploadDto = {
          id: crypto.randomUUID(),
          name: file.name,
          size: file.size,
          file: file,
          status: undefined,
          progress: 0,
        };
        this.attachedFiles.push(attachedFile);
      } else {
        // Show error message for non-PDF files
        console.warn(
          `File "${file.name}" is not a PDF file and will be ignored.`
        );
        // You could also show a toast notification or alert here
      }
    });
  }

  // Remove a specific file
  removeFile(fileId: string): void {
    this.attachedFiles = this.attachedFiles.filter(
      (file) => file.id !== fileId
    );
  }

  // Clear all attached files
  clearAllFiles(): void {
    this.attachedFiles = [];
  }

  // Format file size for display
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Upload all attached files to the current session
  private async uploadAttachedFiles(): Promise<JobDto[]> {
    const sessionId = this.storeService.sessionId();
    if (!sessionId) {
      throw new Error('No session ID available for file upload');
    }

    // Update file status to uploading
    this.attachedFiles.forEach((file) => {
      file.status = JobStatus.Queued;
    });

    // Upload files sequentially and wait for each to complete
    const uploadPromises = this.attachedFiles.map((attachedFile) =>
      firstValueFrom(
        this.documentService.createDocument(sessionId, attachedFile.file)
      )
    );

    const jobs = await Promise.all(uploadPromises);
    return jobs;
  }

  // Poll job statuses for uploaded files
  private async pollJobStatuses(jobs: JobDto[]): Promise<void> {
    const pollingPromises = jobs.map((job, index) => {
      const attachedFile = this.attachedFiles[index];
      if (attachedFile) {
        return this.startJobPolling(job, attachedFile);
      }
      return Promise.resolve();
    });

    await Promise.all(pollingPromises);
  }

  // Start polling for a specific job
  private startJobPolling(
    job: JobDto,
    attachedFile: FileUploadDto
  ): Promise<void> {
    return new Promise((resolve, reject) => {
      const subscription = interval(5000) // Poll every 5 seconds
        .pipe(
          switchMap(() => this.documentService.getUploadStatus(job)),
          takeUntil(this.destroy$),
          takeWhile(
            (status: JobStatusDto) =>
              status.state !== 'Succeeded' && status.state !== 'Failed',
            true
          )
        )
        .subscribe({
          next: (result: JobStatusDto) => {
            // Update attached file status
            attachedFile.progress = result.progress;

            if (result.state === JobState.Succeeded) {
              attachedFile.status = 'succeeded';
              console.log(`File ${attachedFile.name} upload succeeded`);
              this.jobPollingSubscriptions.delete(attachedFile.id);
              resolve();
            } else if (result.state === JobState.Failed) {
              attachedFile.status = 'failed';
              console.error(
                `File ${attachedFile.name} upload failed: ${result.status}`
              );
              this.jobPollingSubscriptions.delete(attachedFile.id);
              reject(new Error(`Upload failed: ${result.status}`));
            } else {
              attachedFile.status = result.status;
              attachedFile.progress = result.progress;
              console.log(
                `File ${attachedFile.name} processing: ${result.progress}%`
              );
            }
          },
          error: (error) => {
            attachedFile.status = 'failed';
            console.error(
              `Error polling status for file ${attachedFile.name}:`,
              error
            );
            this.jobPollingSubscriptions.delete(attachedFile.id);
            reject(error);
          },
          complete: () => {
            this.jobPollingSubscriptions.delete(attachedFile.id);
          },
        });

      // Store the subscription for cleanup
      this.jobPollingSubscriptions.set(attachedFile.id, subscription);
    });
  }

  // Drag and drop event handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(files);
    }
  }

  /**
   * Handles keydown events on the textarea
   * Sends message on Enter (but allows Shift+Enter for new lines)
   */
  onTextareaKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onClickCreateSession();
    }
  }

  /**
   * Handles keydown events on the form
   * Prevents form submission on Enter key in form elements
   */
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
    }
  }

  /**
   * Gets the appropriate Bootstrap icon class based on the AI service ID.
   * @param aiServiceId The unique identifier for the AI service
   * @returns Bootstrap icon class string
   */
  getServiceIcon(aiServiceId: string): string {
    switch (aiServiceId) {
      case AiServiceType.Ollama:
        return 'bi bi-robot'; // Robot icon for Ollama
      case AiServiceType.OpenAI:
        return 'bi bi-openai'; // Lightning charge icon for OpenAI
      case AiServiceType.AzureAIFoundry:
        return 'bi bi-microsoft';
      case AiServiceType.Anthropic:
        return 'bi bi-anthropic';
      default:
        return 'bi bi-question-circle'; // Question circle for unknown services
    }
  }

  /**
   * Angular lifecycle hook that is called when the component is destroyed.
   * Cleans up the SSE (Server-Sent Events) subscription to prevent memory leaks.
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }

    // Clean up all job polling subscriptions
    this.jobPollingSubscriptions.forEach((subscription) =>
      subscription.unsubscribe()
    );
    this.jobPollingSubscriptions.clear();
  }
}
