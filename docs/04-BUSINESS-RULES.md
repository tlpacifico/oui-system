# SHS - Business Rules Document

## 1. Consignment Rules

### RN-CON-01: Item Identification Number Format
- **Format:** `{SupplierInitial}{YYYYMM}{SequenceNumber:0000}`
- **Example:** `MS202509001` (Supplier "Maria Silva", Sep 2025, item #001)
- **Sequence resets** at the beginning of each calendar month
- Sequence is per-supplier, per-month
- **Already implemented** in `ConsignmentService.BuildIdentificationNumber()`

### RN-CON-02: Default Consignment Period
- Standard consignment period: **60 days** from intake date
- After expiry, system should:
  1. Generate alert for store manager
  2. Notify supplier via email/SMS
  3. Allow options: renew (extend 30 days), return, or renegotiate

### RN-CON-03: Commission Rates
- Each supplier has two commission rates:
  - **Cash commission** (`CommissionPercentageInCash`): percentage the store keeps when paying supplier in cash
  - **Product/Credit commission** (`CommissionPercentageInProducts`): percentage the store keeps when supplier takes store credit
- Credit commission is typically lower (supplier gets more), incentivizing store credit
- **Example:** 40% cash commission / 30% credit commission means:
  - Item sold for R$100, cash payment: store keeps R$40, supplier gets R$60
  - Item sold for R$100, credit payment: store keeps R$30, supplier gets R$70 in store credit

### RN-CON-04: Consignment Status Lifecycle
```
Evaluated ──► AwaitingAcceptance ──► ToSell ──► Sold
                    │                   │
                    ▼                   ▼
                 Returned            Returned
```
- **Evaluated (1):** Item received and priced by store
- **AwaitingAcceptance (2):** Waiting for supplier to agree on price
- **ToSell (3):** Approved and available in store for sale
- **Sold (4):** Item was sold - triggers commission calculation
- **Returned (5):** Item returned to supplier (unsold or rejected)

### RN-CON-05: Consignment Contract
- A consignment contract must list all items with:
  - Identification number
  - Description
  - Evaluated value
  - Commission rate agreed
  - Consignment period
- Both parties (store + supplier) must sign the contract
- Contract serves as legal proof of consignment terms

---

## 2. Pricing Rules

### RN-PRC-01: Price Change Authorization
- Price reductions up to **20%** of original value: any manager can approve
- Price reductions above **20%**: requires owner/admin approval
- All price changes must record: old price, new price, reason, who approved, when

### RN-PRC-02: Automatic Price Reduction (Suggested)
Based on market practices (Peca Rara model):
- Items unsold for **30+ days:** suggest 10% price reduction
- Items unsold for **45+ days:** suggest 20% price reduction
- Items unsold for **60+ days:** suggest return to supplier or 30%+ reduction
- These are suggestions only - manager decides final action

### RN-PRC-03: Consignment Price Impact
- When a consigned item's price changes, the commission amount changes proportionally
- Supplier should be notified of price changes above 15%
- Original evaluated value is preserved in history for reference

---

## 3. POS / Sales Rules

### RN-POS-01: Cash Register Session
- Each cashier can have only **one open register** at a time
- Register must be opened before any sales can be processed
- Register must be closed at end of shift
- Opening float amount must be recorded

### RN-POS-02: Discount Authorization
- Discounts up to **10%:** cashier can apply
- Discounts **10-20%:** requires manager approval (PIN or confirmation)
- Discounts above **20%:** requires owner/admin approval
- Discount reason must be recorded

### RN-POS-03: Payment Methods
- Accepted methods: Cash, Credit Card, Debit Card, PIX, Store Credit
- Split payment allowed: customer can combine up to 2 payment methods
- Total of all payments must equal or exceed sale total
- Change is only given for cash payments

### RN-POS-04: Sale Completion Effects
When a sale is finalized:
1. Each sold item's status changes to `Sold`
2. Items are removed from available inventory
3. For consigned items: sale is recorded for future commission settlement
4. Receipt/cupom fiscal is generated
5. Cash register totals are updated
6. If customer is registered: loyalty points are credited

### RN-POS-05: Sale Number Format
- Format: `V{YYYYMMDD}-{DailySequence:000}`
- Example: `V20250901-015` (15th sale on September 1, 2025)
- Sequence resets daily

### RN-POS-06: Large Sale Requirements
- Sales above **R$10,000:** customer CPF required for fiscal compliance
- Sales above **R$5,000 in cash:** additional documentation may be required

---

## 4. Return / Exchange Rules

### RN-RET-01: Return Policy
- **Defective items:** exchange within 7 days of purchase
- **Non-defective items:** store credit only, within 30 days
- No cash refunds - only exchange or store credit
- Item must be in same condition as sold

### RN-RET-02: Return Effects on Consignment
- If a consigned item is returned:
  - Item status reverts to `ToSell`
  - Pending commission for that item is cancelled
  - If commission was already settled, adjustment is made in next settlement
- If item is returned in non-sellable condition:
  - Item status changes to `Returned`
  - Loss is absorbed by the store (supplier not penalized)

### RN-RET-03: Store Credit
- Store credits have an expiration period: **180 days** (6 months)
- Credits are non-transferable
- Credits can be used in combination with other payment methods
- Partial credit usage is allowed (remaining balance is preserved)

---

## 5. Cash Register Closing Rules

### RN-REG-01: Closing Reconciliation
- System calculates expected cash: Opening float + Cash sales - Cash returns
- Cashier counts and enters actual cash amount
- Discrepancy threshold: up to **R$5.00** is acceptable (rounding)
- Discrepancy above **R$5.00** requires written justification
- Discrepancy above **R$50.00** triggers manager review

### RN-REG-02: Closing Report
Closing report must include:
- Number of sales processed
- Total revenue by payment method
- Opening float amount
- Cash counted vs. expected
- Discrepancy amount and notes
- List of voided transactions (if any)
- Manager approval signature

---

## 6. Settlement Rules

### RN-SET-01: Settlement Period
- Default settlement cycle: **monthly** (1st to last day of month)
- Can be adjusted per supplier agreement (bi-weekly, weekly)
- Settlement can only be processed for items sold before the settlement date

### RN-SET-02: Settlement Calculation
For each sold consigned item:
```
If PaymentType = Cash:
    StoreCommission = SalePrice * CommissionPercentageInCash / 100
    SupplierPayment = SalePrice - StoreCommission

If PaymentType = CreditInStore:
    StoreCommission = SalePrice * CommissionPercentageInProducts / 100
    SupplierCredit = SalePrice - StoreCommission
```

### RN-SET-03: Settlement Minimum
- Optional minimum settlement amount: **R$50.00**
- If supplier's pending amount is below minimum, rolls over to next period
- Supplier can request immediate settlement regardless of minimum

### RN-SET-04: Settlement Report
Settlement receipt must include:
- Supplier name and ID
- Period covered
- List of sold items (ID, name, sale date, sale price)
- Commission rate and amount per item
- Total sales, total commission, net payable
- Payment method (cash or store credit)
- Signature fields for both parties

---

## 7. Inventory Rules

### RN-INV-01: Item Uniqueness
- Each item is unique (no SKU-based quantity tracking)
- Each item has a unique identification number (barcode)
- Item cannot be in two places at once (inventory integrity)

### RN-INV-02: Mandatory Item Attributes
- **Required:** Name, Evaluated Value, Brand
- **Recommended:** Size, Color, Condition, Composition
- **Optional:** Tags, Photos, Notes

### RN-INV-03: Item Archival
- Sold items are not deleted - they remain in the database with `Sold` status
- Returned items are kept with `Returned` status
- Soft-deleted items are kept for audit purposes
- Data is retained for minimum **5 years** for fiscal compliance

### RN-INV-04: Stagnant Inventory Alerts
- Items in stock for **30+ days:** yellow alert
- Items in stock for **45+ days:** orange alert
- Items in stock for **60+ days:** red alert + suggested action
- Alert thresholds are configurable per system settings

---

## 8. User Roles & Permissions

### RN-USR-01: Role Definitions

| Permission | Cashier | Manager | Finance | Admin |
|------------|---------|---------|---------|-------|
| View inventory | Yes | Yes | Yes | Yes |
| Add items | No | Yes | No | Yes |
| Edit items | No | Yes | No | Yes |
| Delete items | No | Yes | No | Yes |
| Change prices | No | Yes | No | Yes |
| Open/close register | Yes | Yes | No | Yes |
| Process sales | Yes | Yes | No | Yes |
| Apply discount > 10% | No | Yes | No | Yes |
| Void sales | No | Yes | No | Yes |
| Process returns | No | Yes | No | Yes |
| View reports | No | Yes | Yes | Yes |
| Manage settlements | No | No | Yes | Yes |
| Process payments | No | No | Yes | Yes |
| Manage users | No | No | No | Yes |
| System configuration | No | No | No | Yes |
| Manage suppliers | No | Yes | Yes | Yes |
| Create consignments | No | Yes | No | Yes |

### RN-USR-02: Authentication
- All users must authenticate via Firebase
- Session expires after **8 hours** of inactivity
- Failed login attempts: lock after 5 attempts for 15 minutes

---

## 9. Loyalty Program Rules (Future)

### RN-LOY-01: Points Accumulation
- **1 point per R$10 spent** (configurable)
- Points are credited after sale completion
- Voided/returned sales: points are deducted

### RN-LOY-02: Points Redemption
- **100 points = R$10 discount** (configurable)
- Points can be partially redeemed
- Minimum redemption: 50 points
- Points expire after **12 months** of inactivity

### RN-LOY-03: Special Benefits
- Birthday month: **10% discount** on one purchase
- VIP status (1000+ lifetime points): early access to new arrivals
- Referral bonus: 50 points when referred customer makes first purchase

---

## 10. Fiscal Rules (Brazil-specific)

### RN-FIS-01: Document Emission
- Every sale must generate a fiscal document (NF-e or NFC-e)
- Store must have CNPJ registered
- Integration with SEFAZ required for production environment

### RN-FIS-02: Tax Compliance
- ICMS calculation based on state regulations
- SIMPLES Nacional tax regime for small businesses
- Records must be retained for minimum 5 years

### RN-FIS-03: Consignment Tax Treatment
- Consigned goods have specific tax treatment
- Commission income is the taxable base for the store
- Supplier payments are not revenue for the store
