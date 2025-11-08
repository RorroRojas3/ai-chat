import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { inject } from '@angular/core';
import { NotificationService } from '../services/notification.service';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = '';

      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Network error: ${error.error.message}`;
        notificationService.error(errorMessage);
      } else {
        // Backend returned an unsuccessful response code
        switch (error.status) {
          case 400:
            errorMessage =
              'Bad request. Please check your input and try again.';
            notificationService.error(errorMessage);
            break;
          case 401:
            errorMessage = 'Unauthorized, you must login again.';
            notificationService.error(errorMessage);
            // TODO: Redirect to login page or clear auth tokens
            break;
          case 403:
            errorMessage =
              'Forbidden. You do not have permission to access this resource.';
            notificationService.error(errorMessage);
            break;
          case 404:
            errorMessage = 'Resource not found.';
            notificationService.error(errorMessage);
            break;
          case 408:
            errorMessage = 'Request timeout. Please try again.';
            notificationService.warning(errorMessage);
            break;
          case 422:
            errorMessage = 'Validation error. Please check your input.';
            notificationService.error(errorMessage);
            break;
          case 429:
            errorMessage = 'Too many requests. Please wait and try again.';
            notificationService.warning(errorMessage);
            break;
          case 500:
            errorMessage =
              'Unexpected error occurred on the server. Please try again later.';
            notificationService.error(errorMessage);
            break;
          case 502:
            errorMessage =
              'Bad gateway. The server is temporarily unavailable.';
            notificationService.error(errorMessage);
            break;
          case 503:
            errorMessage = 'Service unavailable. Please try again later.';
            notificationService.error(errorMessage);
            break;
          case 504:
            errorMessage =
              'Gateway timeout. The server took too long to respond.';
            notificationService.error(errorMessage);
            break;
          case 0:
            errorMessage =
              'Unable to connect to the server. Please check your internet connection.';
            notificationService.error(errorMessage);
            break;
          default:
            errorMessage = `An error occurred: ${error.status} - ${
              error.statusText || 'Unknown error'
            }`;
            notificationService.error(errorMessage);
            break;
        }

        // Log additional error details to console for debugging
        if (error.error?.message) {
          console.error('Server message:', error.error.message);
        }
      }

      // Return the error to allow components to handle it if needed
      return throwError(() => error);
    })
  );
};
