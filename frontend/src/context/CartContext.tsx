import React, { createContext, useContext, useEffect, useState } from 'react';
import { CartItem, MenuItem } from '../types';

interface CartContextType {
  items: CartItem[];
  addToCart: (item: MenuItem, quantity?: number) => void;
  updateQuantity: (menuItemId: string, newQuantity: number) => void;
  removeFromCart: (menuItemId: string) => void;
  clearCart: () => void;
  totalItems: number;
  subtotal: number;
  deliveryFee: number;
  totalAmount: number;
  isCartOpen: boolean;
  setIsCartOpen: (open: boolean) => void;
}

const CART_STORAGE_KEY = 'gourmet_express_cart_v1';
const STANDARD_DELIVERY_FEE = 3.99;

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [items, setItems] = useState<CartItem[]>(() => {
    try {
      const stored = localStorage.getItem(CART_STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  });

  const [isCartOpen, setIsCartOpen] = useState(false);

  useEffect(() => {
    try {
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
    } catch (e) {
      console.warn('Could not persist cart to localStorage', e);
    }
  }, [items]);

  const addToCart = (item: MenuItem, quantity: number = 1) => {
    if (!item.isAvailable || quantity <= 0) return;

    setItems((prevItems) => {
      const existingIndex = prevItems.findIndex((ci) => ci.item.id === item.id);
      if (existingIndex > -1) {
        const updated = [...prevItems];
        const newQty = Math.min(99, updated[existingIndex].quantity + quantity);
        updated[existingIndex] = { ...updated[existingIndex], quantity: newQty };
        return updated;
      }
      return [...prevItems, { item, quantity: Math.min(99, quantity) }];
    });
  };

  const updateQuantity = (menuItemId: string, newQuantity: number) => {
    if (newQuantity <= 0) {
      removeFromCart(menuItemId);
      return;
    }

    setItems((prevItems) =>
      prevItems.map((ci) =>
        ci.item.id === menuItemId ? { ...ci, quantity: Math.min(99, newQuantity) } : ci
      )
    );
  };

  const removeFromCart = (menuItemId: string) => {
    setItems((prevItems) => prevItems.filter((ci) => ci.item.id !== menuItemId));
  };

  const clearCart = () => {
    setItems([]);
  };

  const totalItems = items.reduce((acc, ci) => acc + ci.quantity, 0);

  const subtotal = Math.round(
    items.reduce((acc, ci) => acc + ci.item.price * ci.quantity, 0) * 100
  ) / 100;

  const deliveryFee = items.length > 0 ? STANDARD_DELIVERY_FEE : 0;

  const totalAmount = Math.round((subtotal + deliveryFee) * 100) / 100;

  return (
    <CartContext.Provider
      value={{
        items,
        addToCart,
        updateQuantity,
        removeFromCart,
        clearCart,
        totalItems,
        subtotal,
        deliveryFee,
        totalAmount,
        isCartOpen,
        setIsCartOpen,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
