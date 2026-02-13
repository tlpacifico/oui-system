import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ReceptionService } from '../services/reception.service';
import { BrandService } from '../services/brand.service';
import { CategoryService } from '../services/category.service';
import { TagService } from '../services/tag.service';
import { ReceptionDetail, EvaluationItemResponse, AddEvaluationItemRequest } from '../../../core/models/reception.model';

interface SelectOption {
  externalId: string;
  name: string;
  color?: string;
}

@Component({
  selector: 'oui-reception-evaluate-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Avaliar Recepção</h1>
        <p class="page-subtitle">Avaliar individualmente cada peça recebida</p>
      </div>
      <div class="page-header-actions">
        <a class="btn btn-outline" routerLink="/consignments/pending-evaluations">Voltar</a>
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (error()) {
      <div class="state-message">{{ error() }}</div>
    } @else if (reception()) {

      <!-- Reception info header -->
      <div class="card info-card">
        <div class="info-top">
          <div class="supplier-info">
            <span class="initial-badge-lg">{{ reception()!.supplier.initial }}</span>
            <div>
              <span class="supplier-name">{{ reception()!.supplier.name }}</span>
              <span class="reception-meta">
                {{ reception()!.receptionDate | date: 'dd/MM/yyyy HH:mm' }}
                · Ref: {{ reception()!.externalId.substring(0, 8).toUpperCase() }}
              </span>
            </div>
          </div>
          <span class="badge" [ngClass]="reception()!.status === 'Evaluated' ? 'badge-blue' : 'badge-yellow'">
            {{ reception()!.status === 'Evaluated' ? 'Avaliada' : 'Pendente Avaliação' }}
          </span>
        </div>

        <!-- Progress -->
        <div class="progress-section">
          <div class="progress-stats">
            <span class="progress-text">
              <b>{{ items().length }}</b> de <b>{{ reception()!.itemCount }}</b> peças avaliadas
            </span>
            <span class="progress-percent">{{ progressPercent() | number: '1.0-0' }}%</span>
          </div>
          <div class="progress-bar-container">
            <div class="progress-bar" [style.width.%]="progressPercent()"></div>
          </div>
          <div class="progress-summary">
            @if (acceptedCount() > 0) {
              <span class="badge badge-green">{{ acceptedCount() }} aceites</span>
            }
            @if (rejectedCount() > 0) {
              <span class="badge badge-red">{{ rejectedCount() }} rejeitados</span>
            }
            @if (remainingCount() > 0) {
              <span class="badge badge-gray">{{ remainingCount() }} por avaliar</span>
            }
          </div>
        </div>
      </div>

      <!-- Evaluated items list -->
      @if (items().length > 0) {
        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Peças Avaliadas</span>
          </div>
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Marca</th>
                  <th>Tam.</th>
                  <th>Cor</th>
                  <th>Condição</th>
                  <th>Preço</th>
                  <th>Estado</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (item of items(); track item.externalId; let idx = $index) {
                  <tr [class.row-rejected]="item.isRejected">
                    <td class="cell-num">{{ idx + 1 }}</td>
                    <td class="cell-mono">{{ item.identificationNumber }}</td>
                    <td><b>{{ item.name }}</b></td>
                    <td>{{ item.brand }}</td>
                    <td>{{ item.size }}</td>
                    <td>{{ item.color }}</td>
                    <td>{{ getConditionLabel(item.condition) }}</td>
                    <td class="cell-price">{{ item.evaluatedPrice | currency: 'EUR' }}</td>
                    <td>
                      @if (item.isRejected) {
                        <span class="badge badge-red" [title]="item.rejectionReason || ''">Rejeitado</span>
                      } @else {
                        <span class="badge badge-green">Aceite</span>
                      }
                    </td>
                    <td>
                      @if (reception()!.status === 'PendingEvaluation') {
                        <button
                          class="btn btn-outline btn-sm btn-danger-outline"
                          (click)="removeItem(item)"
                          [disabled]="removingItem() === item.externalId"
                          title="Remover"
                        >
                          {{ removingItem() === item.externalId ? '...' : '✕' }}
                        </button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      <!-- Add item form — only while PendingEvaluation and items remaining -->
      @if (reception()!.status === 'PendingEvaluation' && remainingCount() > 0) {
        <div class="card form-card">
          <div class="card-title-bar">
            <span class="card-title">Avaliar Peça {{ items().length + 1 }} de {{ reception()!.itemCount }}</span>
          </div>

          @if (formError()) {
            <div class="alert alert-error">{{ formError() }}</div>
          }

          <div class="form-section">
            <div class="form-group">
              <label for="itemName">Nome *</label>
              <input
                id="itemName"
                type="text"
                [(ngModel)]="form.name"
                class="form-input"
                placeholder="Ex: Vestido floral manga longa"
                maxlength="500"
                [class.input-error]="formSubmitted && !form.name.trim()"
              />
              @if (formSubmitted && !form.name.trim()) {
                <span class="field-error">O nome é obrigatório.</span>
              }
            </div>

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="itemBrand">Marca *</label>
                <select
                  id="itemBrand"
                  [(ngModel)]="form.brandExternalId"
                  class="form-input"
                  [class.input-error]="formSubmitted && !form.brandExternalId"
                >
                  <option value="">Selecionar marca...</option>
                  @for (brand of brands(); track brand.externalId) {
                    <option [value]="brand.externalId">{{ brand.name }}</option>
                  }
                </select>
                @if (formSubmitted && !form.brandExternalId) {
                  <span class="field-error">A marca é obrigatória.</span>
                }
              </div>

              <div class="form-group form-group-grow">
                <label for="itemCategory">Categoria</label>
                <select
                  id="itemCategory"
                  [(ngModel)]="form.categoryExternalId"
                  class="form-input"
                >
                  <option value="">Sem categoria</option>
                  @for (cat of categories(); track cat.externalId) {
                    <option [value]="cat.externalId">{{ cat.name }}</option>
                  }
                </select>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="itemSize">Tamanho *</label>
                <select
                  id="itemSize"
                  [(ngModel)]="form.size"
                  class="form-input"
                  [class.input-error]="formSubmitted && !form.size"
                >
                  <option value="">Selecionar...</option>
                  <option value="XXS">XXS</option>
                  <option value="XS">XS</option>
                  <option value="S">S</option>
                  <option value="M">M</option>
                  <option value="L">L</option>
                  <option value="XL">XL</option>
                  <option value="XXL">XXL</option>
                  <option value="XXXL">XXXL</option>
                  <option value="34">34</option>
                  <option value="36">36</option>
                  <option value="38">38</option>
                  <option value="40">40</option>
                  <option value="42">42</option>
                  <option value="44">44</option>
                  <option value="46">46</option>
                  <option value="Único">Único</option>
                </select>
                @if (formSubmitted && !form.size) {
                  <span class="field-error">O tamanho é obrigatório.</span>
                }
              </div>

              <div class="form-group form-group-grow">
                <label for="itemColor">Cor *</label>
                <input
                  id="itemColor"
                  type="text"
                  [(ngModel)]="form.color"
                  class="form-input"
                  placeholder="Ex: Azul marinho"
                  maxlength="100"
                  [class.input-error]="formSubmitted && !form.color.trim()"
                />
                @if (formSubmitted && !form.color.trim()) {
                  <span class="field-error">A cor é obrigatória.</span>
                }
              </div>

              <div class="form-group form-group-grow">
                <label for="itemCondition">Condição *</label>
                <select
                  id="itemCondition"
                  [(ngModel)]="form.condition"
                  class="form-input"
                  [class.input-error]="formSubmitted && !form.condition"
                >
                  <option value="">Selecionar...</option>
                  <option value="Excellent">Excelente</option>
                  <option value="VeryGood">Muito Bom</option>
                  <option value="Good">Bom</option>
                  <option value="Fair">Razoável</option>
                  <option value="Poor">Mau</option>
                </select>
                @if (formSubmitted && !form.condition) {
                  <span class="field-error">A condição é obrigatória.</span>
                }
              </div>
            </div>

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="itemPrice">Preço de Venda (€) *</label>
                <input
                  id="itemPrice"
                  type="number"
                  [(ngModel)]="form.evaluatedPrice"
                  class="form-input"
                  placeholder="0.00"
                  min="0.01"
                  step="0.01"
                  [class.input-error]="formSubmitted && (!form.evaluatedPrice || form.evaluatedPrice <= 0)"
                />
                @if (formSubmitted && (!form.evaluatedPrice || form.evaluatedPrice <= 0)) {
                  <span class="field-error">O preço deve ser maior que zero.</span>
                }
              </div>

              <div class="form-group form-group-sm">
                <label for="itemCommission">Comissão %</label>
                <input
                  id="itemCommission"
                  type="number"
                  [(ngModel)]="form.commissionPercentage"
                  class="form-input"
                  placeholder="50"
                  min="0"
                  max="100"
                  step="1"
                />
              </div>
            </div>

            <div class="form-group">
              <label for="itemComposition">Composição</label>
              <input
                id="itemComposition"
                type="text"
                [(ngModel)]="form.composition"
                class="form-input"
                placeholder="Ex: 100% algodão"
                maxlength="500"
              />
            </div>

            <div class="form-group">
              <label for="itemDescription">Descrição</label>
              <textarea
                id="itemDescription"
                [(ngModel)]="form.description"
                class="form-input form-textarea"
                placeholder="Observações sobre a peça..."
                rows="2"
                maxlength="2000"
              ></textarea>
            </div>

            <!-- Tags -->
            @if (tags().length > 0) {
              <div class="form-group">
                <label>Tags</label>
                <div class="tags-picker">
                  @for (tag of tags(); track tag.externalId) {
                    <label
                      class="tag-chip"
                      [class.tag-selected]="isTagSelected(tag.externalId)"
                      [style.--tag-color]="tag.color || '#6366f1'"
                    >
                      <input
                        type="checkbox"
                        [checked]="isTagSelected(tag.externalId)"
                        (change)="toggleTag(tag.externalId)"
                      />
                      {{ tag.name }}
                    </label>
                  }
                </div>
              </div>
            }

            <!-- Rejection toggle -->
            <div class="rejection-section">
              <label class="rejection-toggle">
                <input type="checkbox" [(ngModel)]="form.isRejected" />
                <span class="toggle-label">Rejeitar esta peça</span>
              </label>

              @if (form.isRejected) {
                <div class="form-group rejection-reason-group">
                  <label for="rejectionReason">Motivo da rejeição</label>
                  <input
                    id="rejectionReason"
                    type="text"
                    [(ngModel)]="form.rejectionReason"
                    class="form-input"
                    placeholder="Ex: Mancha grande, tecido rasgado..."
                    maxlength="500"
                  />
                </div>
              }
            </div>
          </div>

          <div class="form-actions">
            <button
              class="btn btn-primary"
              (click)="submitItem()"
              [disabled]="saving()"
            >
              {{ saving() ? 'A guardar...' : (form.isRejected ? 'Rejeitar Peça' : 'Aceitar Peça') }}
            </button>
          </div>
        </div>
      }

      <!-- Complete evaluation button -->
      @if (reception()!.status === 'PendingEvaluation' && items().length > 0 && remainingCount() === 0) {
        <div class="card complete-card">
          <div class="complete-icon">✓</div>
          <h3 class="complete-title">Todas as peças foram avaliadas!</h3>
          <p class="complete-subtitle">
            {{ acceptedCount() }} aceites e {{ rejectedCount() }} rejeitados de {{ reception()!.itemCount }} peças.
          </p>

          @if (completeError()) {
            <div class="alert alert-error">{{ completeError() }}</div>
          }

          <button
            class="btn btn-primary btn-lg"
            (click)="completeEvaluation()"
            [disabled]="completing()"
          >
            {{ completing() ? 'A concluir...' : 'Concluir Avaliação' }}
          </button>
        </div>
      }

      <!-- Already completed -->
      @if (reception()!.status === 'Evaluated') {
        <div class="card complete-card">
          <div class="complete-icon done-icon">✓</div>
          <h3 class="complete-title">Avaliação Concluída</h3>
          <p class="complete-subtitle">
            Avaliada em {{ reception()!.evaluatedAt | date: 'dd/MM/yyyy HH:mm' }}
            · {{ acceptedCount() }} aceites · {{ rejectedCount() }} rejeitados
          </p>

          <!-- Email status -->
          <div class="email-section">
            @if (emailSent()) {
              <div class="email-status email-success">
                <span class="email-icon">&#9993;</span>
                <span>Email enviado com sucesso para <b>{{ emailSentTo() }}</b></span>
              </div>
            }
            @if (emailError()) {
              <div class="email-status email-error">
                <span>{{ emailError() }}</span>
              </div>
            }

            <button
              class="btn btn-outline btn-email"
              (click)="sendEmail()"
              [disabled]="sendingEmail()"
            >
              <span class="email-btn-icon">&#9993;</span>
              {{ sendingEmail() ? 'A enviar...' : (emailSent() ? 'Reenviar Email ao Fornecedor' : 'Enviar Email ao Fornecedor') }}
            </button>
          </div>

          <a class="btn btn-outline" routerLink="/consignments/receptions">Ver Todas as Recepções</a>
        </div>
      }
    }
  `,
  styles: [`
    :host { display: block; }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .page-title {
      font-size: 22px;
      font-weight: 700;
      margin: 0 0 4px;
      color: #1e293b;
    }

    .page-subtitle {
      font-size: 14px;
      color: #64748b;
      margin: 0;
    }

    .page-header-actions { display: flex; gap: 8px; }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
      margin-bottom: 16px;
    }

    .table-card { padding: 0; }

    .card-title-bar {
      padding: 14px 20px;
      border-bottom: 1px solid #e2e8f0;
    }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .form-card {
      border: 2px solid #c7d2fe;
      background: #fafaff;
    }

    /* ── Buttons ── */
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
      text-decoration: none;
    }

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }
    .btn-outline { background: white; color: #1e293b; border-color: #e2e8f0; }
    .btn-outline:hover:not(:disabled) { background: #f8fafc; }
    .btn-sm { padding: 4px 8px; font-size: 12px; }
    .btn-lg { padding: 10px 24px; font-size: 14px; }
    .btn-danger-outline { color: #ef4444; border-color: #fecaca; }
    .btn-danger-outline:hover:not(:disabled) { background: #fef2f2; }

    /* ── Info card ── */
    .info-card { display: flex; flex-direction: column; gap: 16px; }

    .info-top {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .supplier-info {
      display: flex;
      gap: 12px;
      align-items: center;
    }

    .initial-badge-lg {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 44px;
      height: 44px;
      border-radius: 10px;
      background: #6366f1;
      color: white;
      font-size: 16px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .supplier-name {
      display: block;
      font-size: 16px;
      font-weight: 700;
      color: #1e293b;
    }

    .reception-meta {
      display: block;
      font-size: 12px;
      color: #64748b;
    }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-yellow { background: #fef9c3; color: #854d0e; }
    .badge-green { background: #dcfce7; color: #166534; }
    .badge-red { background: #fee2e2; color: #991b1b; }
    .badge-blue { background: #dbeafe; color: #1e40af; }
    .badge-gray { background: #f1f5f9; color: #475569; }

    /* ── Progress ── */
    .progress-section { display: flex; flex-direction: column; gap: 6px; }

    .progress-stats {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
    }

    .progress-text { font-size: 14px; color: #475569; }
    .progress-percent { font-size: 14px; font-weight: 700; color: #6366f1; }

    .progress-bar-container {
      width: 100%;
      height: 8px;
      background: #e2e8f0;
      border-radius: 4px;
      overflow: hidden;
    }

    .progress-bar {
      height: 100%;
      background: #6366f1;
      border-radius: 4px;
      transition: width 0.3s;
    }

    .progress-summary {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
    }

    /* ── Table ── */
    .table-wrapper { overflow-x: auto; }

    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

    th {
      background: #f8fafc;
      padding: 10px 14px;
      text-align: left;
      font-weight: 600;
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #64748b;
      border-bottom: 1px solid #e2e8f0;
    }

    td {
      padding: 10px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td { background: #f1f5f9; }

    .row-rejected td { background: #fef2f2; }
    .row-rejected:hover td { background: #fee2e2; }

    .cell-num { width: 40px; color: #94a3b8; text-align: center; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #64748b; }
    .cell-price { font-weight: 600; white-space: nowrap; }

    /* ── Form ── */
    .form-section { padding: 0 20px 20px; }
    .form-card .card-title-bar { margin-bottom: 16px; }

    .form-group { margin-bottom: 14px; }
    .form-group:last-child { margin-bottom: 0; }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 5px;
    }

    .form-row { display: flex; gap: 12px; }

    .form-group-grow { flex: 1; }
    .form-group-sm { width: 120px; flex-shrink: 0; }

    .form-input {
      width: 100%;
      padding: 9px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      color: #1e293b;
      font-family: inherit;
      transition: border-color 0.15s;
      background: white;
    }

    .form-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.input-error { border-color: #ef4444; }

    .form-textarea { resize: vertical; min-height: 60px; }

    .field-error {
      display: block;
      font-size: 12px;
      color: #ef4444;
      margin-top: 3px;
    }

    /* ── Tags ── */
    .tags-picker {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    .tag-chip {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 5px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 500;
      cursor: pointer;
      border: 1.5px solid #e2e8f0;
      background: white;
      color: #475569;
      transition: all 0.15s;
      user-select: none;
    }

    .tag-chip input { display: none; }
    .tag-chip:hover { border-color: var(--tag-color, #6366f1); }

    .tag-chip.tag-selected {
      border-color: var(--tag-color, #6366f1);
      background: color-mix(in srgb, var(--tag-color, #6366f1) 12%, white);
      color: var(--tag-color, #6366f1);
      font-weight: 600;
    }

    /* ── Rejection ── */
    .rejection-section {
      margin-top: 16px;
      padding: 14px;
      background: white;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
    }

    .rejection-toggle {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
    }

    .rejection-toggle input {
      width: 18px;
      height: 18px;
      accent-color: #ef4444;
    }

    .toggle-label {
      font-size: 14px;
      font-weight: 600;
      color: #991b1b;
    }

    .rejection-reason-group { margin-top: 12px; margin-bottom: 0; }

    /* ── Form actions ── */
    .form-actions {
      display: flex;
      justify-content: flex-end;
      padding: 0 20px 20px;
    }

    /* ── Complete card ── */
    .complete-card {
      text-align: center;
      padding: 40px 24px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 10px;
    }

    .complete-icon {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: #dbeafe;
      color: #2563eb;
      font-size: 28px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 8px;
    }

    .done-icon {
      background: #dcfce7;
      color: #16a34a;
    }

    .complete-title {
      font-size: 18px;
      font-weight: 700;
      color: #1e293b;
      margin: 0;
    }

    .complete-subtitle {
      font-size: 14px;
      color: #64748b;
      margin: 0;
    }

    /* ── Alert ── */
    .alert {
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      margin-bottom: 14px;
    }

    .alert-error {
      background: #fef2f2;
      color: #991b1b;
      border: 1px solid #fecaca;
    }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    /* ── Email section ── */
    .email-section {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 10px;
      margin: 8px 0 12px;
    }

    .email-status {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 13px;
    }

    .email-success {
      background: #f0fdf4;
      color: #166534;
      border: 1px solid #bbf7d0;
    }

    .email-error {
      background: #fef2f2;
      color: #991b1b;
      border: 1px solid #fecaca;
    }

    .email-icon { font-size: 16px; }

    .btn-email {
      border-color: #c7d2fe;
      color: #4f46e5;
    }

    .btn-email:hover:not(:disabled) {
      background: #eef2ff;
      border-color: #818cf8;
    }

    .email-btn-icon {
      font-size: 15px;
    }

    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .form-row { flex-direction: column; gap: 0; }
      .form-group-sm { width: 100%; }
    }
  `]
})
export class ReceptionEvaluatePageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly receptionService = inject(ReceptionService);
  private readonly brandService = inject(BrandService);
  private readonly categoryService = inject(CategoryService);
  private readonly tagService = inject(TagService);

  // Data
  reception = signal<ReceptionDetail | null>(null);
  items = signal<EvaluationItemResponse[]>([]);
  brands = signal<SelectOption[]>([]);
  categories = signal<SelectOption[]>([]);
  tags = signal<SelectOption[]>([]);

  // State
  loading = signal(true);
  error = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  removingItem = signal<string | null>(null);
  completing = signal(false);
  completeError = signal<string | null>(null);
  sendingEmail = signal(false);
  emailSent = signal(false);
  emailSentTo = signal<string | null>(null);
  emailError = signal<string | null>(null);
  formSubmitted = false;

  // Tag selection
  selectedTagIds = new Set<string>();

  // Form
  form = this.getEmptyForm();

  // Computed values
  acceptedCount = signal(0);
  rejectedCount = signal(0);
  remainingCount = signal(0);
  progressPercent = signal(0);

  ngOnInit(): void {
    const receptionId = this.route.snapshot.paramMap.get('id');
    if (!receptionId) {
      this.error.set('ID de recepção inválido.');
      this.loading.set(false);
      return;
    }

    // Load all data in parallel
    forkJoin({
      reception: this.receptionService.getById(receptionId),
      items: this.receptionService.getReceptionItems(receptionId),
      brands: this.brandService.getAll(),
      categories: this.categoryService.getAll(),
      tags: this.tagService.getAll(),
    }).subscribe({
      next: ({ reception, items, brands, categories, tags }) => {
        this.reception.set(reception);
        this.items.set(items);
        this.brands.set(brands.map((b: any) => ({ externalId: b.externalId, name: b.name })));
        this.categories.set(categories.map((c: any) => ({ externalId: c.externalId, name: c.name })));
        this.tags.set(tags.map((t: any) => ({ externalId: t.externalId, name: t.name, color: t.color })));
        this.updateCounts();
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erro ao carregar dados da recepção.');
        this.loading.set(false);
      }
    });
  }

  private updateCounts(): void {
    const items = this.items();
    const reception = this.reception();
    const accepted = items.filter(i => !i.isRejected).length;
    const rejected = items.filter(i => i.isRejected).length;
    const remaining = reception ? reception.itemCount - items.length : 0;
    const progress = reception && reception.itemCount > 0
      ? (items.length / reception.itemCount) * 100
      : 0;

    this.acceptedCount.set(accepted);
    this.rejectedCount.set(rejected);
    this.remainingCount.set(Math.max(0, remaining));
    this.progressPercent.set(progress);
  }

  private getEmptyForm() {
    return {
      name: '',
      description: '',
      brandExternalId: '',
      categoryExternalId: '',
      size: '',
      color: '',
      composition: '',
      condition: '',
      evaluatedPrice: null as number | null,
      commissionPercentage: 50 as number | null,
      isRejected: false,
      rejectionReason: '',
    };
  }

  private resetForm(): void {
    this.form = this.getEmptyForm();
    this.selectedTagIds.clear();
    this.formSubmitted = false;
    this.formError.set(null);
  }

  isTagSelected(externalId: string): boolean {
    return this.selectedTagIds.has(externalId);
  }

  toggleTag(externalId: string): void {
    if (this.selectedTagIds.has(externalId)) {
      this.selectedTagIds.delete(externalId);
    } else {
      this.selectedTagIds.add(externalId);
    }
  }

  getConditionLabel(condition: string): string {
    const labels: Record<string, string> = {
      Excellent: 'Excelente',
      VeryGood: 'Muito Bom',
      Good: 'Bom',
      Fair: 'Razoável',
      Poor: 'Mau',
    };
    return labels[condition] || condition;
  }

  submitItem(): void {
    this.formSubmitted = true;
    this.formError.set(null);

    // Validate
    if (!this.form.name.trim()) return;
    if (!this.form.brandExternalId) return;
    if (!this.form.size) return;
    if (!this.form.color.trim()) return;
    if (!this.form.condition) return;
    if (!this.form.evaluatedPrice || this.form.evaluatedPrice <= 0) return;

    this.saving.set(true);

    const data: AddEvaluationItemRequest = {
      name: this.form.name.trim(),
      description: this.form.description.trim() || undefined,
      brandExternalId: this.form.brandExternalId,
      categoryExternalId: this.form.categoryExternalId || undefined,
      size: this.form.size,
      color: this.form.color.trim(),
      composition: this.form.composition.trim() || undefined,
      condition: this.form.condition,
      evaluatedPrice: this.form.evaluatedPrice!,
      commissionPercentage: this.form.commissionPercentage || undefined,
      isRejected: this.form.isRejected,
      rejectionReason: this.form.isRejected ? (this.form.rejectionReason.trim() || undefined) : undefined,
      tagExternalIds: this.selectedTagIds.size > 0 ? Array.from(this.selectedTagIds) : undefined,
    };

    this.receptionService.addEvaluationItem(this.reception()!.externalId, data).subscribe({
      next: (newItem) => {
        this.saving.set(false);
        this.items.set([...this.items(), newItem]);
        this.updateCounts();
        this.resetForm();
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(err.error?.error || 'Erro ao adicionar item.');
      }
    });
  }

  removeItem(item: EvaluationItemResponse): void {
    this.removingItem.set(item.externalId);

    this.receptionService.removeEvaluationItem(this.reception()!.externalId, item.externalId).subscribe({
      next: () => {
        this.removingItem.set(null);
        this.items.set(this.items().filter(i => i.externalId !== item.externalId));
        this.updateCounts();
      },
      error: () => {
        this.removingItem.set(null);
      }
    });
  }

  completeEvaluation(): void {
    this.completing.set(true);
    this.completeError.set(null);

    this.receptionService.completeEvaluation(this.reception()!.externalId).subscribe({
      next: (result) => {
        this.completing.set(false);

        // Check auto-send email result
        if (result.emailSent) {
          this.emailSent.set(true);
          this.emailSentTo.set(this.reception()?.supplier?.name || null);
        }

        // Reload reception to get updated status
        this.receptionService.getById(this.reception()!.externalId).subscribe({
          next: (updated) => {
            this.reception.set(updated);
          }
        });
      },
      error: (err) => {
        this.completing.set(false);
        this.completeError.set(err.error?.error || 'Erro ao concluir avaliação.');
      }
    });
  }

  sendEmail(): void {
    this.sendingEmail.set(true);
    this.emailError.set(null);

    this.receptionService.sendEvaluationEmail(this.reception()!.externalId).subscribe({
      next: (result) => {
        this.sendingEmail.set(false);
        this.emailSent.set(true);
        this.emailSentTo.set(result.sentTo || this.reception()?.supplier?.name || null);
      },
      error: (err) => {
        this.sendingEmail.set(false);
        this.emailError.set(err.error?.error || 'Erro ao enviar email. Verifique as configurações SMTP.');
      }
    });
  }
}
