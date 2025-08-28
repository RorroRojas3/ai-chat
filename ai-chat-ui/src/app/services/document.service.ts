import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { JobDto } from '../dtos/JobDto';
import { JobStatusDto } from '../dtos/JobStatusDto';

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
}
