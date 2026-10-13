import React from 'react';
import { Pizza, Beef, Utensils, Salad, IceCream, Coffee, Sparkles } from 'lucide-react';

interface CategoryFilterProps {
  categories: string[];
  selectedCategory: string;
  onSelectCategory: (category: string) => void;
}

const categoryIcons: Record<string, React.ElementType> = {
  All: Sparkles,
  Pizza: Pizza,
  Burgers: Beef,
  Pasta: Utensils,
  Salads: Salad,
  Desserts: IceCream,
  Beverages: Coffee,
};

export const CategoryFilter: React.FC<CategoryFilterProps> = ({
  categories,
  selectedCategory,
  onSelectCategory,
}) => {
  return (
    <div
      className="flex items-center gap-2 overflow-x-auto pb-2 scrollbar-none"
      role="tablist"
      aria-label="Food Categories"
    >
      {categories.map((category) => {
        const isSelected = selectedCategory === category;
        const Icon = categoryIcons[category] || Utensils;

        return (
          <button
            key={category}
            role="tab"
            aria-selected={isSelected}
            onClick={() => onSelectCategory(category)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-2xl text-xs font-bold whitespace-nowrap transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-orange-500 select-none ${
              isSelected
                ? 'bg-orange-600 text-white shadow-md shadow-orange-600/30 scale-105'
                : 'bg-white text-stone-600 hover:bg-stone-100 hover:text-stone-900 border border-stone-200/80 shadow-sm'
            }`}
          >
            <Icon className={`w-3.5 h-3.5 ${isSelected ? 'text-white' : 'text-stone-400'}`} />
            <span>{category}</span>
          </button>
        );
      })}
    </div>
  );
};
