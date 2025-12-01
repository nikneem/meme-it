import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { retry, timer } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { API_BASE_URL } from '../constants/api.constants';

/**
 * HTTP status codes that should trigger a retry
 */
const RETRYABLE_STATUS_CODES = new Set([
    408, // Request Timeout
    429, // Too Many Requests
    500, // Internal Server Error
    502, // Bad Gateway
    503, // Service Unavailable
    504, // Gateway Timeout
]);

/**
 * HTTP Interceptor that:
 * 1. Adds Bearer token to Authorization header if available and request is to API_BASE_URL
 * 2. Retries failed requests with exponential backoff for retryable errors
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getToken();

    // Clone request and add Authorization header if token exists and request is to our API
    const isApiRequest = req.url.startsWith(API_BASE_URL);
    const authReq = token && isApiRequest
        ? req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        })
        : req;

    // Apply retry logic with exponential backoff
    return next(authReq).pipe(
        retry({
            count: 2,
            delay: (error: HttpErrorResponse, retryCount: number) => {
                // Only retry for specific status codes
                if (error instanceof HttpErrorResponse && RETRYABLE_STATUS_CODES.has(error.status)) {
                    // Exponential backoff: 1000ms, 2000ms
                    const delayMs = Math.pow(2, retryCount) * 1000;
                    console.log(`Retrying request (attempt ${retryCount + 1}/2) after ${delayMs}ms`);
                    return timer(delayMs);
                }
                // Don't retry for other errors
                throw error;
            }
        })
    );
};
