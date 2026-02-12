import { Component, computed, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly auth = inject(AuthService);
  protected readonly showShell = computed(() => this.auth.isAuthenticated());

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
