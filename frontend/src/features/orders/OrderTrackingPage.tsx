import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Clock,
  MapPin,
  Phone,
  User,
  Radio,
  RefreshCw,
  ArrowLeft,
  AlertTriangle,
  Receipt,
} from 'lucide-react';
import { orderService } from '../../services/orderService';
import { signalrService } from '../../services/signalrService';
import { OrderDto } from '../../types';
import { StatusTimeline } from '../../components/UI/StatusTimeline';
import { Badge } from '../../components/UI/Badge';
import { Button } from '../../components/UI/Button';

export const OrderTrackingPage: React.FC = () => {
  const { orderNumber } = useParams<{ orderNumber: string }>();
  const [liveOrder, setLiveOrder] = useState<OrderDto | null>(null);
  const [signalrConnected, setSignalrConnected] = useState(false);

  const cleanOrderNumber = orderNumber?.trim() || '';

  // Polling fallback with TanStack Query
  const {
    data: fetchedOrder,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['order', cleanOrderNumber],
    queryFn: () => orderService.getOrderByNumber(cleanOrderNumber),
    enabled: Boolean(cleanOrderNumber),
    refetchInterval: (query) => {
      const data = query.state.data;
      if (!data) return 4000;
      // Stop polling when order reached terminal state
      if (data.status === 'Delivered' || data.status === 'Cancelled') {
        return false;
      }
      return 4000;
    },
    staleTime: 2000,
  });

  // Keep liveOrder synced with fetched order initially
  useEffect(() => {
    if (fetchedOrder) {
      setLiveOrder(fetchedOrder);
    }
  }, [fetchedOrder]);

  // Connect SignalR for live push
  useEffect(() => {
    if (!cleanOrderNumber) return;

    let cleanupFn: (() => void) | undefined;

    const connectHub = async () => {
      cleanupFn = await signalrService.joinOrderTracking(cleanOrderNumber, (updated) => {
        setLiveOrder(updated);
        setSignalrConnected(true);
      });
      setSignalrConnected(true);
    };

    connectHub();

    return () => {
      if (cleanupFn) cleanupFn();
    };
  }, [cleanOrderNumber]);

  const activeOrder = liveOrder || fetchedOrder;

  if (!cleanOrderNumber) {
    return (
      <div className="max-w-md mx-auto my-12 text-center bg-white p-8 rounded-3xl border border-stone-200">
        <h2 className="text-xl font-bold text-stone-900">No order number provided</h2>
        <p className="text-xs text-stone-500 mt-2">
          Please check your order confirmation link or search for an active order.
        </p>
        <Link to="/track" className="mt-4 inline-block">
          <Button variant="primary" size="sm">Search Order</Button>
        </Link>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="max-w-3xl mx-auto my-12 bg-white p-8 rounded-3xl border border-stone-100 shadow-sm space-y-6 animate-pulse">
        <div className="h-8 bg-stone-200 rounded-lg w-1/3" />
        <div className="h-4 bg-stone-100 rounded w-1/2" />
        <div className="h-36 bg-stone-100 rounded-2xl" />
        <div className="h-48 bg-stone-50 rounded-2xl" />
      </div>
    );
  }

  if (isError || !activeOrder) {
    return (
      <div className="max-w-md mx-auto my-12 text-center bg-white p-8 rounded-3xl border border-rose-200 shadow-sm space-y-4">
        <div className="w-14 h-14 rounded-full bg-rose-100 text-rose-600 flex items-center justify-center mx-auto">
          <AlertTriangle className="w-7 h-7" />
        </div>
        <h2 className="text-lg font-bold text-rose-900">Order Not Found</h2>
        <p className="text-xs text-rose-700">
          {(error as Error)?.message || `We couldn't locate an order matching "${cleanOrderNumber}".`}
        </p>
        <div className="pt-2 flex justify-center gap-2">
          <Button onClick={() => refetch()} variant="outline" size="sm">
            <RefreshCw className="w-4 h-4 mr-1.5" />
            <span>Retry</span>
          </Button>
          <Link to="/">
            <Button variant="primary" size="sm">Back to Menu</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-8">
      {/* Header Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-stone-200">
        <div>
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 text-xs font-bold text-stone-500 hover:text-stone-900 mb-2 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Return to Menu</span>
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl sm:text-3xl font-black text-stone-900 tracking-tight">
              Order {activeOrder.orderNumber}
            </h1>
            <Badge
              variant={
                activeOrder.status === 'Delivered'
                  ? 'success'
                  : activeOrder.status === 'Cancelled'
                  ? 'danger'
                  : 'primary'
              }
            >
              {activeOrder.statusText}
            </Badge>
          </div>
        </div>

        {/* Live sync pill */}
        <div className="flex items-center gap-2 bg-stone-100 px-3.5 py-1.5 rounded-2xl border border-stone-200/80 text-xs text-stone-600 w-fit">
          <Radio
            className={`w-4 h-4 ${
              signalrConnected ? 'text-emerald-500 animate-pulse' : 'text-amber-500'
            }`}
          />
          <span className="font-semibold">
            {signalrConnected ? 'Real-Time Sync Active' : 'Polling for Updates'}
          </span>
        </div>
      </div>

      {/* Visual Status Progression Timeline */}
      <div className="bg-white p-6 sm:p-8 rounded-3xl border border-stone-200/80 shadow-sm space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-base font-extrabold text-stone-900">Live Status Progression</h2>
          <span className="text-xs text-stone-400">
            Placed {new Date(activeOrder.createdAtUtc).toLocaleTimeString()}
          </span>
        </div>
        <StatusTimeline status={activeOrder.status} updatedAt={activeOrder.updatedAtUtc} />
      </div>

      {/* Customer and Order Summary Details */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Customer Information */}
        <div className="bg-white p-6 rounded-3xl border border-stone-200/80 shadow-sm space-y-4">
          <h3 className="text-sm font-extrabold text-stone-900 uppercase tracking-wider">
            Customer & Delivery
          </h3>

          <div className="space-y-3 text-xs text-stone-600">
            <div className="flex items-start gap-2.5">
              <User className="w-4 h-4 text-stone-400 shrink-0 mt-0.5" />
              <div>
                <span className="font-bold text-stone-800">Customer Name</span>
                <p className="text-stone-600">{activeOrder.customerName}</p>
              </div>
            </div>

            <div className="flex items-start gap-2.5">
              <MapPin className="w-4 h-4 text-stone-400 shrink-0 mt-0.5" />
              <div>
                <span className="font-bold text-stone-800">Destination Address</span>
                <p className="text-stone-600">{activeOrder.deliveryAddress}</p>
              </div>
            </div>

            <div className="flex items-start gap-2.5">
              <Phone className="w-4 h-4 text-stone-400 shrink-0 mt-0.5" />
              <div>
                <span className="font-bold text-stone-800">Phone Contact</span>
                <p className="text-stone-600">{activeOrder.phoneNumber}</p>
              </div>
            </div>

            <div className="flex items-start gap-2.5">
              <Clock className="w-4 h-4 text-stone-400 shrink-0 mt-0.5" />
              <div>
                <span className="font-bold text-stone-800">Order Placed</span>
                <p className="text-stone-600">{new Date(activeOrder.createdAtUtc).toLocaleString()}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Ordered Item Snapshots & Financial Totals */}
        <div className="bg-white p-6 rounded-3xl border border-stone-200/80 shadow-sm space-y-4">
          <div className="flex items-center gap-2">
            <Receipt className="w-4 h-4 text-stone-600" />
            <h3 className="text-sm font-extrabold text-stone-900 uppercase tracking-wider">
              Itemized Receipt
            </h3>
          </div>

          <div className="divide-y divide-stone-100 max-h-56 overflow-y-auto pr-1">
            {activeOrder.items.map((item) => (
              <div key={item.id} className="py-2.5 flex justify-between items-center text-xs">
                <div>
                  <span className="font-bold text-stone-900">{item.itemNameSnapshot}</span>
                  <span className="text-stone-400 ml-2">
                    Qty: {item.quantity} × ${item.unitPriceSnapshot.toFixed(2)}
                  </span>
                </div>
                <span className="font-extrabold text-stone-800">${item.lineTotal.toFixed(2)}</span>
              </div>
            ))}
          </div>

          <div className="pt-3 border-t border-stone-200 space-y-1.5 text-xs">
            <div className="flex justify-between text-stone-600">
              <span>Subtotal</span>
              <span className="font-semibold text-stone-900">${activeOrder.subtotal.toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-stone-600">
              <span>Delivery Fee</span>
              <span className="font-semibold text-stone-900">${activeOrder.deliveryFee.toFixed(2)}</span>
            </div>
            <div className="pt-2 border-t border-stone-100 flex justify-between text-sm font-black text-stone-900">
              <span>Grand Total</span>
              <span className="text-orange-600 text-base">${activeOrder.totalAmount.toFixed(2)}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
