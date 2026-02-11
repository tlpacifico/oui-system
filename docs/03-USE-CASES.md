# SHS - Use Cases Document

## Legend

| Symbol | Meaning |
|--------|---------|
| **[DONE]** | Already implemented in current codebase |
| **[TODO]** | Not yet implemented |
| **P0** | Must-have for MVP |
| **P1** | Important, second phase |
| **P2** | Nice-to-have, future enhancement |

---

## MODULE 1: INVENTORY MANAGEMENT

### CU-01: Register Item in Inventory [TODO] - P0

**Primary Actor:** Store Employee
**Pre-conditions:** Employee authenticated, Consignment exists

**Main Flow:**
1. Employee navigates to consignment and selects "Add Item"
2. System displays registration form
3. Employee fills in:
   - Name/description
   - Brand (select from catalog)
   - Size
   - Color
   - Composition (fabric type)
   - Item Condition (new, good, used, etc.)
   - Tags/categories
   - Evaluated value (price)
4. System auto-generates identification number: `{SupplierInitial}{YYYYMM}{Sequence:0000}`
5. System saves item linked to the consignment
6. Item status is set to `Evaluated`

**Alternative Flows:**
- 3a. Brand doesn't exist: employee can use existing brand catalog
- 4a. Monthly sequence resets at the beginning of each month

**Post-conditions:** Item available in inventory with unique identification number


---

### CU-02: Search/Browse Inventory	[TODO] - P0

**Primary Actor:** Employee/Manager
**Pre-conditions:** User authenticated

**Main Flow:**
1. User accesses items list
2. System displays items with filters:
   - Item name (text search)
   - Price range (min/max)
   - Size
   - Brand
   - Color
   - Supplier name
   - Consignment date range
3. User applies desired filters
4. System returns paginated results with: name, brand, size, price, status, supplier
5. User can navigate pages

---

### CU-03: Update Item Price [TODO] - P0

**Primary Actor:** Manager
**Pre-conditions:** Item exists in system

**Main Flow:**
1. Manager finds item via search or consignment
2. System displays item details
3. Manager selects "Edit"
4. Manager updates evaluated value and/or other attributes
5. System saves changes with audit trail (UpdatedBy, UpdatedOn)

**Business Rules:**
- RN-01: If item is consigned, commission must be recalculated on sale
- RN-02: Price changes above X% should require manager approval [TODO]
- RN-03: Price history should be maintained [TODO]


---

### CU-04: Transfer Item Between Stores [TODO] - P2

**Primary Actor:** Manager
**Pre-conditions:** Multi-store system active

**Main Flow:**
1. Manager selects item(s) for transfer
2. System displays available stores
3. Manager selects destination store
4. System generates transfer note
5. Origin store employee confirms shipment
6. Destination store employee confirms receipt
7. System updates inventory for both stores

**Dependencies:** Multi-store support (Module 8)

---

### CU-05: Delete/Remove Item [TODO] - P0

**Primary Actor:** Manager/Employee
**Pre-conditions:** Item exists, not yet sold

**Main Flow:**
1. User finds item
2. User selects "Delete"
3. System confirms deletion
4. System performs soft-delete (sets IsDeleted flag)
5. Item no longer appears in active inventory

---

## MODULE 2: CONSIGNMENT MANAGEMENT

### CU-06: Register Supplier/Consignor [TODO] - P0

**Primary Actor:** Employee
**Pre-conditions:** Employee authenticated

**Main Flow:**
1. Employee selects "New Supplier"
2. System displays registration form
3. Employee fills in:
   - Name
   - Email
   - Phone Number
   - Initial (letter for item ID generation)
   - Commission percentage in cash
   - Commission percentage in products/store credit
4. System generates supplier code
5. System saves supplier

---

### CU-07: Create Consignment (Receive Items) [TODO] - P0

**Primary Actor:** Employee
**Pre-conditions:** Supplier registered

**Main Flow:**
1. Employee selects "New Consignment"
2. Employee selects or creates supplier
3. System displays consignment form
4. Employee sets consignment date
5. Employee adds items one by one (uses CU-01)
6. For each item: name, brand, size, color, condition, price
7. System auto-generates identification numbers
8. Employee finalizes consignment

**Alternative Flows:**
- 7a. Generate consignment contract/receipt PDF [TODO]
- 7b. Set consignment expiry date [TODO]

**Business Rules:**
- RN-03: Default consignment period: 60 days [TODO]
- RN-04: After expiry, system alerts for renewal or return [TODO]



---

### CU-08: Search Consignments [TODO] - P0

**Primary Actor:** Employee/Manager

**Main Flow:**
1. User accesses consignment list
2. System displays paginated list with search
3. User can view consignment details including all items
4. User can navigate to edit consignment

---

### CU-09: Process Consignment Settlement [TODO] - P1

**Primary Actor:** Finance Staff
**Pre-conditions:** Consigned items have been sold

**Main Flow:**
1. System lists sold items pending payment, grouped by supplier
2. Finance staff selects supplier and period
3. System calculates:
   - Total sales value
   - Store commission amount
   - Net amount payable to supplier
4. System generates detailed report per item
5. Finance staff selects payment method:
   - Cash payment (uses cash commission percentage)
   - Store credit (uses product commission percentage)
6. Finance staff confirms payment
7. System records payment and sends receipt to supplier (email/SMS)
8. System updates items status to "consignment settled"

**Business Rules:**
- RN-05: Different commission rates for cash vs. store credit payments
- RN-06: Settlement minimum threshold (optional)
- RN-07: Settlement report must list each item with sale date, price, and commission

---

### CU-10: Return Unsold Items to Supplier [TODO] - P1

**Primary Actor:** Employee
**Pre-conditions:** Consignment period expired

**Main Flow:**
1. System alerts about items with expired consignment period
2. Employee selects items for return
3. System generates return receipt
4. Employee contacts supplier for pickup
5. Upon pickup confirmation, system removes items from inventory
6. System updates status to "Returned"

**Business Rules:**
- RN-08: Items past X days trigger automatic return notification
- RN-09: Return receipt must list all returned items with original values

---

## MODULE 3: POINT OF SALE (POS/CASH REGISTER)

### CU-11: Open Cash Register [TODO] - P0

**Primary Actor:** Cashier
**Pre-conditions:** Cashier authenticated

**Main Flow:**
1. Cashier logs into POS
2. System requests opening cash amount (float)
3. Cashier enters cash amount in register
4. System records opening with date/time and cashier identity
5. System enables POS for sales transactions

**Business Rules:**
- RN-10: Only one open register per cashier at a time
- RN-11: Opening amount must be recorded for reconciliation

---

### CU-12: Process Sale [TODO] - P0

**Primary Actor:** Cashier
**Pre-conditions:** Cash register open

**Main Flow:**
1. Cashier scans item barcode or searches manually
2. System displays item details and price
3. Cashier adds item to cart
4. Repeat steps 1-3 for additional items
5. Cashier finalizes sale
6. System displays total
7. Customer selects payment method:
   - Cash
   - Credit/Debit card
   - PIX (instant payment)
   - Mixed (split between methods)
8. Cashier processes payment
9. System removes items from inventory
10. System generates receipt
11. System prints receipt

**Alternative Flows:**
- 4a. Apply discount: cashier enters percentage/value (subject to RN-12)
- 4b. Apply promotional coupon: system validates and applies
- 7a. Split payment: customer divides across two payment methods
- 9a. Consigned items: system records sale for commission calculation

**Business Rules:**
- RN-12: Discounts above X% require manager authorization
- RN-13: Sales above R$10,000 require customer CPF registration
- RN-14: Each sold consigned item must be tracked for supplier settlement

---

### CU-13: Process Exchange/Return [TODO] - P1

**Primary Actor:** Cashier
**Pre-conditions:** Original sale in system, within return period

**Main Flow:**
1. Customer presents receipt/proof of purchase
2. Cashier finds sale in system
3. System displays sale items
4. Cashier selects item for exchange/return
5. Customer chooses:
   - Exchange for another item
   - Store credit
6. If exchange: cashier creates new sale with original value as discount
7. If store credit: system generates credit voucher
8. System returns item to inventory (if in sellable condition)

**Business Rules:**
- RN-15: Exchange period: 7 days for defective items, 30 days for store credit
- RN-16: Returned consigned items revert to "Available" status
- RN-17: Exchange value cannot exceed original sale price without additional payment

---

### CU-14: Close Cash Register [TODO] - P0

**Primary Actor:** Cashier
**Pre-conditions:** Cash register open

**Main Flow:**
1. Cashier selects "Close Register"
2. System displays summary:
   - Sales count
   - Total by payment method (cash, card, PIX)
   - Expected cash amount
3. Cashier counts physical cash and enters amount
4. System compares counted vs. expected
5. If discrepancy exists, cashier provides justification
6. System generates closing report
7. Manager approves closing
8. System locks register for new sales

---

## MODULE 4: REPORTS & BUSINESS INTELLIGENCE

### CU-15: Sales Report [TODO] - P1

**Primary Actor:** Manager/Owner

**Main Flow:**
1. User accesses reports module
2. User selects filters: period, store, category, salesperson
3. System generates report:
   - Total revenue
   - Average ticket value
   - Top selling items/categories/brands
   - Payment method breakdown
   - Comparison with previous period
4. User can export as PDF/Excel or view as dashboard

---

### CU-16: Inventory Rotation Analysis [TODO] - P1

**Primary Actor:** Manager

**Main Flow:**
1. Manager accesses inventory analytics
2. System displays:
   - Average time in stock by category
   - Items stagnant for more than X days
   - Inventory turnover rate
   - Aging distribution chart
3. System suggests actions:
   - Promotional pricing for stagnant items
   - Return to supplier for expired consignments
4. Manager can generate action lists

---

### CU-17: Executive Dashboard [TODO] - P1

**Primary Actor:** Owner/Director

**Main Flow:**
1. User accesses dashboard
2. System displays real-time KPIs:
   - Daily/monthly revenue
   - Current inventory value
   - Pending supplier commissions
   - Sales by channel (physical vs. online)
   - Period-over-period comparison
   - Top 5 categories and brands
   - Consignment intake rate
   - Customer acquisition metrics

---

## MODULE 5: OMNICHANNEL INTEGRATION

### CU-18: Sync Inventory with E-commerce [TODO] - P2

**Primary Actor:** System (automated)

**Main Flow:**
1. System monitors inventory changes (new item, sale, price change)
2. System automatically syncs with e-commerce platform
3. If item sold online, system removes from physical inventory
4. System notifies staff for packaging and shipping

---

### CU-19: Publish Item to Marketplace [TODO] - P2

**Primary Actor:** E-commerce Operator

**Main Flow:**
1. Operator selects items for marketplace listing
2. System validates photos and descriptions (per marketplace standards)
3. Operator adjusts description/photos if needed
4. System publishes to integrated marketplaces (OLX, Enjoei, etc.)
5. System monitors inquiries and messages
6. When sold, system updates local inventory

---

## MODULE 6: CUSTOMER & LOYALTY

### CU-20: Register Customer [TODO] - P2

**Primary Actor:** Cashier

**Main Flow:**
1. Customer agrees to join loyalty program
2. Cashier registers: name, CPF, phone, email, birth date
3. System generates loyalty card (physical or digital)
4. System records preferences (sizes, favorite brands)

---

### CU-21: Accumulate Loyalty Points [TODO] - P2

**Primary Actor:** System (automated)

**Main Flow:**
1. At sale completion, system identifies customer
2. System calculates points (e.g., 1 point per R$10 spent)
3. System credits points to customer account
4. System notifies customer via SMS/email about accumulated points
5. Points can be redeemed for discounts on future purchases

---

## MODULE 7: ADMINISTRATION & CONFIGURATION

### CU-22: Manage Users and Permissions [TODO] - P0

**Primary Actor:** Administrator

**Main Flow:**
1. Admin creates user (employee)
2. Admin defines role: Cashier, Manager, Finance, Admin
3. System assigns permissions per role:
   - **Cashier:** POS operations, basic inventory view
   - **Manager:** Full inventory, pricing, reports, POS
   - **Finance:** Settlement, payments, financial reports
   - **Admin:** Full system access, user management
4. Admin can customize specific permissions per user

**Current Implementation:**
- Firebase Authentication exists
- Role-based permissions: NOT yet implemented (all users have same access)

---

### CU-23: Configure System Parameters [TODO] - P1

**Primary Actor:** Administrator

**Main Flow:**
1. Admin accesses system configuration
2. Admin can set:
   - Default consignment period (days)
   - Default commission percentages
   - Maximum discount without authorization
   - Loyalty points conversion rate
   - Store information (name, address, CNPJ)
   - Receipt/invoice templates
   - Fiscal integration settings
3. System saves configuration
4. Changes take effect immediately
