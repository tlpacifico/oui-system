import { inject } from '@angular/core';
import { Router, CanMatchFn } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // Check if user is authenticated (computed signal returns boolean)
  if (auth.isAuthenticated()) {
    return true;
  }

  router.navigateByUrl('/login');
  return false;
};
