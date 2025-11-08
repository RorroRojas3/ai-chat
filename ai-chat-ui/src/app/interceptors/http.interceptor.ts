import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { inject } from '@angular/core';
import { NotificationService } from '../services/notification.service';
import { ErrorDto } from '../dtos/ErrorDto';
import { environment } from '../../environments/environment';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage: string;

      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Network error: ${error.error.message}`;
        notificationService.error(errorMessage);
      } else {
        const apiError = extractErrorFromDto(error);

        // Backend returned an unsuccessful response code
        switch (error.status) {
          case 400:
            errorMessage =
              apiError || 'Bad request. Please check your input and try again.';
            notificationService.error(errorMessage);
            break;
          case 401:
            errorMessage = apiError || 'Unauthorized, you must log in again.';
            notificationService.error(errorMessage);
            // TODO: Redirect to login page or clear auth tokens
            break;
          case 403:
            errorMessage =
              apiError ||
              'Forbidden. You do not have permission to access this resource.';
            notificationService.error(errorMessage);
            break;
          case 404:
            errorMessage = apiError || 'Resource not found.';
            notificationService.error(errorMessage);
            break;
          case 408:
            errorMessage = apiError || 'Request timeout. Please try again.';
            notificationService.warning(errorMessage);
            break;
          case 422:
            errorMessage =
              apiError || 'Validation error. Please check your input.';
            notificationService.error(errorMessage);
            break;
          case 429:
            errorMessage =
              apiError || 'Too many requests. Please wait and try again.';
            notificationService.warning(errorMessage);
            break;
          case 500:
            errorMessage =
              apiError ||
              'An unexpected error occurred on the server. Please try again later.';
            notificationService.error(errorMessage);
            break;
          case 502:
            errorMessage =
              apiError || 'Bad gateway. The server is temporarily unavailable.';
            notificationService.error(errorMessage);
            break;
          case 503:
            errorMessage =
              apiError || 'Service unavailable. Please try again later.';
            notificationService.error(errorMessage);
            break;
          case 504:
            errorMessage =
              apiError ||
              'Gateway timeout. The server took too long to respond.';
            notificationService.error(errorMessage);
            break;
          case 0:
            errorMessage =
              'Unable to connect to the server. Please check your internet connection.';
            notificationService.error(errorMessage);
            break;
          default:
            errorMessage =
              apiError ||
              `An error occurred: ${error.status} - ${
                error.statusText || 'Unknown error'
              }`;
            notificationService.error(errorMessage);
            break;
        }

        if (!environment.production) {
          logErrorDetails(error);
        }
      }

      // Return the error to allow components to handle it if needed
      return throwError(() => error);
    })
  );
};

/**
 * Extracts error message(s) from ErrorDto structure
 */
function extractErrorFromDto(error: HttpErrorResponse): string | null {
  if (!error.error) {
    return null;
  }

  const errorDto = error.error as Partial<ErrorDto>;

  // Check if the error response matches ErrorDto structure
  if (
    errorDto.errors &&
    Array.isArray(errorDto.errors) &&
    errorDto.errors.length > 0
  ) {
    // Join multiple errors with line breaks or return first error
    return errorDto.errors.length === 1
      ? errorDto.errors[0]
      : errorDto.errors.join('\n\n');
  }

  return null;
}

/**
 * Logs detailed error information to console for debugging
 */
function logErrorDetails(error: HttpErrorResponse): void {
  if (!error.error) {
    return;
  }

  const errorDto = error.error as Partial<ErrorDto>;

  console.group(`HTTP Error ${error.status}`);

  if (errorDto.errors && errorDto.errors.length > 0) {
    console.error('Errors:', errorDto.errors);
  }

  if (errorDto.traceId) {
    console.error('Trace ID:', errorDto.traceId);
  }

  if (errorDto.timestamp) {
    console.error('Timestamp:', errorDto.timestamp);
  }

  console.groupEnd();
}
