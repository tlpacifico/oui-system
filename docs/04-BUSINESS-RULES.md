# Oui Circular - Business Rules Document

## 1. Consignment Rules

### RN-CON-01: Item Identification Number Format
- **Format:** `{SupplierInitial}{YYYYMM}{SequenceNumber:0000}`
- **Example:** `AF20260200001` (Supplier "Ana Ferreira", Feb 2026, item #001)
- **Sequence resets** at the beginning of each calendar month
- Sequence is per-supplier, per-month

### RN-CON-02: Default Consignment Period
- Standard consignment period: **60 days** from intake date
- After expiry, system should:
  1. Generate alert for store manager
  2. Notify supplier via WhatsApp/email
  3. Allow options: renew (extend 30 days), return, or renegotiate

### RN-CON-03: Commission Rates
- Each supplier has two commission rates:
  - **Cash commission** (`CommissionPercentageInCash`): the **client receives 40%** of the sale price; the store keeps **60%**
  - **Credit commission** (`CommissionPercentageInCredit`): the **client receives 50%** of the sale price as store credit; the store keeps **50%**
- Credit commission is higher for the client, incentivizing store credit over cash
- **Example:** Item sold for **€20**:
  - Cash payment: client receives **€8**, store keeps **€12**
  - Credit payment: client receives **€10** in store credit, store keeps **€10**

### RN-CON-04: Consignment Status Lifecycle
```
Recebido --> Avaliado --> A Venda --> Vendido --> Pago
                |                       |
           Com Defeito              Devolvido
                |
           Devolvido
```
- **Recebido (1):** Items received and counted; reception receipt signed by client; awaiting evaluation
- **Avaliado (2):** Items evaluated and priced by the store
- **ComDefeito (3):** Item rejected due to defect; awaiting return to supplier
- **AVenda (4):** Item approved and available in store for sale
- **Vendido (5):** Item was sold; triggers commission calculation
- **Pago (6):** Settlement completed; supplier has been paid
- **Devolvido (7):** Item returned to supplier (unsold, rejected, or end of consignment period)

### RN-CON-05: Reception Receipt
- Generated automatically at item intake (**Etapa 1 - Recebido**)
- **Format:** `REC-{YYYY}-{Seq:0000}` (e.g., `REC-2026-0042`)
- Sequence is global and resets annually
- Receipt contains:
  - Client name
  - Date of reception (dd/mm/yyyy)
  - Quantity of items received
  - **No values or prices** are listed on the reception receipt
- Client must sign the receipt at the moment of drop-off
- A copy is kept digitally; the original is given to the client

### RN-CON-06: Evaluation Email
- Sent to the client after the evaluation of all items is complete (**Etapa 2 - Avaliado**)
- Email contains:
  - List of **accepted items** with their evaluated values
  - List of **rejected items** with the reason for rejection (e.g., defect, stain, wear)
  - Statement of commission terms: **40% cash / 50% store credit**
  - Instructions for how and when to collect rejected items
- If no email is on file, the client is contacted via WhatsApp

### RN-CON-07: Consignment Contract
- A consignment contract must list all accepted items with:
  - Identification number
  - Description
  - Evaluated value
  - Commission rate agreed (40% cash / 50% credit)
  - Consignment period
- Both parties (store + supplier) must sign the contract
- Contract serves as legal proof of consignment terms

---

## 2. Acquisition Model

### RN-ACQ-01: Mixed Acquisition Model
The store operates with three acquisition channels:

1. **Consignment** - Items received from clients under consignment agreement
   - Subject to commission rules (RN-CON-03)
   - Tracked through the full status lifecycle (Recebido --> Pago)
   - Supplier receives payment upon settlement

2. **Own Purchase** - Items bought directly by the store from external sources (e.g., Humana, Vinted, feiras, other suppliers)
   - **No commission** applies; the store owns the items outright
   - Purchase cost is recorded for margin calculation
   - Items enter inventory directly in status **AVenda**
   - Tracked as store-owned in the system

3. **Personal Collection** - Items from the store owner's personal wardrobe or collection
   - **No commission** applies
   - No purchase cost recorded
   - Items enter inventory directly in status **AVenda**
   - Flagged as owner items in the system

### RN-ACQ-02: Acquisition Source Tracking
- Every item in inventory must have an **acquisition type** recorded: `Consignment`, `OwnPurchase`, or `PersonalCollection`
- For consignment items: linked to a supplier record
- For own purchase items: source name and purchase cost recorded
- For personal collection items: no additional data required

---

## 3. Pricing Rules

### RN-PRC-01: Price Change Authorization
- Price reductions up to **20%** of original value: any manager can approve
- Price reductions above **20%**: requires owner/admin approval
- All price changes must record: old price, new price, reason, who approved, when

### RN-PRC-02: Automatic Price Reduction (Suggested)
Based on market practices:
- Items unsold for **30+ days:** suggest 10% price reduction
- Items unsold for **45+ days:** suggest 20% price reduction
- Items unsold for **60+ days:** suggest return to supplier or 30%+ reduction
- These are suggestions only - manager decides final action

### RN-PRC-03: Consignment Price Impact
- When a consigned item's price changes, the commission amount changes proportionally
- Supplier should be notified of price changes above 15%
- Original evaluated value is preserved in history for reference

---

## 4. POS / Sales Rules

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
- Accepted methods: **Cash, Credit Card, Debit Card, MBWAY, Store Credit**
- Split payment allowed: customer can combine up to 2 payment methods
- Total of all payments must equal or exceed sale total
- Change is only given for cash payments

### RN-POS-04: Sale Completion Effects
When a sale is finalized:
1. Each sold item's status changes to `Vendido`
2. Items are removed from available inventory
3. For consigned items: sale is recorded for future commission settlement
4. Receipt is generated
5. Cash register totals are updated
6. If customer is registered: loyalty points are credited

### RN-POS-05: Sale Number Format
- Format: `V{YYYYMMDD}-{DailySequence:000}`
- Example: `V20260215-015` (15th sale on 15 February 2026)
- Sequence resets daily

### RN-POS-06: Large Sale Requirements
- Sales above **€1,000:** customer NIF required for invoicing compliance
- Sales above **€500 in cash:** additional documentation may be required

---

## 5. Return / Exchange Rules

### RN-RET-01: Return Policy
- **Defective items:** exchange within 7 days of purchase
- **Non-defective items:** store credit only, within 30 days
- No cash refunds - only exchange or store credit
- Item must be in same condition as sold

### RN-RET-02: Return Effects on Consignment
- If a consigned item is returned:
  - Item status reverts to `AVenda`
  - Pending commission for that item is cancelled
  - If commission was already settled, adjustment is made in next settlement
- If item is returned in non-sellable condition:
  - Item status changes to `Devolvido`
  - Loss is absorbed by the store (supplier not penalized)

### RN-RET-03: Store Credit
- Store credits have an expiration period: **180 days** (6 months)
- Credits are non-transferable
- Credits can be used in combination with other payment methods
- Partial credit usage is allowed (remaining balance is preserved)

---

## 6. Cash Register Closing Rules

### RN-REG-01: Closing Reconciliation
- System calculates expected cash: Opening float + Cash sales - Cash returns
- Cashier counts and enters actual cash amount
- Discrepancy threshold: up to **€5.00** is acceptable (rounding)
- Discrepancy above **€5.00** requires written justification
- Discrepancy above **€50.00** triggers manager review

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

## 7. Settlement Rules

### RN-SET-01: Settlement Period
- Default settlement cycle: **monthly** (1st to last day of month)
- Can be adjusted per supplier agreement (bi-weekly, weekly)
- Settlement can only be processed for items sold before the settlement date

### RN-SET-02: Settlement Calculation
For each sold consigned item:
```
If PaymentType = Cash:
    ClientPayment = SalePrice * 40 / 100
    StoreKeeps = SalePrice - ClientPayment

If PaymentType = CreditInStore:
    ClientCredit = SalePrice * 50 / 100
    StoreKeeps = SalePrice - ClientCredit
```

### RN-SET-03: Settlement Minimum
- Minimum settlement amount: **€20.00**
- If supplier's pending amount is below minimum, rolls over to next period
- Supplier can request immediate settlement regardless of minimum

### RN-SET-04: Settlement Communication
- Suppliers are notified of pending settlements via **WhatsApp**
- Settlement summary includes: items sold, amounts, commission breakdown
- Supplier confirms preferred payment method (cash or store credit)

### RN-SET-05: Settlement Report
Settlement receipt must include:
- Supplier name and NIF
- Period covered
- List of sold items (ID, name, sale date, sale price)
- Commission rate and amount per item
- Total sales, total commission, net payable to supplier
- Payment method (cash, MBWAY, or store credit)
- Signature fields for both parties

---

## 8. Inventory Rules

### RN-INV-01: Item Uniqueness
- Each item is unique (no SKU-based quantity tracking)
- Each item has a unique identification number (barcode)
- Item cannot be in two places at once (inventory integrity)

### RN-INV-02: Mandatory Item Attributes
- **Required:** Name, Evaluated Value, Brand, Acquisition Type
- **Recommended:** Size, Color, Condition, Composition
- **Optional:** Tags, Photos, Notes

### RN-INV-03: Item Archival
- Sold items are not deleted - they remain in the database with `Vendido` status
- Returned items are kept with `Devolvido` status
- Soft-deleted items are kept for audit purposes
- Data is retained for minimum **5 years** for fiscal compliance

### RN-INV-04: Stagnant Inventory Alerts
- Items in stock for **30+ days:** yellow alert
- Items in stock for **45+ days:** orange alert
- Items in stock for **60+ days:** red alert + suggested action
- Alert thresholds are configurable per system settings

---

## 9. User Roles & Permissions

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

## 10. Loyalty Program Rules (Future)

### RN-LOY-01: Points Accumulation
- **1 point per €5 spent** (configurable)
- Points are credited after sale completion
- Voided/returned sales: points are deducted

### RN-LOY-02: Points Redemption
- **100 points = €5 discount** (configurable)
- Points can be partially redeemed
- Minimum redemption: 50 points
- Points expire after **12 months** of inactivity

### RN-LOY-03: Special Benefits
- Birthday month: **10% discount** on one purchase
- VIP status (1000+ lifetime points): early access to new arrivals
- Referral bonus: 50 points when referred customer makes first purchase

---

## 11. Fiscal Rules (Portugal)

### RN-FIS-01: Invoicing Requirements
- Every sale must generate a fiscal document compliant with Portuguese invoicing law (faturacao certificada)
- Store must have NIF (Numero de Identificacao Fiscal) registered
- Invoicing software must be certified by AT (Autoridade Tributaria e Aduaneira)

### RN-FIS-02: Tax Compliance
- IVA (Imposto sobre o Valor Acrescentado) calculation based on applicable rates:
  - Standard rate: **23%**
  - Intermediate rate: **13%**
  - Reduced rate: **6%**
- Second-hand goods may be subject to the **margin scheme** (Regime da Margem de Lucro) for IVA purposes
- Records must be retained for minimum **5 years** for fiscal compliance

### RN-FIS-03: Consignment Tax Treatment
- Consigned goods have specific tax treatment under Portuguese law
- Commission income is the taxable base for the store
- Supplier payments are not revenue for the store
- NIF of the supplier must be recorded for all consignment agreements

### RN-FIS-04: Customer NIF
- Customer NIF is requested for all sales but only **mandatory for sales above €1,000** or when the customer requests an invoice with NIF
- NIF format: 9 digits (e.g., 123456789)
- NIF validation: check digit must be verified

---

## 12. Data & Communication Formats (Portugal)

### RN-FMT-01: Currency
- All monetary values are in **Euro (€)**
- Format: `€XX.XX` or `XX,XX €` depending on context
- Decimal separator: comma (,)
- Thousands separator: period (.)

### RN-FMT-02: Contact Information
- Phone format: **+351 XXX XXX XXX**
- NIF format: 9 digits, no separators
- Date format: **dd/mm/yyyy**

### RN-FMT-03: Communication Channels
- Primary client communication: **WhatsApp**
- Secondary: email
- Settlement notifications: WhatsApp
- Evaluation notifications: email (with WhatsApp fallback)
