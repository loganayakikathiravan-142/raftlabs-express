import React from 'react';
import { Plus, Minus, Trash2 } from 'lucide-react';
import { CartItem } from '../../types';
import { useCart } from '../../context/CartContext';

interface CartItemRowProps {
  cartItem: CartItem;
}

export const CartItemRow: React.FC<CartItemRowProps> = ({ cartItem }) => {
  const { updateQuantity, removeFromCart } = useCart();
  const { item, quantity } = cartItem;

  const lineTotal = Math.round(item.price * quantity * 100) / 100;

  return (
    <div className="flex items-center gap-3.5 py-4 border-b border-stone-100 last:border-0 group">
      {/* Thumbnail */}
      <img
        src={item.imageUrl}
        alt={item.name}
        className="w-16 h-16 rounded-xl object-cover shrink-0 bg-stone-100 shadow-sm"
        loading="lazy"
      />

      {/* Item info */}
      <div className="flex-1 min-w-0">
        <h4 className="text-sm font-bold text-stone-900 truncate">{item.name}</h4>
        <p className="text-xs text-stone-500 mt-0.5">${item.price.toFixed(2)} each</p>

        {/* Quantity Controls */}
        <div className="flex items-center gap-2 mt-2">
          <div className="flex items-center border border-stone-200 rounded-lg bg-stone-50 overflow-hidden">
            <button
              onClick={() => updateQuantity(item.id, quantity - 1)}
              aria-label={`Decrease quantity of ${item.name}`}
              className="p-1 hover:bg-stone-200 text-stone-600 transition-colors focus:outline-none"
            >
              <Minus className="w-3.5 h-3.5" />
            </button>
            <span className="px-2.5 text-xs font-bold text-stone-800 tabular-nums">
              {quantity}
            </span>
            <button
              onClick={() => updateQuantity(item.id, quantity + 1)}
              aria-label={`Increase quantity of ${item.name}`}
              className="p-1 hover:bg-stone-200 text-stone-600 transition-colors focus:outline-none"
            >
              <Plus className="w-3.5 h-3.5" />
            </button>
          </div>

          <button
            onClick={() => removeFromCart(item.id)}
            aria-label={`Remove ${item.name} from cart`}
            className="text-stone-400 hover:text-rose-600 p-1 rounded-md transition-colors"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Subtotal */}
      <div className="text-right shrink-0">
        <span className="text-sm font-extrabold text-stone-900">${lineTotal.toFixed(2)}</span>
      </div>
    </div>
  );
};
