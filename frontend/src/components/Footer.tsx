import React from 'react';
import { UtensilsCrossed, ShieldCheck, Heart } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer className="w-full border-t border-stone-200 bg-white py-12 mt-20">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 items-center pb-8 border-b border-stone-100">
          <div className="flex items-center gap-3">
            <div className="h-9 w-9 rounded-xl bg-orange-600 flex items-center justify-center text-white">
              <UtensilsCrossed className="w-5 h-5" />
            </div>
            <div>
              <span className="font-extrabold text-stone-900 tracking-tight">RaftLabs Express</span>
              <p className="text-xs text-stone-500">Enterprise Order Management Solution</p>
            </div>
          </div>

          <div className="flex items-center md:justify-end gap-2 text-xs text-emerald-700 bg-emerald-50 px-3 py-1.5 rounded-xl border border-emerald-200/60 w-fit md:ml-auto">
            <ShieldCheck className="w-4 h-4 text-emerald-600" />
            <span className="font-medium">Server-authoritative pricing & validation active</span>
          </div>
        </div>

        <div className="mt-8 flex flex-col sm:flex-row items-center justify-between text-xs text-stone-400 gap-4">
          <p>© {new Date().getFullYear()} RaftLabs Express.</p>
          <div className="flex items-center gap-1 text-stone-500">
            Crafted with precision & care <Heart className="w-3.5 h-3.5 text-rose-500 fill-rose-500 inline" />
          </div>
        </div>
      </div>
    </footer>
  );
};
