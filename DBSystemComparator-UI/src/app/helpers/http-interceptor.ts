import { inject } from '@angular/core';
import { HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/common/notification-service';

export const HTTPInterceptor: HttpInterceptorFn = (request: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const notificationService = inject(NotificationService);

  const authRequest = request;

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status >= 400) {
        notificationService.showErrorToast(error?.error?.message);
      }
      else {
        notificationService.showErrorToast('Unable to connect to the application server.');
      }

      return throwError(() => error);
    })
  );
};