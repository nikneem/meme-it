import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { PasscodeAuthService } from '@services/passcode-auth.service';
import { API_BASE_URL } from '../constants/api.constants';

export const apiKeyInterceptor: HttpInterceptorFn = (req, next) => {
    const passcodeAuthService = inject(PasscodeAuthService);

    // Only add X-ApiKey header to requests going to the Memes API
    if (req.url.startsWith(`${API_BASE_URL}/memes`)) {
        const passcode = passcodeAuthService.getPasscode();

        if (passcode) {
            const clonedRequest = req.clone({
                setHeaders: {
                    'X-ApiKey': passcode
                }
            });
            return next(clonedRequest);
        }
    }

    return next(req);
};
