import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, inject, effect } from '@angular/core';
import { AuthService } from '../auth.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private requiredPermissions: string[] = [];

  @Input() set hasPermission(permissions: string | string[]) {
    this.requiredPermissions = Array.isArray(permissions) ? permissions : [permissions];
    this.updateView();
  }

  constructor() {
    effect(() => {
      // Re-run when permissions signal changes
      this.authService.permissions();
      this.updateView();
    });
  }

  ngOnInit() {
    this.updateView();
  }

  private updateView() {
    const hasPermission = this.authService.hasAnyPermission(this.requiredPermissions);

    if (hasPermission) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
