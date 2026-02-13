import { Routes } from '@angular/router';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { ItemListPageComponent } from './features/inventory/pages/item-list-page.component';
import { ItemDetailPageComponent } from './features/inventory/pages/item-detail-page.component';
import { ItemFormPageComponent } from './features/inventory/pages/item-form-page.component';
import { BrandListPageComponent } from './features/inventory/pages/brand-list-page.component';
import { CategoryListPageComponent } from './features/inventory/pages/category-list-page.component';
import { TagListPageComponent } from './features/inventory/pages/tag-list-page.component';
import { SupplierListPageComponent } from './features/inventory/pages/supplier-list-page.component';
import { SupplierDetailPageComponent } from './features/inventory/pages/supplier-detail-page.component';
import { ReceptionListPageComponent } from './features/inventory/pages/reception-list-page.component';
import { ReceptionReceivePageComponent } from './features/inventory/pages/reception-receive-page.component';
import { PendingEvaluationsPageComponent } from './features/inventory/pages/pending-evaluations-page.component';
import { ReceptionEvaluatePageComponent } from './features/inventory/pages/reception-evaluate-page.component';
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
    path: 'inventory/items/new',
    component: ItemFormPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/items/:id/edit',
    component: ItemFormPageComponent,
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
    path: 'inventory/suppliers',
    component: SupplierListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'inventory/suppliers/:id',
    component: SupplierDetailPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/receptions',
    component: ReceptionListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/receive',
    component: ReceptionReceivePageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/pending-evaluations',
    component: PendingEvaluationsPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/receptions/:id/evaluate',
    component: ReceptionEvaluatePageComponent,
    canMatch: [authGuard],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
