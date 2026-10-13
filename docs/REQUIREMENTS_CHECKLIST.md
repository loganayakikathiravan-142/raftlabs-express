# Assessment Requirements Traceability Matrix

| # | Requirement | Status | Implementation Details & File Mappings | Automated Verification |
|:---|:---|:---:|:---|:---|
| 1 | Display a food menu containing realistic food items | ✅ Complete | `DatabaseSeeder.cs`, `MenuService.cs`, `MenuController.cs`, `MenuList.tsx` | `MenuEndpointsTests.cs`, `MenuRendering.test.tsx` |
| 2 | Display each item's name, description, price, image, and availability | ✅ Complete | `MenuItem.cs`, `MenuItemDto.cs`, `MenuItemCard.tsx` (In Stock & Sold Out badges) | `MenuRendering.test.tsx`, `MenuEndpointsTests.cs` |
| 3 | Allow users to add food items to a cart | ✅ Complete | `CartContext.tsx`, `CartDrawer.tsx`, `MenuItemCard.tsx` | `CartContext.test.tsx` |
| 4 | Allow quantity increases, decreases, and item removal | ✅ Complete | `CartContext.tsx` (`updateQuantity`, `removeFromCart`), `CartItemRow.tsx` | `CartContext.test.tsx` |
| 5 | Calculate item subtotals, order totals, and explicitly configured delivery fee | ✅ Complete | `Order.cs` (`RecalculateTotals`), `CartContext.tsx`, `CartSummary.tsx` ($3.99 delivery fee) | `OrderDomainTests.cs`, `CartContext.test.tsx`, `OrderEndpointsTests.cs` |
| 6 | Provide a checkout form containing customer name, delivery address, and phone number | ✅ Complete | `CheckoutForm.tsx`, `CheckoutPage.tsx`, `Input.tsx` | `CheckoutValidation.test.tsx` |
| 7 | Validate checkout input on both frontend and backend | ✅ Complete | **Frontend:** Zod + React Hook Form (`CheckoutForm.tsx`)<br>**Backend:** FluentValidation (`CreateOrderRequestValidator.cs`) | `CreateOrderRequestValidatorTests.cs`, `CheckoutValidation.test.tsx` |
| 8 | Place orders through a real REST API | ✅ Complete | `OrdersController.cs` (`POST /api/v1/orders`), `orderService.ts` | `OrderEndpointsTests.cs`, `FullCustomerJourneyEndToEndTests.cs` |
| 9 | Persist menu items and orders in a database | ✅ Complete | Entity Framework Core (`ApplicationDbContext.cs`, `DatabaseSeeder.cs`, SQLite / SQL Server) | `OrderServiceTests.cs`, `OrderEndpointsTests.cs` |
| 10 | Display order confirmation and a unique order identifier | ✅ Complete | `Order.GenerateOrderNumber` (`ORD-timestamp-suffix`), `OrderTrackingPage.tsx` | `OrderDomainTests.cs`, `FullCustomerJourneyEndToEndTests.cs` |
| 11 | Display order status (Order Received, Preparing, Out for Delivery, Delivered) | ✅ Complete | `OrderStatus.cs` enum, `StatusTimeline.tsx` responsive animated step progress | `OrderDomainTests.cs`, `OrderEndpointsTests.cs` |
| 12 | Provide a functional order status update mechanism with automatic UI refresh | ✅ Complete | **SignalR:** `OrderHub.cs`, `OrderNotificationService.cs`, `signalrService.ts`<br>**Fallback:** Smart polling interval with auto-stop on terminal state | `FullCustomerJourneyEndToEndTests.cs`, `OrderEndpointsTests.cs` |
| 13 | Implement CRUD operations for orders wherever permitted by order lifecycle | ✅ Complete | `GET /orders`, `GET /orders/{id}`, `GET /orders/by-number`, `POST /orders`, `PATCH /orders/{id}/status`, `PUT /orders/{id}`, `DELETE /orders/{id}` | `OrderEndpointsTests.cs`, `FullCustomerJourneyEndToEndTests.cs` |
| 14 | Implement meaningful backend and frontend automated tests | ✅ Complete | **Backend:** 36 Unit Tests + 11 Integration Tests (xUnit + FluentAssertions + Moq + WebApplicationFactory)<br>**Frontend:** 13 Vitest + RTL tests | `dotnet test` (47 passed), `npm run test` (13 passed) |
| 15 | Provide a responsive, polished, accessible user interface | ✅ Complete | Tailwind CSS, Plus Jakarta Sans, Lucide icons, accessible ARIA attributes, semantic markup | Verified in build and test |
| 16 | Include complete setup instructions, environment configuration, and API documentation | ✅ Complete | `README.md`, `appsettings.example.json`, `.env.example`, `docs/API_COLLECTION.http`, Swagger UI (`/swagger`) | Verified |
| 17 | Prepare the application for GitHub publication and hosting | ✅ Complete | Git repository initialized, `.gitignore`, Dockerfile, clean clone instructions | Verified |
| 18 | Document architecture, engineering decisions, testing, AI assistance, and trade-offs | ✅ Complete | `README.md`, `docs/ARCHITECTURE.md`, `docs/DEPLOYMENT_GUIDE.md`, `docs/LOOM_PRESENTATION_OUTLINE.md` | Verified |
