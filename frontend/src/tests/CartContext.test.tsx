import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import { CartProvider, useCart } from '../context/CartContext';
import { MenuItem } from '../types';

const mockPizza: MenuItem = {
  id: 'item-1',
  name: 'Margherita Pizza',
  description: 'Classic pizza',
  price: 15.0,
  category: 'Pizza',
  imageUrl: 'https://example.com/pizza.jpg',
  isAvailable: true,
  createdAtUtc: new Date().toISOString(),
};

const mockBurger: MenuItem = {
  id: 'item-2',
  name: 'Truffle Burger',
  description: 'Juicy burger',
  price: 12.5,
  category: 'Burgers',
  imageUrl: 'https://example.com/burger.jpg',
  isAvailable: true,
  createdAtUtc: new Date().toISOString(),
};

const mockUnavailableItem: MenuItem = {
  id: 'item-3',
  name: 'Sold Out Item',
  description: 'Not available',
  price: 20.0,
  category: 'Pasta',
  imageUrl: 'https://example.com/pasta.jpg',
  isAvailable: false,
  createdAtUtc: new Date().toISOString(),
};

const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <CartProvider>{children}</CartProvider>
);

describe('CartContext', () => {
  it('starts with an empty cart and zero delivery fee', () => {
    const { result } = renderHook(() => useCart(), { wrapper });
    expect(result.current.items).toEqual([]);
    expect(result.current.totalItems).toBe(0);
    expect(result.current.subtotal).toBe(0);
    expect(result.current.deliveryFee).toBe(0);
    expect(result.current.totalAmount).toBe(0);
  });

  it('adds an item to cart and correctly computes subtotals and delivery fee', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 2);
    });

    expect(result.current.items).toHaveLength(1);
    expect(result.current.items[0].quantity).toBe(2);
    expect(result.current.totalItems).toBe(2);
    expect(result.current.subtotal).toBe(30.0);
    expect(result.current.deliveryFee).toBe(3.99);
    expect(result.current.totalAmount).toBe(33.99);
  });

  it('increments quantity when existing item is added again', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 1);
    });
    act(() => {
      result.current.addToCart(mockPizza, 2);
    });

    expect(result.current.items).toHaveLength(1);
    expect(result.current.items[0].quantity).toBe(3);
    expect(result.current.subtotal).toBe(45.0);
  });

  it('does not add unavailable items', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockUnavailableItem, 1);
    });

    expect(result.current.items).toHaveLength(0);
  });

  it('updates quantity and recalculates totals', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 1);
      result.current.addToCart(mockBurger, 1);
    });

    expect(result.current.subtotal).toBe(27.5);

    act(() => {
      result.current.updateQuantity(mockPizza.id, 3);
    });

    // 3 * 15.0 + 1 * 12.5 = 45.0 + 12.5 = 57.5
    expect(result.current.subtotal).toBe(57.5);
    expect(result.current.totalAmount).toBe(61.49);
  });

  it('removes item when quantity is reduced to 0', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 1);
    });
    expect(result.current.items).toHaveLength(1);

    act(() => {
      result.current.updateQuantity(mockPizza.id, 0);
    });

    expect(result.current.items).toHaveLength(0);
    expect(result.current.deliveryFee).toBe(0);
  });

  it('removes item directly via removeFromCart', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 1);
      result.current.addToCart(mockBurger, 1);
    });
    expect(result.current.items).toHaveLength(2);

    act(() => {
      result.current.removeFromCart(mockPizza.id);
    });

    expect(result.current.items).toHaveLength(1);
    expect(result.current.items[0].item.id).toBe(mockBurger.id);
  });

  it('clears all items via clearCart', () => {
    const { result } = renderHook(() => useCart(), { wrapper });

    act(() => {
      result.current.addToCart(mockPizza, 2);
    });
    expect(result.current.items).toHaveLength(1);

    act(() => {
      result.current.clearCart();
    });

    expect(result.current.items).toHaveLength(0);
    expect(result.current.subtotal).toBe(0);
  });
});
