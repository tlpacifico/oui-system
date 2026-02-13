import { Routes } from '@angular/router';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { ItemListPageComponent } from './features/inventory/pages/item-list-page.component';
import { ItemDetailPageComponent } from './features/inventory/pages/item-detail-page.component';
import { BrandListPageComponent } from './features/inventory/pages/brand-list-page.component';
import { CategoryListPageComponent } from './features/inventory/pages/category-list-page.component';
import { TagListPageComponent } from './features/inventory/pages/tag-list-page.component';
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
    path: 'inventory/brands',
    component: BrandListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/categories',
    component: CategoryListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/tags',
    component: TagListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
