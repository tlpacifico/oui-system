import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SystemSettingService } from '../services/system-setting.service';
import { SystemSetting, SystemSettingGroup } from '../../../core/models/system-setting.model';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'oui-system-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h1>Configurações do Sistema</h1>
      <p class="subtitle">Gerir feature flags e configurações gerais do sistema</p>
    </div>

    @if (loading()) {
      <div class="loading">A carregar configurações...</div>
    }

    @if (errorMessage()) {
      <div class="alert alert-error">{{ errorMessage() }}</div>
    }

    @for (group of groups(); track group.module) {
      <div class="settings-card">
        <div class="card-header">
          <h2>{{ getModuleLabel(group.module) }}</h2>
        </div>
        <div class="card-body">
          @for (setting of group.settings; track setting.key) {
            <div class="setting-row">
              <div class="setting-info">
                <span class="setting-name">{{ setting.displayName }}</span>
                @if (setting.description) {
                  <span class="setting-description">{{ setting.description }}</span>
                }
              </div>
              <div class="setting-control">
                @if (setting.valueType === 'bool') {
                  @if (canEdit()) {
                    <label class="toggle">
                      <input
                        type="checkbox"
                        [checked]="setting.value === 'true'"
                        (change)="onToggle(setting, $event)"
                      />
                      <span class="toggle-slider"></span>
                    </label>
                  } @else {
                    <span class="bool-value">{{ setting.value === 'true' ? 'Ativo' : 'Inativo' }}</span>
                  }
                } @else if (setting.valueType === 'decimal' || setting.valueType === 'int') {
                  @if (canEdit()) {
                    <div class="input-group">
                      <input
                        type="number"
                        class="input-number"
                        [value]="setting.value"
                        [step]="setting.valueType === 'decimal' ? '0.01' : '1'"
                        (blur)="onValueChange(setting, $event)"
                        (keyup.enter)="onValueChange(setting, $event)"
                      />
                      @if (setting.key.includes('percentage')) {
                        <span class="input-suffix">%</span>
                      }
                    </div>
                  } @else {
                    <span class="readonly-value">{{ setting.value }}{{ setting.key.includes('percentage') ? '%' : '' }}</span>
                  }
                } @else {
                  @if (canEdit()) {
                    <input
                      type="text"
                      class="input-text"
                      [value]="setting.value"
                      (blur)="onValueChange(setting, $event)"
                      (keyup.enter)="onValueChange(setting, $event)"
                    />
                  } @else {
                    <span class="readonly-value">{{ setting.value }}</span>
                  }
                }
                @if (savingKey() === setting.key) {
                  <span class="saving-indicator">A guardar...</span>
                }
                @if (savedKey() === setting.key) {
                  <span class="saved-indicator">Guardado</span>
                }
              </div>
            </div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header {
      margin-bottom: 24px;
    }
    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #1e1b4b;
      margin: 0 0 4px 0;
    }
    .subtitle {
      color: #6b7280;
      font-size: 0.875rem;
      margin: 0;
    }
    .loading {
      text-align: center;
      padding: 40px;
      color: #6b7280;
    }
    .alert-error {
      background: #fef2f2;
      color: #991b1b;
      padding: 12px 16px;
      border-radius: 8px;
      margin-bottom: 16px;
      border: 1px solid #fecaca;
    }
    .settings-card {
      background: #fff;
      border-radius: 12px;
      border: 1px solid #e5e7eb;
      margin-bottom: 20px;
      overflow: hidden;
    }
    .card-header {
      background: #f9fafb;
      padding: 16px 20px;
      border-bottom: 1px solid #e5e7eb;
    }
    .card-header h2 {
      margin: 0;
      font-size: 1rem;
      font-weight: 600;
      color: #374151;
      text-transform: capitalize;
    }
    .card-body {
      padding: 0;
    }
    .setting-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 20px;
      border-bottom: 1px solid #f3f4f6;
      gap: 16px;
    }
    .setting-row:last-child {
      border-bottom: none;
    }
    .setting-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
      flex: 1;
      min-width: 0;
    }
    .setting-name {
      font-weight: 500;
      color: #111827;
      font-size: 0.875rem;
    }
    .setting-description {
      color: #9ca3af;
      font-size: 0.75rem;
    }
    .setting-control {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-shrink: 0;
    }

    /* Toggle switch */
    .toggle {
      position: relative;
      display: inline-block;
      width: 44px;
      height: 24px;
      cursor: pointer;
    }
    .toggle input {
      opacity: 0;
      width: 0;
      height: 0;
    }
    .toggle-slider {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background-color: #d1d5db;
      border-radius: 24px;
      transition: background-color 0.2s;
    }
    .toggle-slider::before {
      content: '';
      position: absolute;
      height: 18px;
      width: 18px;
      left: 3px;
      bottom: 3px;
      background-color: white;
      border-radius: 50%;
      transition: transform 0.2s;
    }
    .toggle input:checked + .toggle-slider {
      background-color: #6366f1;
    }
    .toggle input:checked + .toggle-slider::before {
      transform: translateX(20px);
    }
    .bool-value {
      font-size: 0.8rem;
      color: #6b7280;
    }

    /* Number / text inputs */
    .input-group {
      display: flex;
      align-items: center;
      gap: 4px;
    }
    .input-number, .input-text {
      width: 100px;
      padding: 6px 10px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 0.875rem;
      color: #111827;
      outline: none;
      transition: border-color 0.15s;
    }
    .input-number:focus, .input-text:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.15);
    }
    .input-suffix {
      color: #6b7280;
      font-size: 0.875rem;
    }
    .readonly-value {
      font-size: 0.875rem;
      color: #374151;
      font-weight: 500;
    }
    .saving-indicator {
      font-size: 0.75rem;
      color: #6366f1;
    }
    .saved-indicator {
      font-size: 0.75rem;
      color: #059669;
    }
  `]
})
export class SystemSettingsPageComponent implements OnInit {
  private readonly settingService = inject(SystemSettingService);
  private readonly authService = inject(AuthService);

  canEdit = computed(() => this.authService.hasAnyPermission(['admin.settings.update']));

  groups = signal<SystemSettingGroup[]>([]);
  loading = signal(false);
  errorMessage = signal('');
  savingKey = signal('');
  savedKey = signal('');

  private savedTimeout: any;

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.settingService.getAll().subscribe({
      next: (groups) => {
        this.groups.set(groups);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Erro ao carregar configurações.');
        this.loading.set(false);
      }
    });
  }

  getModuleLabel(module: string): string {
    const labels: Record<string, string> = {
      general: 'Geral',
      consignment: 'Consignação',
      pos: 'Ponto de Venda',
      financial: 'Financeiro',
      inventory: 'Inventário',
    };
    return labels[module] || module;
  }

  onToggle(setting: SystemSetting, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.saveSetting(setting, checked.toString());
  }

  onValueChange(setting: SystemSetting, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (value !== setting.value) {
      this.saveSetting(setting, value);
    }
  }

  private saveSetting(setting: SystemSetting, newValue: string): void {
    this.savingKey.set(setting.key);
    this.savedKey.set('');
    clearTimeout(this.savedTimeout);

    this.settingService.update(setting.key, { value: newValue }).subscribe({
      next: () => {
        setting.value = newValue;
        this.savingKey.set('');
        this.savedKey.set(setting.key);
        this.savedTimeout = setTimeout(() => this.savedKey.set(''), 2000);
      },
      error: () => {
        this.savingKey.set('');
        this.errorMessage.set(`Erro ao guardar "${setting.displayName}".`);
      }
    });
  }
}
