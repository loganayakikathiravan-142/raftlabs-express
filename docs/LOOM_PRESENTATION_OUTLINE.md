# Loom Presentation Outline (12–15 Minutes)

**Title:** Senior Full Stack Developer Assessment — Production-Quality Food Delivery Order Management System  
**Presenter:** Loganayaki  
**Target Duration:** 12 to 15 minutes  

---

## 1. Introduction & Agenda (1:30)
- **Introduction:** Candidate background, senior full-stack engineering approach, and key goals of the assessment.
- **System Overview:** Gourmet Express — an end-to-end, enterprise-ready food delivery order management platform.
- **Agenda:**
  1. Clean Architecture backend (.NET 8, EF Core, SignalR)
  2. Modern React TypeScript frontend (Vite, Tailwind, TanStack Query, Zod)
  3. Live end-to-end customer journey & real-time order tracking
  4. Kitchen administrative management & state machine lifecycle
  5. Automated testing matrix (47 backend tests + 13 frontend tests)
  6. Security safeguards, engineering decisions, and trade-offs

---

## 2. Backend Clean Architecture & Domain Design (3:00)
- **Clean Architecture Layers:**
  - `FoodDelivery.Domain`: Pure C# business rules, entities (`MenuItem`, `Order`, `OrderItem`), `OrderStatus` state machine, and domain invariants.
  - `FoodDelivery.Application`: DTOs, use cases, FluentValidation rules, and abstractions (`IOrderService`, `IApplicationDbContext`).
  - `FoodDelivery.Infrastructure`: EF Core relational persistence (SQLite & SQL Server), seeder, SignalR `OrderHub` and `OrderNotificationService`.
  - `FoodDelivery.Api`: Versioned REST APIs (`/api/v1/menu`, `/api/v1/orders`), Swagger OpenAPI, and RFC 7807 ProblemDetails middleware.
- **Domain Invariants & State Machine:**
  - Permitted transitions: `OrderReceived` → `Preparing` → `OutForDelivery` → `Delivered`.
  - Terminal states (`Delivered`, `Cancelled`) enforce strict immutability.
  - Snapshot persistence (`ItemNameSnapshot`, `UnitPriceSnapshot`) prevents future price changes from mutating historic orders.

---

## 3. Live Customer Journey Demonstration (5:00)
- **Menu Experience:**
  - High-resolution dish photography, responsive card layouts, category filtering (Pizzas, Burgers, Pastas, Salads, Desserts, Beverages), and keyword search.
  - Out-of-stock items (e.g., Seasonal Lobster Thermidor) properly display "Sold Out" badges and disabled cart controls.
- **Shopping Cart Management:**
  - Slide-over cart drawer with quantity increment, decrement, and removal.
  - Real-time recalculation of subtotals, standard $3.99 delivery fee, and grand total.
  - Persistence across browser refreshes via localStorage.
- **Checkout & Dual Validation:**
  - Demonstration of field-level client validation (Zod + React Hook Form) and matching backend rejection (FluentValidation).
  - Placing an order via `POST /api/v1/orders` and receiving the authoritative `ORD-timestamp-suffix` tracking ID.
- **Real-Time Live Order Tracking:**
  - Visual status timeline displaying progression through the 4 lifecycle stages.
  - Two browser windows side-by-side: Customer Tracker on one side, Kitchen Admin on the other.
  - Transitioning status from `OrderReceived` to `Preparing` to `OutForDelivery` to `Delivered`.
  - Watch the customer tracker update in real-time via SignalR WebSocket push with zero manual page refreshes.

---

## 4. Kitchen Administration & Operational Dashboard (2:00)
- **Kitchen Dashboard (`/admin`):**
  - Live order feed with status filter tabs and real-time subscription.
  - Contextual action buttons based on valid lifecycle paths (Start Prep, Dispatch, Mark Delivered, Cancel).
  - Terminal state locking preventing illegal transitions.

---

## 5. Automated Testing Strategy & Code Quality (2:00)
- **Backend Testing Matrix:**
  - 36 Unit Tests: Domain invariants, price calculations, state machine transitions, FluentValidation rules, and mocked service behavior.
  - 11 Integration Tests: `WebApplicationFactory` running in-memory SQLite instances verifying all REST endpoints, RFC 7807 problem details, and full customer lifecycle.
- **Frontend Testing Matrix:**
  - 13 Vitest + React Testing Library tests: Cart operations, item rendering, stock availability, and Zod form validation.
- **Total Test Count:** 60 automated tests passing with 100% green status.
- **Type Safety & Build:** Strict TypeScript check (`tsc && vite build`) and `.NET 8` compilation with 0 warnings and 0 errors.

---

## 6. Security, Architectural Trade-offs & Wrap-Up (1:30)
- **Security:** Server-authoritative pricing (never trust client totals), RFC 7807 error masking, parameterized EF Core queries, and strict CORS configuration.
- **Trade-offs:** Clean service pattern vs MediatR (chosen to avoid unnecessary mediator indirection for a focused domain while preserving full testability).
- **Future Improvements:** Multi-tenant restaurant support, payment gateway webhook integration (Stripe), and push notification services.
- **Closing:** Summary of requirements fulfilled and thank you.
