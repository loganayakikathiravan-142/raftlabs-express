import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, ShieldCheck, Clock } from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { Button } from '../../components/UI/Button';

interface CartSummaryProps {
  onCheckoutClick?: () => void;
  showCheckoutButton?: boolean;
}

export const CartSummary: React.FC<CartSummaryProps> = ({
  onCheckoutClick,
  showCheckoutButton = true,
}) => {
  const { items, subtotal, deliveryFee, totalAmount, setIsCartOpen } = useCart();
  const navigate = useNavigate();

  const handleCheckout = () => {
    setIsCartOpen(false);
    if (onCheckoutClick) {
      onCheckoutClick();
    } else {
      navigate('/checkout');
    }
  };

  if (items.length === 0) return null;

  return (
    <div className="space-y-4 pt-4 border-t border-stone-200">
      <div className="space-y-2 text-sm">
        <div className="flex justify-between text-stone-600">
          <span>Subtotal</span>
          <span className="font-semibold text-stone-900">${subtotal.toFixed(2)}</span>
        </div>
        <div className="flex justify-between text-stone-600">
          <div className="flex items-center gap-1.5">
            <span>Standard Delivery</span>
            <span className="text-[11px] bg-stone-100 text-stone-600 px-1.5 py-0.5 rounded">Flat</span>
          </div>
          <span className="font-semibold text-stone-900">${deliveryFee.toFixed(2)}</span>
        </div>
        <div className="pt-2 border-t border-stone-100 flex justify-between text-base font-extrabold text-stone-900">
          <span>Total</span>
          <span className="text-orange-600 text-lg">${totalAmount.toFixed(2)}</span>
        </div>
      </div>

      <div className="flex items-center justify-between text-xs text-stone-500 py-1 bg-stone-50 px-3 rounded-xl border border-stone-100">
        <div className="flex items-center gap-1.5">
          <Clock className="w-3.5 h-3.5 text-stone-400" />
          <span>Est. Delivery: 25–35 min</span>
        </div>
        <div className="flex items-center gap-1 text-emerald-600 font-medium">
          <ShieldCheck className="w-3.5 h-3.5" />
          <span>Fresh Guarantee</span>
        </div>
      </div>

      {showCheckoutButton && (
        <Button
          onClick={handleCheckout}
          variant="primary"
          size="lg"
          className="w-full gap-2 shadow-lg shadow-orange-600/20"
        >
          <span>Proceed to Checkout</span>
          <ArrowRight className="w-4 h-4" />
        </Button>
      )}
    </div>
  );
};
