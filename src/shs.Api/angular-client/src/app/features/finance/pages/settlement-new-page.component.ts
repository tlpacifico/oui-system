import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FinanceService, PendingSettlementGroup, CalculateSettlementResponse } from '../finance.service';

type Step = 1 | 2 | 3;

@Component({
  selector: 'oui-settlement-new-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page">
      <div class="detail-topbar">
        <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
      </div>

      <h1>Novo Acerto</h1>
      <p class="subtitle">Processar acerto com fornecedor consignante</p>

      <!-- Step indicator -->
      <div class="steps">
        <div class="step" [class.active]="currentStep() >= 1" [class.done]="currentStep() > 1">
          <span class="step-num">1</span> Fornecedor e Período
        </div>
        <div class="step" [class.active]="currentStep() >= 2" [class.done]="currentStep() > 2">
          <span class="step-num">2</span> Resumo
        </div>
        <div class="step" [class.active]="currentStep() >= 3">
          <span class="step-num">3</span> Confirmar
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else {
        <!-- Step 1: Select supplier and period -->
        @if (currentStep() === 1) {
          <div class="card form-card">
            <div class="form-group">
              <label>Fornecedor *</label>
              <select
                [(ngModel)]="selectedSupplierId"
                (ngModelChange)="onSupplierChange()"
                class="form-control"
              >
                <option [ngValue]="null">Selecione o fornecedor</option>
                @for (g of pendingGroups(); track g.supplierId) {
                  <option [ngValue]="g.supplierId">
                    {{ g.supplierInitial }} – {{ g.supplierName }} ({{ g.itemCount }} itens, {{ g.totalSalesAmount | currency: 'EUR' }})
                  </option>
                }
              </select>
            </div>
            @if (selectedSupplierId != null) {
              <div class="form-row">
                <div class="form-group">
                  <label>Data início *</label>
                  <input
                    type="date"
                    [(ngModel)]="periodStart"
                    (ngModelChange)="onPeriodChange()"
                    class="form-control"
                  />
                </div>
                <div class="form-group">
                  <label>Data fim *</label>
                  <input
                    type="date"
                    [(ngModel)]="periodEnd"
                    (ngModelChange)="onPeriodChange()"
                    class="form-control"
                  />
                </div>
              </div>
              <div class="form-group">
                <label>Notas (opcional)</label>
                <textarea
                  [(ngModel)]="notes"
                  class="form-control"
                  rows="2"
                  placeholder="Notas internas sobre este acerto..."
                ></textarea>
              </div>
              <button
                class="btn btn-primary"
                [disabled]="!canProceedStep1()"
                (click)="calculateAndProceed()"
              >
                Calcular e Continuar
              </button>
            }
          </div>
        }

        <!-- Step 2: Review breakdown -->
        @if (currentStep() === 2 && preview()) {
          <div class="card">
            <div class="card-title">Resumo do Acerto</div>
            <div class="summary-grid">
              <div class="summary-row">
                <span>Fornecedor</span>
                <strong>{{ preview()!.supplierName }}</strong>
              </div>
              <div class="summary-row">
                <span>Período</span>
                <strong>{{ formatPeriod(preview()!.periodStart, preview()!.periodEnd) }}</strong>
              </div>
              <div class="summary-row">
                <span>Itens vendidos</span>
                <strong>{{ preview()!.itemCount }}</strong>
              </div>
              <div class="summary-row">
                <span>Total vendas</span>
                <strong>{{ preview()!.totalSalesAmount | currency: 'EUR' }}</strong>
              </div>
              <div class="summary-row">
                <span>Crédito em loja ({{ preview()!.creditPercentageInStore }}%)</span>
                <strong>{{ preview()!.storeCreditAmount | currency: 'EUR' }}</strong>
              </div>
              <div class="summary-row">
                <span>Resgate em dinheiro ({{ preview()!.cashRedemptionPercentage }}%)</span>
                <strong>{{ preview()!.cashRedemptionAmount | currency: 'EUR' }}</strong>
              </div>
              <div class="summary-row highlight">
                <span>Total a pagar ao fornecedor</span>
                <strong>{{ preview()!.netAmountToSupplier | currency: 'EUR' }}</strong>
              </div>
              <div class="summary-row">
                <span>Comissão da loja</span>
                <strong>{{ preview()!.storeCommissionAmount | currency: 'EUR' }}</strong>
              </div>
            </div>
            <div class="form-actions">
              <button class="btn btn-outline" (click)="currentStep.set(1)">Voltar</button>
              <button class="btn btn-primary" (click)="currentStep.set(3)">Confirmar e Criar</button>
            </div>
          </div>
        }

        <!-- Step 3: Confirm and create -->
        @if (currentStep() === 3 && preview()) {
          <div class="card">
            <div class="card-title">Confirmar criação do acerto</div>
            <p>
              Será criado um acerto para <strong>{{ preview()!.supplierName }}</strong>
              referente ao período {{ formatPeriod(preview()!.periodStart, preview()!.periodEnd) }},
              com {{ preview()!.itemCount }} itens e total a pagar de
              <strong>{{ preview()!.netAmountToSupplier | currency: 'EUR' }}</strong>.
            </p>
            <p class="text-muted">
              Após criar, poderá processar o pagamento (emitir crédito em loja e registar saldo para resgate em dinheiro)
              na página de detalhe do acerto.
            </p>
            <div class="form-actions">
              <button class="btn btn-outline" (click)="currentStep.set(2)">Voltar</button>
              <button
                class="btn btn-primary"
                [disabled]="creating()"
                (click)="createSettlement()"
              >
                {{ creating() ? 'A criar...' : 'Criar Acerto' }}
              </button>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page { max-width: 640px; margin: 0 auto; }
    .detail-topbar { margin-bottom: 20px; }
    .subtitle { color: #64748b; margin: 4px 0 24px; font-size: 14px; }
    .steps {
      display: flex; gap: 16px; margin-bottom: 32px;
      padding: 16px; background: #f8fafc; border-radius: 8px;
    }
    .step {
      display: flex; align-items: center; gap: 8px;
      font-size: 14px; color: #94a3b8;
    }
    .step.active { color: #0f172a; font-weight: 500; }
    .step.done { color: #059669; }
    .step-num {
      width: 24px; height: 24px; border-radius: 50%;
      background: #e2e8f0; color: #64748b;
      display: inline-flex; align-items: center; justify-content: center;
      font-size: 12px; font-weight: 600;
    }
    .step.active .step-num { background: #0f172a; color: #fff; }
    .step.done .step-num { background: #059669; color: #fff; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .form-card { max-width: 480px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 6px; font-size: 14px; }
    .form-control {
      width: 100%; padding: 10px 12px; border: 1px solid #e2e8f0;
      border-radius: 6px; font-size: 14px;
    }
    .form-row { display: flex; gap: 16px; }
    .form-row .form-group { flex: 1; }
    .form-actions { display: flex; gap: 12px; margin-top: 24px; }
    .summary-grid { display: flex; flex-direction: column; gap: 12px; margin: 16px 0; }
    .summary-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 8px 0; border-bottom: 1px solid #f1f5f9;
    }
    .summary-row.highlight {
      font-size: 16px; border-bottom: none;
      margin-top: 8px; padding-top: 16px;
      border-top: 2px solid #0f172a;
    }
    .text-muted { color: #64748b; font-size: 14px; margin-top: 16px; }
  `],
})
export class SettlementNewPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  currentStep = signal<Step>(1);
  loading = signal(true);
  creating = signal(false);
  pendingGroups = signal<PendingSettlementGroup[]>([]);
  preview = signal<CalculateSettlementResponse | null>(null);

  selectedSupplierId: number | null = null;
  periodStart = '';
  periodEnd = '';
  notes = '';

  canProceedStep1 = computed(() => {
    if (!this.selectedSupplierId || !this.periodStart || !this.periodEnd) return false;
    const start = new Date(this.periodStart);
    const end = new Date(this.periodEnd);
    return start <= end;
  });

  ngOnInit(): void {
    this.finance.getPendingSettlementItems().subscribe({
      next: (groups) => {
        this.pendingGroups.set(groups);
        const supplierId = this.route.snapshot.queryParams['supplierId'];
        if (supplierId) {
          const id = Number(supplierId);
          if (groups.some((g) => g.supplierId === id)) {
            this.selectedSupplierId = id;
            this.setDefaultPeriodForSupplier(id);
          }
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private setDefaultPeriodForSupplier(supplierId: number): void {
    const group = this.pendingGroups().find((g) => g.supplierId === supplierId);
    if (group && group.items.length > 0) {
      const dates = group.items.map((i) => new Date(i.updatedOn));
      const min = new Date(Math.min(...dates.map((d) => d.getTime())));
      const max = new Date(Math.max(...dates.map((d) => d.getTime())));
      this.periodStart = min.toISOString().slice(0, 10);
      this.periodEnd = max.toISOString().slice(0, 10);
    } else {
      const now = new Date();
      this.periodStart = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10);
      this.periodEnd = now.toISOString().slice(0, 10);
    }
  }

  onSupplierChange(): void {
    if (this.selectedSupplierId != null) {
      this.setDefaultPeriodForSupplier(this.selectedSupplierId);
    }
  }

  onPeriodChange(): void {
    this.preview.set(null);
  }

  calculateAndProceed(): void {
    if (!this.selectedSupplierId || !this.periodStart || !this.periodEnd) return;
    this.loading.set(true);
    this.finance
      .calculateSettlement({
        supplierId: this.selectedSupplierId,
        periodStart: this.periodStart,
        periodEnd: this.periodEnd,
      })
      .subscribe({
        next: (res) => {
          this.preview.set(res);
          this.currentStep.set(2);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          alert(err.error?.error ?? 'Erro ao calcular acerto.');
        },
      });
  }

  createSettlement(): void {
    if (!this.selectedSupplierId || !this.periodStart || !this.periodEnd || !this.preview()) return;
    this.creating.set(true);
    this.finance
      .createSettlement({
        supplierId: this.selectedSupplierId,
        periodStart: this.periodStart,
        periodEnd: this.periodEnd,
        notes: this.notes || undefined,
      })
      .subscribe({
        next: (res) => {
          this.creating.set(false);
          this.router.navigate(['/finance/settlements', res.externalId]);
        },
        error: (err) => {
          this.creating.set(false);
          alert(err.error?.error ?? 'Erro ao criar acerto.');
        },
      });
  }

  formatPeriod(start: string, end: string): string {
    const s = new Date(start);
    const e = new Date(end);
    return `${s.toLocaleDateString('pt-PT')} – ${e.toLocaleDateString('pt-PT')}`;
  }

  goBack(): void {
    this.router.navigate(['/finance/settlements']);
  }
}
