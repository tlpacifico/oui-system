import { Routes } from '@angular/router';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { ItemListPageComponent } from './features/inventory/pages/item-list-page.component';
import { ItemDetailPageComponent } from './features/inventory/pages/item-detail-page.component';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPageComponent,
  },
  {
    path: '',
    component: DashboardPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/items',
    component: ItemListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/items/:id',
    component: ItemDetailPageComponent,
    canMatch: [authGuard],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
