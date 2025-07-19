import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { StoreService } from '../../../store/store.service';
import { ChatService } from '../../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, Subject, Subscription } from 'rxjs';
import markdownit from 'markdown-it';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MessageDto } from '../../../dtos/MessageDto';
import hljs from 'highlight.js';
import markdown_it_highlightjs from 'markdown-it-highlightjs';
import { SessionService } from '../../../services/session.service';
import { DocumentService } from '../../../services/document.service';

interface AttachedFile {
  id: string;
  name: string;
  size: number;
  file: File;
}

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
  attachedFiles: AttachedFile[] = [];
  isDragOver: boolean = false;
  private fileIdCounter: number = 0;

  async onClickCreateSession(): Promise<void> {
    if (!this.prompt.trim()) {
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

      // Upload attached files to the session
      if (this.attachedFiles.length > 0) {
        await this.uploadAttachedFiles();
      }

      this.storeService.messages.update((messages) => [
        ...messages,
        new MessageDto(this.prompt, true, undefined),
      ]);

      this.storeService.isStreaming.set(true);
      this.sseSubscription = this.chatService
        .getServerSentEvent(this.prompt)
        .subscribe({
          next: (message) => {
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
            this.sessionService.getSessions().subscribe((sessions) => {
              this.storeService.sessions.set(sessions);
            });
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

      this.prompt = '';
      // Clear attached files after sending
      this.clearAllFiles();
    } catch (error) {
      // Handle errors appropriately
      console.error('Error in session creation:', error);
      // You might want to show an error message to the user
    }
  }

  onModelChange(event: string): void {
    this.storeService.selectedModelId.set(event);
  }

  // Drag and drop handlers
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
      console.log('Files dropped');
      this.handleFiles(files);
    }
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
  // ...existing code...

  // Handle multiple files
  private handleFiles(fileList: FileList): void {
    Array.from(fileList).forEach((file) => {
      // Only accept PDF files
      if (
        file.type === 'application/pdf' ||
        file.name.toLowerCase().endsWith('.pdf')
      ) {
        const attachedFile: AttachedFile = {
          id: this.generateFileId(),
          name: file.name,
          size: file.size,
          file: file,
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

  // ...existing code...

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

  // Generate unique file ID
  private generateFileId(): string {
    return `file_${Date.now()}_${++this.fileIdCounter}`;
  }

  // Upload all attached files to the current session
  private async uploadAttachedFiles(): Promise<void> {
    const sessionId = this.storeService.sessionId();
    if (!sessionId) {
      throw new Error('No session ID available for file upload');
    }

    // Upload files sequentially and wait for each to complete
    const uploadPromises = this.attachedFiles.map((attachedFile) =>
      firstValueFrom(
        this.documentService.createDocument(sessionId, attachedFile.file)
      )
    );

    await Promise.all(uploadPromises);
  }

  ngOnDestroy(): void {
    if (this.sseSubscription) {
      this.sseSubscription.unsubscribe();
    }
  }
}
