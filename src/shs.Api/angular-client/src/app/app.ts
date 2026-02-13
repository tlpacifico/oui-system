import { Component, computed, inject, OnInit, effect } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth/auth.service';
import { HasPermissionDirective } from './core/auth/directives/has-permission.directive';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, HasPermissionDirective],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly auth = inject(AuthService);
  protected readonly showShell = computed(() => this.auth.isAuthenticated());

  constructor() {
    // Load permissions when authentication state changes
    effect(() => {
      if (this.auth.isAuthenticated() && this.auth.permissions().length === 0) {
        //this.auth.loadUserAuthContext().subscribe({
          //error: (err) => console.error('Failed to load user context:', err)
        //});
      }
    });
  }

  ngOnInit() {
    // Load permissions on app initialization if user is already authenticated
    if (this.auth.isAuthenticated()) {
     // this.auth.loadUserAuthContext().subscribe({
        //error: (err) => console.error('Failed to load user context:', err)
      //});
    }
  }

  get initials(): string {
    const u = this.auth.currentUser();
    if (!u?.displayName) return u?.email?.slice(0, 2).toUpperCase() ?? '?';
    const parts = u.displayName.trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return u.displayName.slice(0, 2).toUpperCase();
  }

  logout(): void {
    this.auth.logout().subscribe();
  }
}
