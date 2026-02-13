import { inject } from '@angular/core';
import { Router, CanMatchFn } from '@angular/router';
import { AuthService } from './auth.service';

export function permissionGuard(requiredPermission: string): CanMatchFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.hasPermission(requiredPermission)) {
      return true;
    }

    router.navigateByUrl('/');
    return false;
  };
}

export function anyPermissionGuard(...requiredPermissions: string[]): CanMatchFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.hasAnyPermission(requiredPermissions)) {
      return true;
    }

    router.navigateByUrl('/');
    return false;
  };
}

export function roleGuard(requiredRole: string): CanMatchFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.hasRole(requiredRole)) {
      return true;
    }

    router.navigateByUrl('/');
    return false;
  };
}
