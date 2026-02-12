# Oui System - Product Vision Document

## 1. Overview

**Project Name:** Oui System - ERP para Moda Circular & Sustentavel
**Store:** Oui Circular (moda circular & sustentavel)
**Country:** Portugal
**Currency:** Euro (EUR / €)
**Language:** pt-PT
**Version:** 1.0
**Last Updated:** 2026-02-11
**Tech Stack:** .NET 9 / Angular 20 / PostgreSQL / Firebase Auth

---

## 2. Product Vision

Build a complete ERP system for **Oui Circular**, a Portuguese second-hand clothing store operating under a circular fashion and sustainability model. The system manages the full operation cycle: from receiving items (via consignment from clients or own purchases), through inventory management, point-of-sale, financial settlement with consignment clients, and business intelligence reporting.

The store operates a **mixed acquisition model**:
- **Consignment from clients** (~62 active clients) - items received from individuals, evaluated, priced, and sold on their behalf with commission split
- **Own purchases** - items sourced directly from Humana, Vinted, H&M, and personal collection, owned outright by the store

The system is designed specifically for the Portuguese market (NIF-based identification, Portuguese invoicing/faturacao, MBWAY payments) while drawing inspiration from market leaders such as Peca Rara (Brazil's largest consignment franchise), Vinted (Europe's largest second-hand marketplace), ThredUp (US recommerce leader), and SimpleConsign (consignment management SaaS).

---

## 3. Problem Statement

Second-hand clothing stores with a mixed consignment/own-purchase model face unique operational challenges not addressed by generic retail ERPs:

1. **Mixed Acquisition Model Complexity** - The store operates with two distinct item flows: consignment items (belonging to third-party clients who receive payment only after sale) and own-purchase items (bought outright from various sources). Each flow has different financial tracking, pricing, and settlement rules.
2. **Consignment Commission Management** - Consignment clients choose between two settlement options: **40% of sale price in cash** or **50% of sale price in store credit**. The store retains 60% or 50% respectively. This requires per-client, per-item tracking of commission preferences and payment status.
3. **Deferred Evaluation Workflow** - Unlike stores that evaluate in front of the client, Oui Circular receives items, counts them, has the client sign a receipt, and evaluates later. This creates a unique status flow requiring a "Recebido" (Received) status before evaluation.
4. **Unique Inventory** - Each item is one-of-a-kind, requiring individual tracking rather than SKU-based batch management. Current stock: ~519 items, average price €12.55, range €2-€100.
5. **Price Lifecycle** - Items may depreciate over time if not sold, requiring dynamic pricing strategies, promotional markdowns, and eventual return to client.
6. **Multi-Channel Sales** - Items are sold across physical store, Instagram, and Vinted, requiring synchronized inventory and channel-specific workflows.
7. **Portuguese Regulatory Compliance** - The system must comply with Portuguese invoicing requirements (faturacao portuguesa), NIF-based client identification, and support MBWAY as a payment method.

---

## 4. Target Users

| Role | Description |
|------|-------------|
| **Store Owner** | Owner of Oui Circular. Needs dashboards, financial reports, settlement oversight, and strategic decision-making tools. |
| **Store Manager** | Day-to-day operations: inventory management, pricing, client relationships, and item evaluation. |
| **Sales Associate** | Front-line staff processing sales, handling customer interactions at POS, and managing Instagram/Vinted listings. |
| **Finance/Settlements** | Handles consignment commission calculations, client payments (cash or store credit), and financial reporting. |
| **Consignment Client** | External person who consigns items. Needs visibility on item status (received, evaluated, on sale, sold, paid) and settlement amounts. Currently ~62 active clients. |

---

## 5. Market Reference Analysis

> **Note:** Oui Circular operates in **Portugal**. The references below are drawn from international market leaders to inform system design, but all system features, regulatory compliance, and operational workflows are built for the Portuguese market.

### 5.1 Peca Rara (Brazil)
- **Model:** Franchise-based second-hand store with 130+ units in Brazil
- **Key Insight:** 100% consignment model - franchisees don't buy inventory, reducing risk
- **Relevance to Oui System:**
  - Consignment contract management workflow is a strong reference
  - High volume item intake registration workflows are relevant as Oui scales
  - Commission calculation and supplier settlement automation is critical
  - Their item lifecycle states informed our status design

### 5.2 Vinted (Europe)
- **Model:** Zero-fee peer-to-peer marketplace, 600M+ items across Europe
- **Tech:** Ruby on Rails, React, 3-stage recommender system
- **Key Insight:** Faceted search with rich filtering (condition, size, brand, color) is essential for discovery
- **Relevance to Oui System:**
  - Vinted is also a **sales channel** for Oui Circular (items are cross-listed)
  - Rich product attributes (condition, brand, size, color, composition) are critical
  - Smart search and filtering drives in-store and online sales
  - Integration with Vinted as a marketplace is a future goal

### 5.3 ThredUp (US)
- **Model:** Managed recommerce with AI-powered pricing and logistics
- **Tech:** AWS/SageMaker, processes 100K+ items/day
- **Key Insight:** Data-driven pricing using 55K+ brand sales history
- **Relevance to Oui System:**
  - Item identification numbering system is essential for logistics
  - Automated pricing suggestions based on brand/condition/category
  - Quality grading standardization for evaluation workflow

### 5.4 SimpleConsign (SaaS)
- **Model:** Cloud-based consignment store management system
- **Key Features:** Consignor management, automated settlements, barcode labels, reporting
- **Relevance to Oui System:**
  - Settlement workflow (cash vs. store credit) is a direct reference
  - Consignor portal for item status visibility
  - Commission rate flexibility per consignor

---

## 6. Core Value Propositions

1. **End-to-end consignment lifecycle** - From item receipt (Recebido) through evaluation, sale, to client settlement (cash or store credit)
2. **Mixed acquisition support** - Seamless handling of both consignment items and own-purchase items in a single inventory
3. **Unique item tracking** - Individual identification with generated codes for every item
4. **Automated commission calculation** - Real-time commission tracking with support for 40% cash / 50% store credit split
5. **Multi-channel ready** - Physical store POS + Instagram + Vinted integration
6. **Smart inventory analytics** - Rotation analysis, aging reports, pricing suggestions for ~519-item catalog
7. **Portuguese compliance** - NIF-based identification, Portuguese invoicing (faturacao), MBWAY payments
8. **Client communication** - Email for evaluation notifications, WhatsApp for settlement communications

---

## 7. Business Model Details

### 7.1 Acquisition Sources

| Source | Type | Description |
|--------|------|-------------|
| **Consignment Clients** | Consignment | ~62 active clients who bring items to sell on commission |
| **Humana** | Own Purchase | Bulk purchases from Humana second-hand stores |
| **Vinted** | Own Purchase | Individual items sourced from Vinted marketplace |
| **H&M** | Own Purchase | Items from H&M (likely conscious/recycling program) |
| **Personal Collection** | Own Purchase | Owner's personal clothing collection |

### 7.2 Commission Structure (Consignment)

| Option | Client Receives | Store Retains |
|--------|----------------|---------------|
| **Cash Settlement** | 40% of sale price | 60% of sale price |
| **Store Credit** | 50% of sale price | 50% of sale price |

### 7.3 Inventory Profile

| Metric | Value |
|--------|-------|
| **Total Items in Stock** | ~519 |
| **Average Price** | €12.55 |
| **Price Range** | €2 - €100 |
| **Top Brands** | Zara, H&M, Mango, Stradivarius, Lefties, Cortefiel |

### 7.4 Sales Channels

| Channel | Description |
|---------|-------------|
| **Physical Store** | Primary sales channel, in-person browsing and POS |
| **Instagram** | Social selling, item showcasing, customer engagement |
| **Vinted** | Online marketplace cross-listing for broader reach |

### 7.5 Communication Channels

| Purpose | Channel |
|---------|---------|
| **Evaluation Results** | Email (notify client when items have been evaluated) |
| **Settlement/Payments** | WhatsApp (coordinate cash or credit settlements) |

---

## 8. Item Lifecycle & Status Flow

### 8.1 Consignment Item Flow

The consignment workflow at Oui Circular has a distinctive characteristic: **items are NOT evaluated in front of the client**. Instead:

1. Client brings items to the store
2. Items are received and counted
3. Client signs a receipt confirming the quantity of items left
4. Items are evaluated later by the store team (condition, brand, pricing)
5. Client is notified of evaluation results via email

### 8.2 Status Codes

| Code | Status | Description |
|------|--------|-------------|
| **RC** | Recebido | Item received from client, counted, receipt signed. Awaiting evaluation. |
| **AV** | Avaliado | Item evaluated (condition assessed, price set). Ready to be made available. |
| **CS** | Consignado | Item accepted for consignment, terms agreed. |
| **DL** | Disponivel em Loja | Item is on the shop floor, available for sale. |
| **VD** | Vendido | Item has been sold. Commission calculation triggered. |
| **PG** | Pago | Client has been paid (cash or store credit applied). Settlement complete. |
| **CD** | Com Defeito | Item found to have a defect during evaluation. Client to be notified. |
| **DV** | Devolvido | Item returned to the client (unsold, defective, or contract expired). |

### 8.3 Status Flow Diagram

```
Consignment Items:
  RC (Recebido) → AV (Avaliado) → CS (Consignado) → DL (Disponivel em Loja) → VD (Vendido) → PG (Pago)
                        ↓                                      ↓
                    CD (Com Defeito)                      DV (Devolvido)
                        ↓
                    DV (Devolvido)

Own-Purchase Items:
  AV (Avaliado) → DL (Disponivel em Loja) → VD (Vendido)
```

---

## 9. System Modules

| Module | Priority | Description |
|--------|----------|-------------|
| **M1 - Inventory Management** | P0 | Item registration, cataloging, barcode/ID generation, search, filtering. Support for ~519 items with brands like Zara, H&M, Mango. |
| **M2 - Consignment Management** | P0 | Client management (~62 clients), item intake with receipt, deferred evaluation workflow, returns, commission tracking (40% cash / 50% credit). |
| **M3 - Point of Sale (POS)** | P0 | Cash register, sales processing, receipts, payment methods (cash, card, MBWAY). Portuguese invoicing compliance (faturacao). |
| **M4 - Financial/Settlement** | P1 | Commission calculation (40/60 or 50/50 split), client payments, cash flow, settlement via WhatsApp coordination. |
| **M5 - Reports & BI** | P1 | Sales reports, inventory analytics, dashboards, brand performance, channel performance (store vs. Instagram vs. Vinted). |
| **M6 - Customer & Loyalty** | P2 | Customer registration (NIF-based), store credit tracking, loyalty program. |
| **M7 - Omnichannel Integration** | P2 | Instagram catalog sync, Vinted cross-listing, inventory synchronization across channels. |
| **M8 - Administration** | P0 | Users, roles, permissions, system configuration, commission rate settings. |

---

## 10. Non-Functional Requirements

| Requirement | Target |
|-------------|--------|
| **Availability** | 99.5% uptime |
| **Response Time** | < 500ms for API calls, < 2s for page loads |
| **Concurrent Users** | Support 10+ simultaneous users (single store operation) |
| **Data Retention** | 5 years minimum for financial/invoicing data (Portuguese legal requirement) |
| **Security** | Firebase JWT authentication, role-based access control |
| **Localization** | pt-PT language, Euro (€) currency, Portuguese date/number formats |
| **Regulatory** | NIF validation, Portuguese invoicing (faturacao) compliance, GDPR compliance |
| **Payment Methods** | Cash, Card (Multibanco/VISA/Mastercard), MBWAY |
| **Browser Support** | Chrome, Edge, Firefox (latest 2 versions) |
| **Mobile** | Responsive design for tablet POS usage |

---

## 11. Current Implementation Status

### Already Implemented
- Clean Architecture with CQRS pattern
- Consignment CRUD (create, read, update, delete items)
- Supplier/Client CRUD
- Item search with filters (name, price, size, brand, color, supplier, date)
- Automatic item identification number generation (`{SupplierInitial}{YYYYMM}{Sequence}`)
- Firebase JWT authentication
- Angular 20 frontend with PrimeNG
- PostgreSQL database with EF Core migrations
- Brand and Tag management
- Soft delete support
- Audit trail (CreatedBy, UpdatedBy, timestamps)

### Not Yet Implemented
- "Recebido" status and deferred evaluation workflow
- Full item status lifecycle (RC → AV → CS → DL → VD → PG / CD / DV)
- Point of Sale (POS/Cash Register)
- MBWAY payment integration
- Portuguese invoicing (faturacao) compliance
- NIF validation and client identification
- Financial Settlement module (40% cash / 50% store credit)
- Commission payment processing
- Customer management and loyalty / store credit tracking
- Reports and Business Intelligence dashboards
- Instagram catalog integration
- Vinted cross-listing integration
- Barcode/label printing integration
- Email notifications for evaluation results
- WhatsApp integration for settlement communication
- Exchange/return processing
