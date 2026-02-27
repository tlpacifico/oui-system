import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { AuthService } from '../auth.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private requiredPermissions: string[] = [];
  private isRendered = false;

  @Input() set hasPermission(permissions: string | string[]) {
    this.requiredPermissions = Array.isArray(permissions) ? permissions : [permissions];
  }

  constructor() {
    effect(() => {
      const hasPermission = this.authService.hasAnyPermission(this.requiredPermissions);

      if (hasPermission && !this.isRendered) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.isRendered = true;
      } else if (!hasPermission && this.isRendered) {
        this.viewContainer.clear();
        this.isRendered = false;
      }
    });
  }
}
