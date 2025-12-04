import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { PasscodeAuthService } from '@services/passcode-auth.service';

export const passcodeGuard: CanActivateFn = (route, state) => {
    const passcodeAuthService = inject(PasscodeAuthService);
    const router = inject(Router);

    if (passcodeAuthService.isAuthenticated()) {
        return true;
    }

    // Redirect to login page and preserve the intended destination
    router.navigate(['/management/login'], {
        queryParams: { returnUrl: state.url }
    });
    return false;
};
