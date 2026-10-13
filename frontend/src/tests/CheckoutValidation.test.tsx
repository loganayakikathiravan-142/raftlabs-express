import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { CheckoutForm } from '../features/checkout/CheckoutForm';
import { CartProvider, useCart } from '../context/CartContext';
import { MenuItem } from '../types';

const mockItem: MenuItem = {
  id: 'item-1',
  name: 'Margherita Pizza',
  description: 'Classic pizza',
  price: 15.0,
  category: 'Pizza',
  imageUrl: 'https://example.com/pizza.jpg',
  isAvailable: true,
  createdAtUtc: new Date().toISOString(),
};

const TestWrapperWithItem: React.FC = () => {
  const { addToCart } = useCart();

  React.useEffect(() => {
    addToCart(mockItem, 1);
  }, []);

  return <CheckoutForm />;
};

describe('CheckoutForm Validation', () => {
  it('displays field validation errors when submitting empty inputs', async () => {
    render(
      <BrowserRouter>
        <CartProvider>
          <TestWrapperWithItem />
        </CartProvider>
      </BrowserRouter>
    );

    const submitButton = screen.getByRole('button', { name: /place order/i });
    expect(submitButton).toBeEnabled();
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/full name must be at least 2 characters/i)).toBeInTheDocument();
      expect(screen.getByText(/delivery address must be at least 5 characters/i)).toBeInTheDocument();
      expect(screen.getByText(/enter a valid phone number/i)).toBeInTheDocument();
    });
  });

  it('rejects invalid phone numbers', async () => {
    render(
      <BrowserRouter>
        <CartProvider>
          <TestWrapperWithItem />
        </CartProvider>
      </BrowserRouter>
    );

    const nameInput = screen.getByLabelText(/full name/i);
    const addressInput = screen.getByLabelText(/delivery address/i);
    const phoneInput = screen.getByLabelText(/contact phone number/i);

    fireEvent.change(nameInput, { target: { value: 'Jane Doe' } });
    fireEvent.change(addressInput, { target: { value: '123 Test Street' } });
    fireEvent.change(phoneInput, { target: { value: 'invalid-phone-xyz' } });

    const submitButton = screen.getByRole('button', { name: /place order/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/enter a valid phone number/i)).toBeInTheDocument();
    });
  });
});
