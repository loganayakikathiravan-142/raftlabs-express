import React, { useState } from 'react';
import { Plus, Check, AlertCircle } from 'lucide-react';
import { MenuItem } from '../../types';
import { useCart } from '../../context/CartContext';
import { Badge } from '../../components/UI/Badge';

interface MenuItemCardProps {
  item: MenuItem;
}

export const MenuItemCard: React.FC<MenuItemCardProps> = ({ item }) => {
  const { addToCart } = useCart();
  const [justAdded, setJustAdded] = useState(false);
  const [imageError, setImageError] = useState(false);

  const fallbackImage = 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80';

  const handleAddToCart = () => {
    if (!item.isAvailable) return;
    addToCart(item, 1);
    setJustAdded(true);
    setTimeout(() => setJustAdded(false), 1200);
  };

  return (
    <article
      className="group bg-white rounded-3xl border border-stone-200/80 shadow-sm hover:shadow-xl hover:border-orange-200/80 transition-all duration-300 flex flex-col overflow-hidden"
      aria-labelledby={`item-title-${item.id}`}
    >
      {/* Food Image Container */}
      <div className="relative aspect-[16/10] overflow-hidden bg-stone-100">
        <img
          src={imageError ? fallbackImage : item.imageUrl}
          alt={`Photograph of ${item.name}`}
          onError={() => setImageError(true)}
          className={`w-full h-full object-cover transition-transform duration-500 group-hover:scale-105 ${
            !item.isAvailable ? 'grayscale opacity-75' : ''
          }`}
          loading="lazy"
        />

        {/* Category Pill */}
        <div className="absolute top-3.5 left-3.5">
          <span className="px-3 py-1 rounded-full text-[11px] font-bold bg-white/90 backdrop-blur-md text-stone-800 shadow-sm border border-stone-200/40">
            {item.category}
          </span>
        </div>

        {/* Availability Badge */}
        <div className="absolute top-3.5 right-3.5">
          {item.isAvailable ? (
            <Badge variant="success" className="bg-emerald-500/90 text-white border-0 shadow-sm">
              In Stock
            </Badge>
          ) : (
            <Badge variant="danger" className="bg-rose-500/90 text-white border-0 shadow-sm flex items-center gap-1">
              <AlertCircle className="w-3 h-3" />
              <span>Sold Out</span>
            </Badge>
          )}
        </div>
      </div>

      {/* Card Content */}
      <div className="p-5 flex-1 flex flex-col justify-between">
        <div>
          <h3
            id={`item-title-${item.id}`}
            className="text-base font-extrabold text-stone-900 group-hover:text-orange-600 transition-colors line-clamp-1"
          >
            {item.name}
          </h3>
          <p className="mt-1.5 text-xs text-stone-500 line-clamp-2 leading-relaxed">
            {item.description}
          </p>
        </div>

        {/* Card Footer: Price & Add-To-Cart */}
        <div className="mt-5 pt-3 border-t border-stone-100 flex items-center justify-between">
          <div>
            <span className="text-[11px] uppercase tracking-wider font-semibold text-stone-400 block">Price</span>
            <span className="text-lg font-extrabold text-stone-900">${item.price.toFixed(2)}</span>
          </div>

          <button
            onClick={handleAddToCart}
            disabled={!item.isAvailable}
            aria-label={
              item.isAvailable
                ? `Add ${item.name} to cart for $${item.price.toFixed(2)}`
                : `${item.name} is currently out of stock`
            }
            className={`flex items-center gap-1.5 px-4 py-2.5 rounded-2xl text-xs font-bold transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 ${
              !item.isAvailable
                ? 'bg-stone-100 text-stone-400 cursor-not-allowed border border-stone-200'
                : justAdded
                ? 'bg-emerald-600 text-white shadow-md shadow-emerald-600/30 scale-105'
                : 'bg-orange-600 hover:bg-orange-700 text-white shadow-md shadow-orange-600/25 active:scale-95 focus:ring-orange-500'
            }`}
          >
            {justAdded ? (
              <>
                <Check className="w-3.5 h-3.5" />
                <span>Added!</span>
              </>
            ) : item.isAvailable ? (
              <>
                <Plus className="w-3.5 h-3.5" />
                <span>Add to Cart</span>
              </>
            ) : (
              <span>Unavailable</span>
            )}
          </button>
        </div>
      </div>
    </article>
  );
};
