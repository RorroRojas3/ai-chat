import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { JobDto } from '../dtos/JobDto';
import { JobStatusDto } from '../dtos/JobStatusDto';
import { DocumentFormats } from '../dtos/const/DocumentFormats';

@Injectable({
  providedIn: 'root',
})
export class DocumentService {
  constructor(private http: HttpClient) {}

  /**
   * Creates a document by uploading a file to the specified session.
   *
   * @param sessionId - The unique identifier of the session to associate the document with
   * @param content - The file to be uploaded as a document
   * @returns void - This method does not return a value but logs the response upon successful creation
   *
   * @remarks
   * This method sends a POST request with the file as FormData to the documents API endpoint.
   * The response is logged to the console upon successful completion.
   */
  createDocument(sessionId: string, content: File): Observable<JobDto> {
    const formData = new FormData();
    formData.append('file', content);

    return this.http.post<JobDto>(
      `${environment.apiUrl}documents/sessions/${sessionId}`,
      formData
    );
  }

  /**
   * Retrieves the upload status for a specific job.
   *
   * @param job - The job data transfer object containing the job ID
   * @returns An Observable that emits a number representing the upload status
   */
  getUploadStatus(job: JobDto): Observable<JobStatusDto> {
    return this.http.get<JobStatusDto>(
      `${environment.apiUrl}documents/upload-status/${job.id}`
    );
  }

  /**
   * Retrieves the conversation history for a specific session as a downloadable file.
   *
   * @param sessionId - The unique identifier of the session whose conversation history is to be retrieved
   * @param format - The format in which the conversation history should be retrieved
   * @returns An Observable that emits an object containing the blob and filename
   *
   * @example
   * ```typescript
   * this.documentService.getConversationHistory('session-123').subscribe(
   *   ({ blob, fileName }) => {
   *     this.documentService.downloadFile(blob, fileName);
   *   }
   * );
   * ```
   */
  getConversationHistory(
    sessionId: string,
    format: DocumentFormats
  ): Observable<{ blob: Blob; fileName: string }> {
    return this.http
      .get(
        `${environment.apiUrl}documents/sessions/${sessionId}/conversation-history?documentFormat=${format}`,
        {
          responseType: 'blob',
          observe: 'response',
        }
      )
      .pipe(
        map((response) => {
          const contentDisposition = response.headers.get(
            'Content-Disposition'
          );
          let fileName = `conversation-${sessionId}.pdf`; // fallback filename

          if (contentDisposition) {
            // Extract filename from Content-Disposition header
            // Matches: filename="file.pdf" or filename=file.pdf or filename*=UTF-8''file.pdf
            const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
            const matches = filenameRegex.exec(contentDisposition);
            if (matches && matches[1]) {
              fileName = matches[1].replace(/['"]/g, '').trim();
            }

            // Handle RFC 5987 encoded filenames (filename*=UTF-8''example.pdf)
            const filenameStarRegex = /filename\*=UTF-8''(.+)/;
            const starMatches = filenameStarRegex.exec(contentDisposition);
            if (starMatches && starMatches[1]) {
              fileName = decodeURIComponent(starMatches[1]);
            }
          }

          return {
            blob: response.body as Blob,
            fileName: fileName,
          };
        })
      );
  }

  /**
   * Downloads a file by creating a temporary anchor element and triggering a click event.
   *
   * @param blob - The Blob object containing the file data to be downloaded
   * @param fileName - The name to be used for the downloaded file
   *
   * @remarks
   * This method creates a temporary URL for the blob, appends an anchor element to the DOM,
   * triggers a download, and then cleans up by removing the anchor and revoking the URL.
   *
   * @example
   * ```typescript
   * const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
   * downloadFile(blob, 'hello.txt');
   * ```
   */
  downloadFile(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
