import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AlertCircle, RefreshCw, Sparkles, SearchX } from 'lucide-react';
import { menuService } from '../../services/menuService';
import { CategoryFilter } from './CategoryFilter';
import { SearchBar } from './SearchBar';
import { MenuItemCard } from './MenuItemCard';
import { Button } from '../../components/UI/Button';

const CATEGORIES = ['All', 'Pizza', 'Burgers', 'Pasta', 'Salads', 'Desserts', 'Beverages'];

export const MenuList: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [searchQuery, setSearchQuery] = useState<string>('');

  const {
    data: items = [],
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['menu', selectedCategory, searchQuery],
    queryFn: () => menuService.getMenuItems(selectedCategory, searchQuery),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  return (
    <div className="space-y-8">
      {/* Hero Banner */}
      <section className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-stone-900 via-stone-800 to-orange-950 p-8 sm:p-12 text-white shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-4">
          <div className="inline-flex items-center gap-2 rounded-full bg-orange-500/20 px-3 py-1 text-xs font-bold text-orange-300 border border-orange-500/30">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Handcrafted Culinary Flavors</span>
          </div>
          <h1 className="text-3xl sm:text-5xl font-black tracking-tight leading-tight">
            Craving exceptional food delivered to your door?
          </h1>
          <p className="text-sm sm:text-base text-stone-300 leading-relaxed">
            Order stone-baked artisan pizzas, prime Angus burgers, hand-spun pastas, and sweet delicacies. Prepared fresh upon order.
          </p>
        </div>

        {/* Decorative background glow */}
        <div className="absolute -right-20 -bottom-20 w-96 h-96 bg-orange-600/20 rounded-full blur-3xl pointer-events-none" />
      </section>

      {/* Filter and Search Bar Section */}
      <div className="space-y-4">
        <div className="flex flex-col sm:flex-row gap-4 items-stretch sm:items-center justify-between">
          <div className="w-full sm:max-w-md">
            <SearchBar value={searchQuery} onChange={setSearchQuery} />
          </div>
          <div className="text-xs font-bold text-stone-500 text-right shrink-0">
            {!isLoading && (
              <span>
                Showing <strong className="text-stone-900">{items.length}</strong> delicious items
              </span>
            )}
          </div>
        </div>

        <CategoryFilter
          categories={CATEGORIES}
          selectedCategory={selectedCategory}
          onSelectCategory={setSelectedCategory}
        />
      </div>

      {/* Content State Handling */}
      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="bg-white rounded-3xl border border-stone-100 p-4 space-y-4 animate-pulse">
              <div className="aspect-[16/10] bg-stone-200 rounded-2xl" />
              <div className="h-5 bg-stone-200 rounded-md w-3/4" />
              <div className="h-3 bg-stone-100 rounded-md w-full" />
              <div className="h-3 bg-stone-100 rounded-md w-2/3" />
              <div className="flex justify-between items-center pt-2">
                <div className="h-6 bg-stone-200 rounded-md w-16" />
                <div className="h-9 bg-stone-200 rounded-xl w-24" />
              </div>
            </div>
          ))}
        </div>
      ) : isError ? (
        <div className="rounded-3xl border border-rose-200 bg-rose-50 p-10 text-center space-y-4">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-rose-100 text-rose-600">
            <AlertCircle className="h-7 w-7" />
          </div>
          <div className="max-w-md mx-auto space-y-1">
            <h3 className="text-lg font-bold text-rose-900">Unable to load food menu</h3>
            <p className="text-xs text-rose-700">
              {(error as Error)?.message || 'We encountered an unexpected network error. Please try again.'}
            </p>
          </div>
          <Button onClick={() => refetch()} variant="danger" size="sm" className="gap-2">
            <RefreshCw className="w-4 h-4" />
            <span>Retry Connection</span>
          </Button>
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-3xl border border-stone-200 bg-white p-12 text-center space-y-4">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-stone-100 text-stone-400">
            <SearchX className="h-8 w-8" />
          </div>
          <div className="max-w-sm mx-auto space-y-1">
            <h3 className="text-base font-bold text-stone-900">No matching dishes found</h3>
            <p className="text-xs text-stone-500">
              Try adjusting your category filter or search terms to explore available items.
            </p>
          </div>
          <Button
            onClick={() => {
              setSelectedCategory('All');
              setSearchQuery('');
            }}
            variant="outline"
            size="sm"
          >
            Clear Filters
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {items.map((item) => (
            <MenuItemCard key={item.id} item={item} />
          ))}
        </div>
      )}
    </div>
  );
};
