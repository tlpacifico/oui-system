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
import { ReceptionDetailPageComponent } from './features/inventory/pages/reception-detail-page.component';
import { ReturnListPageComponent } from './features/inventory/pages/return-list-page.component';
import { ReturnItemsPageComponent } from './features/inventory/pages/return-items-page.component';
import { ReturnDetailPageComponent } from './features/inventory/pages/return-detail-page.component';
import { PosRegisterPageComponent } from './features/pos/pos-register-page.component';
import { PosSalePageComponent } from './features/pos/pos-sale-page.component';
import { PosSalesListPageComponent } from './features/pos/pos-sales-list-page.component';
import { RoleListPageComponent } from './features/admin/pages/role-list-page.component';
import { RoleDetailPageComponent } from './features/admin/pages/role-detail-page.component';
import { PermissionListPageComponent } from './features/admin/pages/permission-list-page.component';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard, anyPermissionGuard } from './core/auth/permission.guard';

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
    path: 'consignments/receptions/:id',
    component: ReceptionDetailPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/returns',
    component: ReturnListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/returns/new',
    component: ReturnItemsPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'consignments/returns/:id',
    component: ReturnDetailPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'pos',
    component: PosRegisterPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'pos/sale',
    component: PosSalePageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'pos/sales',
    component: PosSalesListPageComponent,
    canMatch: [authGuard],
  },
  {
    path: 'admin/roles',
    component: RoleListPageComponent,
    canMatch: [authGuard, permissionGuard('admin.roles.view')],
  },
  {
    path: 'admin/roles/:id',
    component: RoleDetailPageComponent,
    canMatch: [authGuard, permissionGuard('admin.roles.view')],
  },
  {
    path: 'admin/permissions',
    component: PermissionListPageComponent,
    canMatch: [authGuard, permissionGuard('admin.permissions.view')],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
