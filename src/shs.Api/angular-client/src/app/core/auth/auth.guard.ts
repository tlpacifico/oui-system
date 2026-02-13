import { inject } from '@angular/core';
import { Router, CanMatchFn } from '@angular/router';
import { Auth } from '@angular/fire/auth';
import { AuthService } from './auth.service';

export const authGuard: CanMatchFn = async () => {
  const auth = inject(Auth);
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for Firebase Auth to finish restoring the session
  await auth.authStateReady();

  if (auth.currentUser) {
    return true;
  }

  // Clear stale localStorage data if Firebase says not authenticated
  if (authService.isAuthenticated()) {
    authService.logout().subscribe();
  }

  router.navigateByUrl('/login');
  return false;
};
