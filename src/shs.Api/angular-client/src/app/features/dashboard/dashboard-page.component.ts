import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'oui-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="layout">
      <aside class="sidebar">
        <div class="logo">
          <span>OUI</span>
          <small>System</small>
        </div>
        <nav>
          <a class="nav-item active">Dashboard</a>
          <a class="nav-item">Inventário</a>
          <a class="nav-item">Consignações</a>
          <a class="nav-item">POS</a>
          <a class="nav-item">Financeiro</a>
        </nav>
      </aside>

      <main class="main">
        <header class="header">
          <div class="breadcrumb">Dashboard</div>
          <div class="user">
            <span class="avatar">{{ initials }}</span>
            <span class="user-name">{{ auth.currentUser()?.displayName ?? auth.currentUser()?.email }}</span>
            <button type="button" class="btn-logout" (click)="logout()">Sair</button>
          </div>
        </header>

        <section class="content">
          <h1 class="title">Dashboard (MVP)</h1>
          <p class="subtitle">
            Aqui vão ficar os KPIs principais (vendas do dia, itens em stock, acertos pendentes).
          </p>
        </section>
      </main>
    </div>
  `,
  styles: [
    `
      .layout {
        display: grid;
        grid-template-columns: 240px 1fr;
        min-height: 100vh;
        background: #0b1120;
        color: #e5e7eb;
      }

      .sidebar {
        background: #020617;
        border-right: 1px solid #1f2937;
        padding: 1.25rem 1rem;
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }

      .logo span {
        display: block;
        font-weight: 800;
        font-size: 1.4rem;
      }

      .logo small {
        font-size: 0.75rem;
        color: #9ca3af;
      }

      nav {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        font-size: 0.9rem;
      }

      .nav-item {
        padding: 0.45rem 0.75rem;
        border-radius: 0.5rem;
        color: #9ca3af;
        cursor: pointer;
      }

      .nav-item.active {
        background: linear-gradient(90deg, #6366f1, #8b5cf6);
        color: #f9fafb;
      }

      .main {
        display: flex;
        flex-direction: column;
        min-height: 100vh;
      }

      .header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0.75rem 1.5rem;
        border-bottom: 1px solid #1f2937;
        background: #020617;
      }

      .breadcrumb {
        font-size: 0.9rem;
        color: #9ca3af;
      }

      .user {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .user-name {
        font-size: 0.85rem;
        color: #9ca3af;
      }

      .btn-logout {
        padding: 0.35rem 0.6rem;
        font-size: 0.8rem;
        border-radius: 0.4rem;
        border: 1px solid #4b5563;
        background: transparent;
        color: #9ca3af;
        cursor: pointer;
      }

      .btn-logout:hover {
        background: #374151;
        color: #e5e7eb;
      }

      .avatar {
        width: 32px;
        height: 32px;
        border-radius: 999px;
        background: #4f46e5;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-size: 0.8rem;
        font-weight: 700;
      }

      .content {
        padding: 1.5rem;
      }

      .title {
        margin: 0 0 0.25rem;
        font-size: 1.5rem;
        font-weight: 600;
      }

      .subtitle {
        margin: 0;
        font-size: 0.9rem;
        color: #9ca3af;
      }
    `,
  ],
})
export class DashboardPageComponent {
  constructor(protected readonly auth: AuthService) {}

  get initials(): string {
    const u = this.auth.currentUser();
    if (!u?.displayName) return u?.email?.slice(0, 2).toUpperCase() ?? '?';
    const parts = u.displayName.trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return u.displayName.slice(0, 2).toUpperCase();
  }

  logout(): void {
    this.auth.logout();
  }
}

