import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { AlertCircle, ShieldCheck, Lock } from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { orderService } from '../../services/orderService';
import { Input } from '../../components/UI/Input';
import { Button } from '../../components/UI/Button';
import { ApiError } from '../../services/api';

const checkoutSchema = z.object({
  customerName: z
    .string()
    .trim()
    .min(2, 'Full name must be at least 2 characters.')
    .max(100, 'Full name cannot exceed 100 characters.')
    .refine((val) => val.length < 2 || /^[a-zA-Z\s\.\'\-]+$/.test(val), {
      message: 'Name can only contain alphabetic characters, spaces, and hyphens.',
    }),
  deliveryAddress: z
    .string()
    .trim()
    .min(5, 'Delivery address must be at least 5 characters.')
    .max(250, 'Delivery address cannot exceed 250 characters.'),
  phoneNumber: z
    .string()
    .trim()
    .refine(
      (val) => /^(\+?[0-9]{1,4}[\s\-]?)?(\([0-9]{1,4}\)[\s\-]?)?[0-9\s\-]{6,16}$/.test(val),
      {
        message: 'Enter a valid phone number (e.g. +1-555-0199 or 9876543210).',
      }
    ),
});

type CheckoutFormData = z.infer<typeof checkoutSchema>;

export const CheckoutForm: React.FC = () => {
  const { items, clearCart } = useCart();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CheckoutFormData>({
    resolver: zodResolver(checkoutSchema),
    defaultValues: {
      customerName: '',
      deliveryAddress: '',
      phoneNumber: '',
    },
  });

  const onSubmit = async (data: CheckoutFormData) => {
    setServerError(null);

    if (items.length === 0) {
      setServerError('Your basket is currently empty. Please add items to checkout.');
      return;
    }

    try {
      const orderPayload = {
        customerName: data.customerName.trim(),
        deliveryAddress: data.deliveryAddress.trim(),
        phoneNumber: data.phoneNumber.trim(),
        items: items.map((ci) => ({
          menuItemId: ci.item.id,
          quantity: ci.quantity,
        })),
      };

      const createdOrder = await orderService.createOrder(orderPayload);

      // Clear local cart and redirect to live tracking page
      clearCart();
      navigate(`/order-confirmation/${encodeURIComponent(createdOrder.orderNumber)}`);
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setServerError(err.problemDetails?.detail || err.message);
      } else {
        setServerError('An unexpected error occurred while placing your order. Please try again.');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 bg-white p-6 sm:p-8 rounded-3xl border border-stone-200/80 shadow-sm" noValidate>
      <div>
        <h2 className="text-xl font-extrabold text-stone-900 tracking-tight">Delivery Details</h2>
        <p className="text-xs text-stone-500 mt-1">
          Please enter your delivery and contact information for courier dispatch.
        </p>
      </div>

      {serverError && (
        <div className="p-4 rounded-2xl bg-rose-50 border border-rose-200 flex items-start gap-3 text-rose-800" role="alert">
          <AlertCircle className="w-5 h-5 shrink-0 text-rose-600 mt-0.5" />
          <div className="text-xs">
            <strong className="block font-bold">Failed to complete order</strong>
            <span>{serverError}</span>
          </div>
        </div>
      )}

      <div className="space-y-4">
        <Input
          label="Full Name"
          placeholder="e.g. Jane Doe"
          error={errors.customerName?.message}
          {...register('customerName')}
        />

        <Input
          label="Delivery Address"
          placeholder="e.g. 123 Flavor Street, Suite 402, Gourmet City"
          error={errors.deliveryAddress?.message}
          {...register('deliveryAddress')}
        />

        <Input
          label="Contact Phone Number"
          placeholder="e.g. +1-555-0199"
          hint="We will only call regarding delivery coordination."
          error={errors.phoneNumber?.message}
          {...register('phoneNumber')}
        />
      </div>

      <div className="pt-4 border-t border-stone-100 space-y-3">
        <div className="flex items-center gap-2 text-xs text-stone-500">
          <Lock className="w-4 h-4 text-emerald-600 shrink-0" />
          <span>Server-authoritative calculation guarantees accurate pricing.</span>
        </div>

        <Button
          type="submit"
          variant="primary"
          size="lg"
          isLoading={isSubmitting}
          disabled={isSubmitting || items.length === 0}
          className="w-full shadow-lg shadow-orange-600/20"
        >
          {isSubmitting ? 'Securing Your Order...' : 'Place Order'}
        </Button>
      </div>

      <div className="flex items-center justify-center gap-2 text-[11px] text-stone-400">
        <ShieldCheck className="w-3.5 h-3.5 text-stone-400" />
        <span>Encrypted and validated with RESTful RFC 7807 standards</span>
      </div>
    </form>
  );
};
