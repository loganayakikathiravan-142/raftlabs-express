import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Navbar } from '../components/Navbar';
import { Footer } from '../components/Footer';
import { CartDrawer } from '../features/cart/CartDrawer';
import { MenuList } from '../features/menu/MenuList';
import { CheckoutPage } from '../features/checkout/CheckoutPage';
import { OrderTrackingPage } from '../features/orders/OrderTrackingPage';
import { OrderLookupPage } from '../features/orders/OrderLookupPage';
import { AdminOrdersPage } from '../features/admin/AdminOrdersPage';

export const App: React.FC = () => {
  return (
    <div className="flex flex-col min-h-screen bg-stone-50">
      <Navbar />

      <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-10">
        <Routes>
          <Route path="/" element={<MenuList />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/track" element={<OrderLookupPage />} />
          <Route path="/track/:orderNumber" element={<OrderTrackingPage />} />
          <Route path="/order-confirmation/:orderNumber" element={<OrderTrackingPage />} />
          <Route path="/admin" element={<AdminOrdersPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>

      <Footer />
      <CartDrawer />
    </div>
  );
};
