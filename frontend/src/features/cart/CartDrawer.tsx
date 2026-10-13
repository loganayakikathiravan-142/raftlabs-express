import React from 'react';
import { X, ShoppingBag, ArrowRight } from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { CartItemRow } from './CartItemRow';
import { CartSummary } from './CartSummary';
import { Button } from '../../components/UI/Button';

export const CartDrawer: React.FC = () => {
  const { items, isCartOpen, setIsCartOpen, totalItems } = useCart();

  if (!isCartOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden" role="dialog" aria-modal="true" aria-labelledby="cart-title">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-stone-900/40 backdrop-blur-sm transition-opacity"
        onClick={() => setIsCartOpen(false)}
        aria-hidden="true"
      />

      <div className="fixed inset-y-0 right-0 max-w-full flex pl-10">
        <div className="w-screen max-w-md bg-white shadow-2xl flex flex-col transform transition-transform ease-in-out duration-300">
          {/* Header */}
          <div className="p-6 border-b border-stone-100 flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <div className="p-2 bg-orange-100 text-orange-600 rounded-xl">
                <ShoppingBag className="w-5 h-5" />
              </div>
              <div>
                <h2 id="cart-title" className="text-lg font-extrabold text-stone-900">Your Basket</h2>
                <p className="text-xs text-stone-500">
                  {totalItems} {totalItems === 1 ? 'item' : 'items'} selected
                </p>
              </div>
            </div>
            <button
              onClick={() => setIsCartOpen(false)}
              className="p-2 text-stone-400 hover:text-stone-700 hover:bg-stone-100 rounded-xl transition-colors focus:outline-none"
              aria-label="Close cart drawer"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Cart Item List */}
          <div className="flex-1 overflow-y-auto p-6 divide-y divide-stone-100">
            {items.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-center py-12">
                <div className="w-20 h-20 rounded-full bg-stone-100 flex items-center justify-center text-stone-400 mb-4">
                  <ShoppingBag className="w-10 h-10" />
                </div>
                <h3 className="text-base font-bold text-stone-800">Your basket is empty</h3>
                <p className="text-xs text-stone-500 mt-1 max-w-[220px]">
                  Explore our menu of artisanal pizzas, burgers, and pastas to begin!
                </p>
                <Button
                  onClick={() => setIsCartOpen(false)}
                  variant="outline"
                  size="sm"
                  className="mt-6 gap-2"
                >
                  <span>Explore Menu</span>
                  <ArrowRight className="w-4 h-4" />
                </Button>
              </div>
            ) : (
              items.map((cartItem) => (
                <CartItemRow key={cartItem.item.id} cartItem={cartItem} />
              ))
            )}
          </div>

          {/* Footer Summary */}
          {items.length > 0 && (
            <div className="p-6 bg-stone-50 border-t border-stone-200">
              <CartSummary />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
