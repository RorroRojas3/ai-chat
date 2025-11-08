import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = '';

      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Network error: ${error.error.message}`;
        console.error(errorMessage);
      } else {
        // Backend returned an unsuccessful response code
        switch (error.status) {
          case 400:
            errorMessage =
              'Bad request. Please check your input and try again.';
            console.error('400 Bad Request:', errorMessage);
            break;
          case 401:
            errorMessage = 'Unauthorized, you must login again.';
            console.error('401 Unauthorized:', errorMessage);
            // TODO: Redirect to login page or clear auth tokens
            break;
          case 403:
            errorMessage =
              'Forbidden. You do not have permission to access this resource.';
            console.error('403 Forbidden:', errorMessage);
            break;
          case 404:
            errorMessage = 'Resource not found.';
            console.error('404 Not Found:', errorMessage);
            break;
          case 408:
            errorMessage = 'Request timeout. Please try again.';
            console.error('408 Request Timeout:', errorMessage);
            break;
          case 422:
            errorMessage = 'Validation error. Please check your input.';
            console.error('422 Unprocessable Entity:', errorMessage);
            break;
          case 429:
            errorMessage = 'Too many requests. Please wait and try again.';
            console.error('429 Too Many Requests:', errorMessage);
            break;
          case 500:
            errorMessage =
              'Unexpected error occurred on the server. Please try again later.';
            console.error('500 Internal Server Error:', errorMessage);
            break;
          case 502:
            errorMessage =
              'Bad gateway. The server is temporarily unavailable.';
            console.error('502 Bad Gateway:', errorMessage);
            break;
          case 503:
            errorMessage = 'Service unavailable. Please try again later.';
            console.error('503 Service Unavailable:', errorMessage);
            break;
          case 504:
            errorMessage =
              'Gateway timeout. The server took too long to respond.';
            console.error('504 Gateway Timeout:', errorMessage);
            break;
          case 0:
            errorMessage =
              'Unable to connect to the server. Please check your internet connection.';
            console.error('Network Error (status 0):', errorMessage);
            break;
          default:
            errorMessage = `An error occurred: ${error.status} - ${
              error.statusText || 'Unknown error'
            }`;
            console.error(`HTTP Error ${error.status}:`, errorMessage);
            break;
        }

        // Log additional error details if available
        if (error.error?.message) {
          console.error('Server message:', error.error.message);
        }
      }

      // Return the error to allow components to handle it if needed
      return throwError(() => error);
    })
  );
};
