import { NgClass } from '@angular/common';
import {
  Component,
  input,
  inject,
  signal,
  computed,
  AfterViewInit,
  ElementRef,
  viewChild,
  ChangeDetectionStrategy,
} from '@angular/core';
import { MessageDto } from '../../../dtos/MessageDto';
import { StoreService } from '../../../store/store.service';

/** Duration in milliseconds for copy success feedback */
const COPY_FEEDBACK_DURATION_MS = 2000;

@Component({
  selector: 'app-message-bubble',
  imports: [NgClass],
  templateUrl: './message-bubble.component.html',
  styleUrl: './message-bubble.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageBubbleComponent implements AfterViewInit {
  readonly message = input.required<MessageDto>();
  readonly storeService = inject(StoreService);

  private readonly messageContentRef =
    viewChild<ElementRef<HTMLDivElement>>('messageContent');

  /** Tracks whether the full message was recently copied */
  readonly messageCopied = signal(false);

  /** Determines if the copy message button should be shown */
  readonly canCopyMessage = computed(
    () => !this.message().isOutgoing && !this.storeService.isStreaming()
  );

  ngAfterViewInit(): void {
    this.addCodeBlockCopyButtons();
  }

  /**
   * Copies the entire message content to clipboard.
   * Shows success feedback for a brief duration.
   */
  async copyMessage(): Promise<void> {
    const content = this.message().content;
    if (!content) return;

    try {
      await navigator.clipboard.writeText(content);
      this.showCopyFeedback();
    } catch (error) {
      console.error('Failed to copy message:', error);
    }
  }

  /**
   * Shows temporary success feedback after copying.
   */
  private showCopyFeedback(): void {
    this.messageCopied.set(true);
    setTimeout(() => this.messageCopied.set(false), COPY_FEEDBACK_DURATION_MS);
  }

  /**
   * Adds copy buttons to all code blocks within the message content.
   * Each button provides clipboard functionality with visual feedback.
   */
  private addCodeBlockCopyButtons(): void {
    const contentEl = this.messageContentRef()?.nativeElement;
    if (!contentEl) return;

    const preElements = contentEl.querySelectorAll('pre');
    preElements.forEach((pre) => this.wrapCodeBlockWithCopyButton(pre));
  }

  /**
   * Wraps a pre element with a container and adds a copy button.
   */
  private wrapCodeBlockWithCopyButton(pre: HTMLPreElement): void {
    if (pre.querySelector('.code-copy-btn')) return;

    const wrapper = this.createCodeBlockWrapper(pre);
    const copyBtn = this.createCopyButton();

    copyBtn.addEventListener('click', () =>
      this.handleCodeBlockCopy(pre, copyBtn)
    );
    wrapper.appendChild(copyBtn);
  }

  /**
   * Creates a wrapper div for the code block.
   */
  private createCodeBlockWrapper(pre: HTMLPreElement): HTMLDivElement {
    const wrapper = document.createElement('div');
    wrapper.className = 'code-block-wrapper position-relative';
    pre.parentNode?.insertBefore(wrapper, pre);
    wrapper.appendChild(pre);
    return wrapper;
  }

  /**
   * Creates a copy button element with proper styling and accessibility.
   */
  private createCopyButton(): HTMLButtonElement {
    const btn = document.createElement('button');
    btn.className =
      'code-copy-btn btn btn-sm position-absolute top-0 end-0 m-2';
    btn.type = 'button';
    btn.setAttribute('aria-label', 'Copy code');
    btn.innerHTML = '<i class="bi bi-clipboard"></i>';
    return btn;
  }

  /**
   * Handles the copy action for a code block with visual feedback.
   */
  private async handleCodeBlockCopy(
    pre: HTMLPreElement,
    btn: HTMLButtonElement
  ): Promise<void> {
    const codeEl = pre.querySelector('code');
    const codeText = codeEl?.textContent || pre.textContent || '';

    try {
      await navigator.clipboard.writeText(codeText);
      this.showCodeBlockCopyFeedback(btn);
    } catch (error) {
      console.error('Failed to copy code:', error);
    }
  }

  /**
   * Shows temporary success feedback on the code block copy button.
   */
  private showCodeBlockCopyFeedback(btn: HTMLButtonElement): void {
    btn.innerHTML = '<i class="bi bi-clipboard-check"></i>';
    btn.classList.add('copied');

    setTimeout(() => {
      btn.innerHTML = '<i class="bi bi-clipboard"></i>';
      btn.classList.remove('copied');
    }, COPY_FEEDBACK_DURATION_MS);
  }
}
