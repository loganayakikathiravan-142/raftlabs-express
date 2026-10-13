import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MenuItemCard } from '../features/menu/MenuItemCard';
import { CartProvider } from '../context/CartContext';
import { MenuItem } from '../types';

const mockItem: MenuItem = {
  id: 'pizza-1',
  name: 'Artisanal Margherita Pizza',
  description: 'San Marzano tomatoes, fresh buffalo mozzarella, aromatic basil.',
  price: 14.99,
  category: 'Pizza',
  imageUrl: 'https://example.com/margherita.jpg',
  isAvailable: true,
  createdAtUtc: new Date().toISOString(),
};

const mockUnavailableItem: MenuItem = {
  id: 'lobster-1',
  name: 'Seasonal Lobster Thermidor',
  description: 'Decadent pasta with fresh lobster.',
  price: 24.99,
  category: 'Pasta',
  imageUrl: 'https://example.com/lobster.jpg',
  isAvailable: false,
  createdAtUtc: new Date().toISOString(),
};

describe('MenuItemCard', () => {
  it('renders item title, price, description, and in-stock badge', () => {
    render(
      <CartProvider>
        <MenuItemCard item={mockItem} />
      </CartProvider>
    );

    expect(screen.getByText('Artisanal Margherita Pizza')).toBeInTheDocument();
    expect(screen.getByText('$14.99')).toBeInTheDocument();
    expect(screen.getByText(/San Marzano tomatoes/i)).toBeInTheDocument();
    expect(screen.getByText('In Stock')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add .* to cart/i })).toBeEnabled();
  });

  it('renders sold out badge and disables add to cart button when item is unavailable', () => {
    render(
      <CartProvider>
        <MenuItemCard item={mockUnavailableItem} />
      </CartProvider>
    );

    expect(screen.getByText('Seasonal Lobster Thermidor')).toBeInTheDocument();
    expect(screen.getByText('Sold Out')).toBeInTheDocument();
    const button = screen.getByRole('button', { name: /currently out of stock/i });
    expect(button).toBeDisabled();
    expect(button).toHaveTextContent('Unavailable');
  });

  it('provides visual feedback when Add to Cart is clicked', () => {
    render(
      <CartProvider>
        <MenuItemCard item={mockItem} />
      </CartProvider>
    );

    const button = screen.getByRole('button', { name: /add .* to cart/i });
    fireEvent.click(button);

    expect(screen.getByText('Added!')).toBeInTheDocument();
  });
});
