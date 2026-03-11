import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportService } from '../services/import.service';
import { ImportResult } from '../../../core/models/import.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

type ImportType = 'personal' | 'consignment';

@Component({
  selector: 'oui-import-page',
  standalone: true,
  imports: [CommonModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Importação de Dados</h1>
        <p class="page-subtitle">Importe itens a partir de ficheiros Excel (.xlsx)</p>
      </div>
    </div>

    <div class="import-grid" *hasPermission="'admin.import.execute'">
      <!-- Personal Items Card -->
      <div class="card import-card">
        <div class="card-header">
          <span class="card-icon">📦</span>
          <div>
            <h2 class="card-title">Itens Pessoais (Estoque)</h2>
            <p class="card-description">Importar itens pessoais da loja a partir de uma planilha Excel</p>
          </div>
        </div>
        <div class="card-body">
          <div class="file-format-info">
            <span class="badge badge-blue">.xlsx</span>
            <span class="file-limit">Máx. 20 MB</span>
          </div>

          <div
            class="upload-zone"
            [class.drag-over]="personalDragOver()"
            (dragover)="onDragOver($event, 'personal')"
            (dragleave)="onDragLeave('personal')"
            (drop)="onDrop($event, 'personal')"
            (click)="personalFileInput.click()"
          >
            <input
              #personalFileInput
              type="file"
              accept=".xlsx"
              class="file-input-hidden"
              (change)="onFileSelected($event, 'personal')"
            />
            @if (personalFile()) {
              <div class="file-info">
                <span class="file-icon">📄</span>
                <div>
                  <span class="file-name">{{ personalFile()!.name }}</span>
                  <span class="file-size">{{ formatFileSize(personalFile()!.size) }}</span>
                </div>
                <button class="btn-remove" (click)="removeFile($event, 'personal')">&times;</button>
              </div>
            } @else {
              <span class="upload-icon">⬆️</span>
              <span class="upload-text">Clique ou arraste um ficheiro .xlsx</span>
            }
          </div>

          <button
            class="btn btn-primary btn-upload"
            [disabled]="!personalFile() || uploading()"
            (click)="upload('personal')"
          >
            {{ uploading() && activeImportType() === 'personal' ? 'A importar...' : 'Importar Itens Pessoais' }}
          </button>

          @if (personalResult()) {
            <div class="result-card" [class.result-has-errors]="personalResult()!.errors > 0">
              <h3 class="result-title">Resultado da Importação</h3>
              <div class="result-stats">
                <div class="stat">
                  <span class="stat-value">{{ personalResult()!.rowsRead }}</span>
                  <span class="stat-label">Linhas lidas</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ personalResult()!.itemsImported }}</span>
                  <span class="stat-label">Itens importados</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ personalResult()!.brandsCreated }}</span>
                  <span class="stat-label">Marcas criadas</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ personalResult()!.suppliersCreated }}</span>
                  <span class="stat-label">Fornecedores criados</span>
                </div>
                <div class="stat" [class.stat-error]="personalResult()!.errors > 0">
                  <span class="stat-value">{{ personalResult()!.errors }}</span>
                  <span class="stat-label">Erros</span>
                </div>
              </div>
              @if (personalResult()!.errorDetails.length > 0) {
                <div class="error-details">
                  <button class="btn btn-outline btn-sm" (click)="togglePersonalErrors()">
                    {{ showPersonalErrors() ? 'Ocultar erros' : 'Ver erros (' + personalResult()!.errorDetails.length + ')' }}
                  </button>
                  @if (showPersonalErrors()) {
                    <ul class="error-list">
                      @for (error of personalResult()!.errorDetails; track $index) {
                        <li>{{ error }}</li>
                      }
                    </ul>
                  }
                </div>
              }
            </div>
          }
        </div>
      </div>

      <!-- Consignment Items Card -->
      <div class="card import-card">
        <div class="card-header">
          <span class="card-icon">🤝</span>
          <div>
            <h2 class="card-title">Itens Consignados</h2>
            <p class="card-description">Importar itens em consignação a partir de uma planilha Excel</p>
          </div>
        </div>
        <div class="card-body">
          <div class="file-format-info">
            <span class="badge badge-blue">.xlsx</span>
            <span class="file-limit">Máx. 20 MB</span>
          </div>

          <div
            class="upload-zone"
            [class.drag-over]="consignmentDragOver()"
            (dragover)="onDragOver($event, 'consignment')"
            (dragleave)="onDragLeave('consignment')"
            (drop)="onDrop($event, 'consignment')"
            (click)="consignmentFileInput.click()"
          >
            <input
              #consignmentFileInput
              type="file"
              accept=".xlsx"
              class="file-input-hidden"
              (change)="onFileSelected($event, 'consignment')"
            />
            @if (consignmentFile()) {
              <div class="file-info">
                <span class="file-icon">📄</span>
                <div>
                  <span class="file-name">{{ consignmentFile()!.name }}</span>
                  <span class="file-size">{{ formatFileSize(consignmentFile()!.size) }}</span>
                </div>
                <button class="btn-remove" (click)="removeFile($event, 'consignment')">&times;</button>
              </div>
            } @else {
              <span class="upload-icon">⬆️</span>
              <span class="upload-text">Clique ou arraste um ficheiro .xlsx</span>
            }
          </div>

          <button
            class="btn btn-primary btn-upload"
            [disabled]="!consignmentFile() || uploading()"
            (click)="upload('consignment')"
          >
            {{ uploading() && activeImportType() === 'consignment' ? 'A importar...' : 'Importar Itens Consignados' }}
          </button>

          @if (consignmentResult()) {
            <div class="result-card" [class.result-has-errors]="consignmentResult()!.errors > 0">
              <h3 class="result-title">Resultado da Importação</h3>
              <div class="result-stats">
                <div class="stat">
                  <span class="stat-value">{{ consignmentResult()!.rowsRead }}</span>
                  <span class="stat-label">Linhas lidas</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ consignmentResult()!.itemsImported }}</span>
                  <span class="stat-label">Itens importados</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ consignmentResult()!.brandsCreated }}</span>
                  <span class="stat-label">Marcas criadas</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ consignmentResult()!.suppliersCreated }}</span>
                  <span class="stat-label">Fornecedores criados</span>
                </div>
                <div class="stat" [class.stat-error]="consignmentResult()!.errors > 0">
                  <span class="stat-value">{{ consignmentResult()!.errors }}</span>
                  <span class="stat-label">Erros</span>
                </div>
              </div>
              @if (consignmentResult()!.errorDetails.length > 0) {
                <div class="error-details">
                  <button class="btn btn-outline btn-sm" (click)="toggleConsignmentErrors()">
                    {{ showConsignmentErrors() ? 'Ocultar erros' : 'Ver erros (' + consignmentResult()!.errorDetails.length + ')' }}
                  </button>
                  @if (showConsignmentErrors()) {
                    <ul class="error-list">
                      @for (error of consignmentResult()!.errorDetails; track $index) {
                        <li>{{ error }}</li>
                      }
                    </ul>
                  }
                </div>
              }
            </div>
          }
        </div>
      </div>
    </div>

    @if (errorMessage()) {
      <div class="alert alert-danger">{{ errorMessage() }}</div>
    }
  `,
  styles: [`
    :host { display: block; }

    .import-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .import-card {
      display: flex;
      flex-direction: column;
    }

    .card-header {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 20px 20px 16px;
      border-bottom: 1px solid #e2e8f0;
    }

    .card-icon {
      font-size: 22px;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f1f5f9;
      border-radius: 10px;
      flex-shrink: 0;
    }

    .card-title {
      font-size: 16px;
      font-weight: 700;
      color: #1e293b;
      margin: 0 0 4px;
    }

    .card-description {
      font-size: 13px;
      color: #64748b;
      margin: 0;
    }

    .card-body {
      padding: 20px;
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .file-format-info {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .file-limit {
      font-size: 12px;
      color: #94a3b8;
    }

    .upload-zone {
      border: 2px dashed #e2e8f0;
      border-radius: 10px;
      padding: 24px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 8px;
      cursor: pointer;
      transition: all 0.15s;
      min-height: 100px;
    }

    .upload-zone:hover {
      border-color: #6366f1;
      background: #fafafe;
    }

    .upload-zone.drag-over {
      border-color: #6366f1;
      background: #eef2ff;
    }

    .file-input-hidden { display: none; }
    .upload-icon { font-size: 24px; }
    .upload-text { font-size: 13px; color: #64748b; }

    .file-info {
      display: flex;
      align-items: center;
      gap: 10px;
      width: 100%;
    }

    .file-icon { font-size: 20px; }

    .file-name {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #1e293b;
      word-break: break-all;
    }

    .file-size {
      display: block;
      font-size: 12px;
      color: #94a3b8;
    }

    .btn-remove {
      margin-left: auto;
      background: none;
      border: none;
      font-size: 20px;
      color: #94a3b8;
      cursor: pointer;
      padding: 4px 8px;
      line-height: 1;
    }

    .btn-remove:hover { color: #ef4444; }

    .btn-upload {
      width: 100%;
      justify-content: center;
      padding: 10px 16px;
    }

    .badge-blue { background: #eef2ff; color: #6366f1; }

    .result-card {
      background: #f0fdf4;
      border: 1px solid #bbf7d0;
      border-radius: 10px;
      padding: 16px;
    }

    .result-card.result-has-errors {
      background: #fffbeb;
      border-color: #fde68a;
    }

    .result-title {
      font-size: 14px;
      font-weight: 700;
      color: #1e293b;
      margin: 0 0 12px;
    }

    .result-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
      gap: 12px;
    }

    .stat { text-align: center; }

    .stat-value {
      display: block;
      font-size: 20px;
      font-weight: 700;
      color: #1e293b;
    }

    .stat-label {
      display: block;
      font-size: 11px;
      color: #64748b;
      margin-top: 2px;
    }

    .stat-error .stat-value { color: #ef4444; }

    .error-details {
      margin-top: 12px;
      padding-top: 12px;
      border-top: 1px solid #fde68a;
    }

    .error-list {
      margin: 10px 0 0;
      padding: 0 0 0 18px;
      font-size: 12px;
      color: #92400e;
      max-height: 200px;
      overflow-y: auto;
    }

    .error-list li { margin-bottom: 4px; }

    .alert-danger { margin-top: 16px; }

    @media (max-width: 768px) {
      .import-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ImportPageComponent {
  private readonly importService = inject(ImportService);

  readonly uploading = signal(false);
  readonly activeImportType = signal<ImportType | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly personalFile = signal<File | null>(null);
  readonly personalResult = signal<ImportResult | null>(null);
  readonly personalDragOver = signal(false);
  readonly showPersonalErrors = signal(false);

  readonly consignmentFile = signal<File | null>(null);
  readonly consignmentResult = signal<ImportResult | null>(null);
  readonly consignmentDragOver = signal(false);
  readonly showConsignmentErrors = signal(false);

  onFileSelected(event: Event, type: ImportType) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    if (file) this.setFile(file, type);
    input.value = '';
  }

  onDragOver(event: DragEvent, type: ImportType) {
    event.preventDefault();
    event.stopPropagation();
    if (type === 'personal') this.personalDragOver.set(true);
    else this.consignmentDragOver.set(true);
  }

  onDragLeave(type: ImportType) {
    if (type === 'personal') this.personalDragOver.set(false);
    else this.consignmentDragOver.set(false);
  }

  onDrop(event: DragEvent, type: ImportType) {
    event.preventDefault();
    event.stopPropagation();
    this.onDragLeave(type);
    const file = event.dataTransfer?.files[0] ?? null;
    if (file) this.setFile(file, type);
  }

  removeFile(event: Event, type: ImportType) {
    event.stopPropagation();
    if (type === 'personal') {
      this.personalFile.set(null);
      this.personalResult.set(null);
      this.showPersonalErrors.set(false);
    } else {
      this.consignmentFile.set(null);
      this.consignmentResult.set(null);
      this.showConsignmentErrors.set(false);
    }
  }

  upload(type: ImportType) {
    const file = type === 'personal' ? this.personalFile() : this.consignmentFile();
    if (!file) return;

    this.uploading.set(true);
    this.activeImportType.set(type);
    this.errorMessage.set(null);

    const obs = type === 'personal'
      ? this.importService.importPersonalItems(file)
      : this.importService.importConsignmentItems(file);

    obs.subscribe({
      next: (result) => {
        if (type === 'personal') {
          this.personalResult.set(result);
          this.showPersonalErrors.set(false);
        } else {
          this.consignmentResult.set(result);
          this.showConsignmentErrors.set(false);
        }
        this.uploading.set(false);
        this.activeImportType.set(null);
      },
      error: (err) => {
        this.uploading.set(false);
        this.activeImportType.set(null);
        this.errorMessage.set(
          err.error?.message || err.error?.title || 'Erro ao importar ficheiro. Verifique o formato e tente novamente.'
        );
      }
    });
  }

  togglePersonalErrors() {
    this.showPersonalErrors.update(v => !v);
  }

  toggleConsignmentErrors() {
    this.showConsignmentErrors.update(v => !v);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  private setFile(file: File, type: ImportType) {
    if (!file.name.endsWith('.xlsx')) {
      this.errorMessage.set('Formato inválido. Apenas ficheiros .xlsx são aceites.');
      return;
    }
    if (file.size > 20 * 1024 * 1024) {
      this.errorMessage.set('Ficheiro demasiado grande. O tamanho máximo é 20 MB.');
      return;
    }
    this.errorMessage.set(null);
    if (type === 'personal') {
      this.personalFile.set(file);
      this.personalResult.set(null);
    } else {
      this.consignmentFile.set(file);
      this.consignmentResult.set(null);
    }
  }
}
