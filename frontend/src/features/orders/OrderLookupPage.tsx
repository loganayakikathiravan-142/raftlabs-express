import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, PackageCheck } from 'lucide-react';
import { Button } from '../../components/UI/Button';

export const OrderLookupPage: React.FC = () => {
  const [orderNumberInput, setOrderNumberInput] = useState('');
  const navigate = useNavigate();

  const handleLookup = (e: React.FormEvent) => {
    e.preventDefault();
    const clean = orderNumberInput.trim();
    if (clean) {
      navigate(`/track/${encodeURIComponent(clean)}`);
    }
  };

  return (
    <div className="max-w-lg mx-auto my-12 bg-white p-8 rounded-3xl border border-stone-200/80 shadow-sm space-y-6">
      <div className="text-center space-y-2">
        <div className="w-14 h-14 bg-orange-100 text-orange-600 rounded-2xl flex items-center justify-center mx-auto">
          <PackageCheck className="w-7 h-7" />
        </div>
        <h1 className="text-2xl font-black text-stone-900 tracking-tight">Track Your Delivery</h1>
        <p className="text-xs text-stone-500 max-w-sm mx-auto">
          Enter your unique order tracking number from your confirmation screen to see live preparation & courier status.
        </p>
      </div>

      <form onSubmit={handleLookup} className="space-y-4">
        <div>
          <label htmlFor="order-num-input" className="block text-xs font-bold uppercase text-stone-700 mb-1.5">
            Order Number
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-stone-400">
              <Search className="w-4 h-4" />
            </div>
            <input
              id="order-num-input"
              type="text"
              value={orderNumberInput}
              onChange={(e) => setOrderNumberInput(e.target.value)}
              placeholder="e.g. ORD-20261013-1234"
              className="w-full pl-10 pr-4 py-3 bg-white border border-stone-200 rounded-2xl text-sm font-semibold tracking-wider text-stone-900 placeholder:text-stone-400 focus:outline-none focus:ring-2 focus:ring-orange-500"
              required
            />
          </div>
        </div>

        <Button type="submit" variant="primary" size="lg" className="w-full">
          Track Live Status
        </Button>
      </form>

      <div className="pt-4 border-t border-stone-100 text-center">
        <p className="text-xs text-stone-400">
          Orders refresh automatically through WebSocket events without manual reloading.
        </p>
      </div>
    </div>
  );
};
