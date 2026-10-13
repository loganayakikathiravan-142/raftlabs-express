import React from 'react';
import { Link } from 'react-router-dom';
import { ShoppingBag, ArrowLeft } from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { CheckoutForm } from './CheckoutForm';
import { CartSummary } from '../cart/CartSummary';
import { Button } from '../../components/UI/Button';

export const CheckoutPage: React.FC = () => {
  const { items } = useCart();

  if (items.length === 0) {
    return (
      <div className="max-w-md mx-auto my-12 text-center bg-white p-10 rounded-3xl border border-stone-200/80 shadow-sm space-y-4">
        <div className="w-16 h-16 rounded-2xl bg-orange-50 text-orange-600 flex items-center justify-center mx-auto">
          <ShoppingBag className="w-8 h-8" />
        </div>
        <h2 className="text-xl font-bold text-stone-900">Your basket is empty</h2>
        <p className="text-xs text-stone-500">
          You don't have any items ready for checkout. Please add dishes from our menu first.
        </p>
        <Link to="/" className="inline-block pt-2">
          <Button variant="primary" size="md" className="gap-2">
            <ArrowLeft className="w-4 h-4" />
            <span>Return to Menu</span>
          </Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-8">
      {/* Breadcrumb / Back button */}
      <div>
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-xs font-bold text-stone-500 hover:text-stone-900 transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Back to Menu</span>
        </Link>
        <h1 className="text-2xl sm:text-3xl font-black text-stone-900 tracking-tight mt-2">
          Complete Your Order
        </h1>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        {/* Form Column */}
        <div className="lg:col-span-7">
          <CheckoutForm />
        </div>

        {/* Order Summary Column */}
        <div className="lg:col-span-5 bg-white p-6 sm:p-8 rounded-3xl border border-stone-200/80 shadow-sm space-y-6">
          <h3 className="text-lg font-extrabold text-stone-900">Order Summary</h3>

          <div className="divide-y divide-stone-100 max-h-72 overflow-y-auto pr-1">
            {items.map(({ item, quantity }) => (
              <div key={item.id} className="py-3 flex items-center justify-between text-xs gap-3">
                <div className="flex items-center gap-3 min-w-0">
                  <img
                    src={item.imageUrl}
                    alt={item.name}
                    className="w-10 h-10 rounded-lg object-cover bg-stone-100 shrink-0"
                  />
                  <div className="min-w-0">
                    <p className="font-bold text-stone-900 truncate">{item.name}</p>
                    <p className="text-stone-400">Qty: {quantity} × ${item.price.toFixed(2)}</p>
                  </div>
                </div>
                <span className="font-bold text-stone-800 shrink-0">
                  ${(item.price * quantity).toFixed(2)}
                </span>
              </div>
            ))}
          </div>

          <CartSummary showCheckoutButton={false} />
        </div>
      </div>
    </div>
  );
};
