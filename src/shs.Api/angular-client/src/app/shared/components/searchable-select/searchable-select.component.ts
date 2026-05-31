import { Component, ElementRef, HostListener, computed, inject, input, model, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface SearchableOption {
  value: string;
  label: string;
  sublabel?: string;
  badge?: string;
}

@Component({
  selector: 'oui-searchable-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="ss-wrapper">
      <div class="ss-control" [class.input-error]="invalid()">
        @if (selectedOption(); as opt) {
          <div class="ss-selected">
            @if (opt.badge) {
              <span class="initial-badge-sm">{{ opt.badge }}</span>
            }
            <div class="dropdown-item-info">
              <span class="dropdown-item-name">{{ opt.label }}</span>
              @if (opt.sublabel) {
                <span class="dropdown-item-detail">{{ opt.sublabel }}</span>
              }
            </div>
            @if (clearable()) {
              <button type="button" class="btn-clear" (click)="clear($event)">&times;</button>
            }
          </div>
        } @else {
          <input
            #searchInput
            type="text"
            class="ss-input"
            [placeholder]="placeholder()"
            [(ngModel)]="search"
            (ngModelChange)="onSearchChange($event)"
            (focus)="open()"
            autocomplete="off"
          />
        }
      </div>

      @if (isOpen() && !selectedOption()) {
        <div class="dropdown">
          @for (opt of filtered(); track opt.value) {
            <div class="dropdown-item" (click)="select(opt)">
              @if (opt.badge) {
                <span class="initial-badge-sm">{{ opt.badge }}</span>
              }
              <div class="dropdown-item-info">
                <span class="dropdown-item-name">{{ opt.label }}</span>
                @if (opt.sublabel) {
                  <span class="dropdown-item-detail">{{ opt.sublabel }}</span>
                }
              </div>
            </div>
          }
          @if (filtered().length === 0) {
            <div class="dropdown-empty">Sem resultados.</div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }

    .ss-wrapper { position: relative; }

    .ss-control {
      width: 100%;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      background: white;
      transition: border-color 0.15s;
    }

    .ss-control:focus-within {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .ss-control.input-error {
      border-color: #ef4444;
    }

    .ss-input {
      width: 100%;
      padding: 10px 12px;
      border: none;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      color: #1e293b;
      font-family: inherit;
      background: transparent;
    }

    .ss-selected {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 12px;
    }

    /* ── Dropdown ── */
    .dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: white;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      z-index: 50;
      max-height: 260px;
      overflow-y: auto;
      margin-top: 4px;
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 14px;
      cursor: pointer;
      transition: background 0.1s;
    }

    .dropdown-item:hover { background: #f1f5f9; }
    .dropdown-item:first-child { border-radius: 10px 10px 0 0; }
    .dropdown-item:last-child { border-radius: 0 0 10px 10px; }

    .dropdown-empty {
      padding: 12px 14px;
      font-size: 13px;
      color: #94a3b8;
    }

    .initial-badge-sm {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: #6366f1;
      color: white;
      font-size: 11px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .dropdown-item-info { display: flex; flex-direction: column; flex: 1; min-width: 0; }

    .dropdown-item-name {
      font-size: 14px;
      font-weight: 600;
      color: #1e293b;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .dropdown-item-detail {
      font-size: 12px;
      color: #64748b;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .btn-clear {
      background: none;
      border: none;
      font-size: 22px;
      color: #94a3b8;
      cursor: pointer;
      padding: 0 4px;
      line-height: 1;
      flex-shrink: 0;
    }

    .btn-clear:hover { color: #ef4444; }
  `]
})
export class SearchableSelectComponent {
  private readonly host = inject(ElementRef<HTMLElement>);

  options = input<SearchableOption[]>([]);
  value = model<string>('');
  placeholder = input('Selecionar...');
  invalid = input(false);
  clearable = input(true);

  search = '';
  searchText = signal('');
  isOpen = signal(false);

  selectedOption = computed(() => {
    const v = this.value();
    return v ? this.options().find(o => o.value === v) ?? null : null;
  });

  filtered = computed(() => {
    const term = this.searchText().trim().toLowerCase();
    if (!term) return this.options();
    return this.options().filter(o =>
      o.label.toLowerCase().includes(term) ||
      (o.sublabel?.toLowerCase().includes(term) ?? false)
    );
  });

  open(): void {
    this.isOpen.set(true);
  }

  onSearchChange(term: string): void {
    this.searchText.set(term);
    this.isOpen.set(true);
  }

  select(opt: SearchableOption): void {
    this.value.set(opt.value);
    this.search = '';
    this.searchText.set('');
    this.isOpen.set(false);
  }

  clear(event: Event): void {
    event.stopPropagation();
    this.value.set('');
    this.search = '';
    this.searchText.set('');
    this.isOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }
}
