import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ShoppingBag, UtensilsCrossed, Search, ChefHat } from 'lucide-react';
import { useCart } from '../context/CartContext';

export const Navbar: React.FC = () => {
  const { totalItems, subtotal, setIsCartOpen } = useCart();
  const location = useLocation();

  const navLinks = [
    { to: '/', label: 'Menu' },
    { to: '/track', label: 'Track Order' },
    { to: '/admin', label: 'Kitchen Admin' },
  ];

  return (
    <header className="sticky top-0 z-40 w-full border-b border-stone-200/80 bg-white/90 backdrop-blur-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-20 flex items-center justify-between gap-4">
        {/* Brand */}
        <Link to="/" className="flex items-center gap-3 group focus:outline-none focus:ring-2 focus:ring-orange-500 rounded-xl p-1">
          <div className="h-11 w-11 rounded-2xl bg-orange-600 flex items-center justify-center text-white shadow-md shadow-orange-600/30 group-hover:scale-105 transition-transform">
            <UtensilsCrossed className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xl font-extrabold tracking-tight text-stone-900 group-hover:text-orange-600 transition-colors">
              RaftLabs<span className="text-orange-600">Express</span>
            </span>
            <span className="block text-[11px] font-semibold text-stone-400 uppercase tracking-wider">
              Artisanal Food Delivery
            </span>
          </div>
        </Link>

        {/* Navigation links */}
        <nav className="hidden md:flex items-center gap-1 bg-stone-100 p-1.5 rounded-2xl border border-stone-200/60" aria-label="Main Navigation">
          {navLinks.map((link) => {
            const isActive = location.pathname === link.to;
            return (
              <Link
                key={link.to}
                to={link.to}
                className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all ${
                  isActive
                    ? 'bg-white text-stone-900 shadow-sm'
                    : 'text-stone-600 hover:text-stone-900 hover:bg-stone-200/60'
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

        {/* Action Controls */}
        <div className="flex items-center gap-3">
          <Link
            to="/track"
            className="md:hidden p-2.5 rounded-xl text-stone-600 hover:bg-stone-100 focus:outline-none"
            title="Track Order"
          >
            <Search className="w-5 h-5" />
          </Link>
          <Link
            to="/admin"
            className="md:hidden p-2.5 rounded-xl text-stone-600 hover:bg-stone-100 focus:outline-none"
            title="Kitchen Admin"
          >
            <ChefHat className="w-5 h-5" />
          </Link>

          {/* Cart Trigger */}
          <button
            onClick={() => setIsCartOpen(true)}
            aria-label={`Open shopping cart with ${totalItems} items. Subtotal: $${subtotal.toFixed(2)}`}
            className="relative flex items-center gap-2.5 bg-stone-900 hover:bg-stone-800 text-white px-4 py-2.5 rounded-2xl shadow-md shadow-stone-900/10 active:scale-95 transition-all focus:outline-none focus:ring-2 focus:ring-stone-900 focus:ring-offset-2"
          >
            <div className="relative">
              <ShoppingBag className="w-5 h-5 text-orange-400" />
              {totalItems > 0 && (
                <span className="absolute -top-2 -right-2 bg-orange-600 text-white text-[10px] font-extrabold h-4 w-4 rounded-full flex items-center justify-center animate-bounce">
                  {totalItems}
                </span>
              )}
            </div>
            <div className="hidden sm:flex flex-col items-start text-left">
              <span className="text-[10px] uppercase font-bold text-stone-400 leading-tight">Cart</span>
              <span className="text-xs font-bold text-white leading-tight">${subtotal.toFixed(2)}</span>
            </div>
          </button>
        </div>
      </div>
    </header>
  );
};
