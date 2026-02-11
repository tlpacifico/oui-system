# SHS - Second Hand Shop: Product Vision Document

## 1. Overview

**Project Name:** Oui System - Second Hand Shop ERP
**Version:** 1.0
**Last Updated:** 2026-02-06
**Tech Stack:** .NET 9 / Angular 20 / PostgreSQL / Firebase Auth

---

## 2. Product Vision

Build a complete ERP system for second-hand clothing stores (brechos) that manages the full operation cycle: from receiving consigned items, through inventory management, point-of-sale, financial settlement with suppliers, and business intelligence reporting.

The system is inspired by market leaders such as Peca Rara Brecho (130+ franchises in Brazil), Vinted (Europe's largest second-hand marketplace), ThredUp (US recommerce leader), simpleconsign, and ERP Hiper (ERP with specific second-hand store modules).

---

## 3. Problem Statement

Second-hand clothing stores face unique operational challenges not addressed by generic retail ERPs:

1. **Consignment Model Complexity** - Items belong to third-party suppliers who receive payment only after sale, requiring commission tracking, settlement periods, and contract management.
2. **Unique Inventory** - Unlike traditional retail, each item is unique (one-of-a-kind), requiring individual tracking rather than SKU-based batch management.
3. **Price Lifecycle** - Items depreciate over time if not sold, requiring dynamic pricing strategies, promotional markdowns, and eventual return to supplier.
4. **Multi-Supplier Settlement** - Each sale may involve items from different suppliers with different commission rates, creating complex financial settlement workflows.
5. **Quality Assessment** - Each item requires individual condition evaluation, unlike new product retail.

---

## 4. Target Users

| Role | Description |
|------|-------------|
| **Store Owner** | Business owner managing one or multiple stores. Needs dashboards, financial reports, and strategic oversight. |
| **Store Manager** | Day-to-day operational manager. Manages inventory, pricing, staff, and supplier relationships. |
| **Cashier/Sales Associate** | Front-line staff processing sales, returns, and customer interactions at POS. |
| **Finance Staff** | Handles supplier commission calculations, payments, and financial reporting. |
| **E-commerce Operator** | Manages online listings, marketplace integrations, and omnichannel synchronization. |
| **Consignment Supplier** | External person who consigns items. Needs visibility on item status and payments. |

---

## 5. Market Reference Analysis

### 5.1 Peca Rara Brecho (Brazil)
- **Model:** Franchise-based second-hand store with 130+ units
- **Key Insight:** 100% consignment model - franchisees don't buy inventory, reducing risk
- **Revenue:** R$250M+ annual (2024), 4M+ items sold
- **Avg. Traffic:** 15-40 potential suppliers per day per store
- **Learnings for SHS:**
  - Consignment contract management is critical
  - High volume of item intake requires efficient registration workflows
  - Commission calculation and supplier settlement must be automated
  - Multi-store support is essential for scaling

### 5.2 Vinted (Europe)
- **Model:** Zero-fee peer-to-peer marketplace, 600M+ items
- **Tech:** Ruby on Rails, React, 3-stage recommender system
- **Key Insight:** Faceted search with rich filtering (condition, size, brand, color) is essential for discovery
- **Learnings for SHS:**
  - Rich product attributes (condition, brand, size, color, composition) are critical
  - Smart search and filtering drives sales
  - Recommendation engine improves conversion

### 5.3 ThredUp (US)
- **Model:** Managed recommerce with AI-powered pricing and logistics
- **Tech:** AWS/SageMaker, processes 100K+ items/day
- **Key Insight:** Data-driven pricing using 55K+ brand sales history
- **Learnings for SHS:**
  - Item identification numbering system is essential for logistics
  - Automated pricing suggestions based on brand/condition/category
  - Quality grading standardization

### 5.4 ERP Hiper (Brazil)
- **Model:** ERP specifically adapted for second-hand stores
- **Key Features:** Size/color grids, barcode labels, consignment control, fiscal integration
- **Learnings for SHS:**
  - Size and color variant management is standard
  - Barcode generation and label printing are must-haves
  - Fiscal document emission (NF-e) is legally required in Brazil

---

## 6. Core Value Propositions

1. **End-to-end consignment lifecycle** - From item receipt to supplier payment
2. **Unique item tracking** - Individual identification with generated barcodes
3. **Automated commission calculation** - Real-time commission tracking per sale
4. **Multi-channel ready** - Physical store POS + e-commerce integration
5. **Smart inventory analytics** - Rotation analysis, aging reports, pricing suggestions
6. **Multi-store support** - Centralized management for franchise-like operations

---

## 7. System Modules

| Module | Priority | Description |
|--------|----------|-------------|
| **M1 - Inventory Management** | P0 | Item registration, cataloging, barcode, search, filtering |
| **M2 - Consignment Management** | P0 | Supplier management, item intake, contracts, returns |
| **M3 - Point of Sale (POS)** | P0 | Cash register, sales processing, receipts, payment methods |
| **M4 - Financial/Settlement** | P1 | Commission calculation, supplier payments, cash flow |
| **M5 - Reports & BI** | P1 | Sales reports, inventory analytics, dashboards |
| **M6 - Customer & Loyalty** | P2 | Customer registration, points, benefits |
| **M7 - Omnichannel Integration** | P2 | E-commerce sync, marketplace publishing |
| **M8 - Administration** | P0 | Users, roles, permissions, system configuration |

---

## 8. Non-Functional Requirements

| Requirement | Target |
|-------------|--------|
| **Availability** | 99.5% uptime |
| **Response Time** | < 500ms for API calls, < 2s for page loads |
| **Concurrent Users** | Support 50+ simultaneous users per store |
| **Data Retention** | 5 years minimum for fiscal/financial data |
| **Security** | Firebase JWT authentication, role-based access control |
| **Scalability** | Support multi-store with independent or shared databases |
| **Browser Support** | Chrome, Edge, Firefox (latest 2 versions) |
| **Mobile** | Responsive design for tablet POS usage |

---

## 9. Current Implementation Status

### Already Implemented
- Clean Architecture with CQRS pattern
- Consignment CRUD (create, read, update, delete items)
- Supplier CRUD
- Item search with filters (name, price, size, brand, color, supplier, date)
- Automatic item identification number generation (`{SupplierInitial}{YYYYMM}{Sequence}`)
- Firebase JWT authentication
- Angular 20 frontend with PrimeNG
- PostgreSQL database with EF Core migrations
- Brand and Tag management
- Soft delete support
- Audit trail (CreatedBy, UpdatedBy, timestamps)

### Not Yet Implemented
- Point of Sale (POS/Cash Register)
- Financial Settlement module
- Commission payment processing
- Customer management and loyalty
- Reports and Business Intelligence dashboards
- E-commerce/marketplace integration
- Multi-store support
- Barcode printing integration
- Item transfer between stores
- Exchange/return processing
