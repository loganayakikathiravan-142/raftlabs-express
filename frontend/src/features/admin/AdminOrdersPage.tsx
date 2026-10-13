import React, { useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import {
  ChefHat,
  Bike,
  CheckCircle2,
  XCircle,
  RefreshCw,
  Radio,
  ExternalLink,
  AlertCircle,
} from 'lucide-react';
import { orderService } from '../../services/orderService';
import { signalrService } from '../../services/signalrService';
import { OrderStatus } from '../../types';
import { Badge } from '../../components/UI/Badge';
import { Button } from '../../components/UI/Button';

export const AdminOrdersPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [selectedStatus, setSelectedStatus] = useState<string>('All');
  const [actionError, setActionError] = useState<string | null>(null);

  const statusFilter = selectedStatus === 'All' ? undefined : (selectedStatus as OrderStatus);

  const {
    data: pagedOrders,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['admin-orders', selectedStatus],
    queryFn: () => orderService.getOrders(1, 50, statusFilter),
    refetchInterval: 5000,
  });

  // Subscribe to real-time order status updates and creations
  useEffect(() => {
    let cleanupFn: (() => void) | undefined;

    const connectHub = async () => {
      cleanupFn = await signalrService.subscribeAdmin(
        () => {
          queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
        },
        () => {
          queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
        }
      );
    };

    connectHub();

    return () => {
      if (cleanupFn) cleanupFn();
    };
  }, [queryClient]);

  // Mutation to transition status
  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) =>
      orderService.updateOrderStatus(id, status),
    onSuccess: () => {
      setActionError(null);
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
    },
    onError: (err: Error) => {
      setActionError(err.message || 'Failed to update order status');
    },
  });

  const orders = pagedOrders?.items || [];

  return (
    <div className="space-y-8 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-stone-200">
        <div>
          <div className="inline-flex items-center gap-2 px-3 py-1 bg-stone-900 text-white rounded-full text-xs font-bold mb-2">
            <ChefHat className="w-3.5 h-3.5 text-orange-400" />
            <span>Kitchen & Dispatch Management</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-black text-stone-900 tracking-tight">
            Live Order Operations
          </h1>
          <p className="text-xs text-stone-500 mt-1">
            Authorize state transitions, monitor incoming orders, and inspect lifecycle audit events.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 bg-emerald-50 text-emerald-800 border border-emerald-200 px-3 py-1.5 rounded-2xl text-xs font-bold">
            <Radio className="w-3.5 h-3.5 text-emerald-600 animate-pulse" />
            <span>Live Dispatch Active</span>
          </div>
          <Button onClick={() => refetch()} variant="outline" size="sm" className="gap-1.5">
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Refresh</span>
          </Button>
        </div>
      </div>

      {actionError && (
        <div className="p-4 rounded-2xl bg-rose-50 border border-rose-200 flex items-center justify-between text-rose-800 text-xs font-semibold">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 text-rose-600 shrink-0" />
            <span>{actionError}</span>
          </div>
          <button onClick={() => setActionError(null)} className="underline hover:text-rose-950">
            Dismiss
          </button>
        </div>
      )}

      {/* Filter Tabs */}
      <div className="flex items-center gap-2 overflow-x-auto pb-2">
        {['All', 'OrderReceived', 'Preparing', 'OutForDelivery', 'Delivered', 'Cancelled'].map(
          (status) => (
            <button
              key={status}
              onClick={() => setSelectedStatus(status)}
              className={`px-3.5 py-2 rounded-xl text-xs font-bold whitespace-nowrap transition-all ${
                selectedStatus === status
                  ? 'bg-stone-900 text-white shadow-sm'
                  : 'bg-white text-stone-600 hover:bg-stone-100 border border-stone-200'
              }`}
            >
              {status === 'All' ? 'All Orders' : status.replace(/([A-Z])/g, ' $1').trim()}
            </button>
          )
        )}
      </div>

      {/* Orders Table */}
      {isLoading ? (
        <div className="bg-white rounded-3xl border border-stone-200 p-8 space-y-4 animate-pulse">
          <div className="h-6 bg-stone-100 rounded w-1/4" />
          <div className="h-12 bg-stone-50 rounded" />
          <div className="h-12 bg-stone-50 rounded" />
          <div className="h-12 bg-stone-50 rounded" />
        </div>
      ) : isError ? (
        <div className="bg-white rounded-3xl border border-rose-200 p-8 text-center text-rose-800">
          <p className="text-sm font-bold">Failed to load orders for management dashboard.</p>
          <p className="text-xs text-rose-600 mt-1">{(error as Error)?.message}</p>
        </div>
      ) : orders.length === 0 ? (
        <div className="bg-white rounded-3xl border border-stone-200 p-12 text-center text-stone-500 space-y-2">
          <p className="text-sm font-bold text-stone-800">No orders found in this category.</p>
          <p className="text-xs">Placed customer orders will appear here automatically.</p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-stone-200/80 shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-stone-50 border-b border-stone-200 text-stone-600 uppercase tracking-wider font-extrabold">
                  <th className="py-3.5 px-4">Order #</th>
                  <th className="py-3.5 px-4">Customer</th>
                  <th className="py-3.5 px-4">Address</th>
                  <th className="py-3.5 px-4">Items</th>
                  <th className="py-3.5 px-4">Total</th>
                  <th className="py-3.5 px-4">Current Status</th>
                  <th className="py-3.5 px-4 text-right">Lifecycle Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-100">
                {orders.map((o) => (
                  <tr key={o.id} className="hover:bg-stone-50/80 transition-colors">
                    <td className="py-4 px-4 font-bold text-stone-900 whitespace-nowrap">
                      <Link
                        to={`/track/${encodeURIComponent(o.orderNumber)}`}
                        className="inline-flex items-center gap-1.5 text-orange-600 hover:text-orange-700 hover:underline"
                        target="_blank"
                        title="View Live Tracker"
                      >
                        <span>{o.orderNumber}</span>
                        <ExternalLink className="w-3 h-3" />
                      </Link>
                      <span className="block text-[10px] text-stone-400 font-normal">
                        {new Date(o.createdAtUtc).toLocaleTimeString()}
                      </span>
                    </td>
                    <td className="py-4 px-4 font-semibold text-stone-800">
                      <div>{o.customerName}</div>
                      <div className="text-[10px] text-stone-400 font-normal">{o.phoneNumber}</div>
                    </td>
                    <td className="py-4 px-4 text-stone-600 max-w-xs truncate" title={o.deliveryAddress}>
                      {o.deliveryAddress}
                    </td>
                    <td className="py-4 px-4 text-stone-700">
                      <span className="font-bold">{o.items.reduce((acc, i) => acc + i.quantity, 0)}</span> items
                      <div className="text-[10px] text-stone-400 truncate max-w-[140px]">
                        {o.items.map((i) => `${i.quantity}x ${i.itemNameSnapshot}`).join(', ')}
                      </div>
                    </td>
                    <td className="py-4 px-4 font-extrabold text-stone-900 whitespace-nowrap">
                      ${o.totalAmount.toFixed(2)}
                    </td>
                    <td className="py-4 px-4">
                      <Badge
                        variant={
                          o.status === 'Delivered'
                            ? 'success'
                            : o.status === 'Cancelled'
                            ? 'danger'
                            : 'primary'
                        }
                      >
                        {o.statusText}
                      </Badge>
                    </td>
                    <td className="py-4 px-4 text-right whitespace-nowrap">
                      <div className="flex items-center justify-end gap-1.5">
                        {o.status === 'OrderReceived' && (
                          <>
                            <button
                              onClick={() =>
                                updateStatusMutation.mutate({ id: o.id, status: 'Preparing' })
                              }
                              disabled={updateStatusMutation.isPending}
                              className="px-2.5 py-1.5 bg-orange-600 hover:bg-orange-700 text-white rounded-lg font-bold text-[11px] shadow-sm flex items-center gap-1 transition-all"
                            >
                              <ChefHat className="w-3 h-3" />
                              <span>Start Prep</span>
                            </button>
                            <button
                              onClick={() =>
                                updateStatusMutation.mutate({ id: o.id, status: 'Cancelled' })
                              }
                              disabled={updateStatusMutation.isPending}
                              className="px-2 py-1.5 text-stone-500 hover:text-rose-600 hover:bg-rose-50 rounded-lg text-[11px] font-semibold"
                            >
                              <XCircle className="w-3 h-3" />
                            </button>
                          </>
                        )}

                        {o.status === 'Preparing' && (
                          <>
                            <button
                              onClick={() =>
                                updateStatusMutation.mutate({ id: o.id, status: 'OutForDelivery' })
                              }
                              disabled={updateStatusMutation.isPending}
                              className="px-2.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-bold text-[11px] shadow-sm flex items-center gap-1 transition-all"
                            >
                              <Bike className="w-3 h-3" />
                              <span>Dispatch</span>
                            </button>
                            <button
                              onClick={() =>
                                updateStatusMutation.mutate({ id: o.id, status: 'Cancelled' })
                              }
                              disabled={updateStatusMutation.isPending}
                              className="px-2 py-1.5 text-stone-500 hover:text-rose-600 hover:bg-rose-50 rounded-lg text-[11px] font-semibold"
                            >
                              <XCircle className="w-3 h-3" />
                            </button>
                          </>
                        )}

                        {o.status === 'OutForDelivery' && (
                          <button
                            onClick={() =>
                              updateStatusMutation.mutate({ id: o.id, status: 'Delivered' })
                            }
                            disabled={updateStatusMutation.isPending}
                            className="px-2.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-bold text-[11px] shadow-sm flex items-center gap-1 transition-all"
                          >
                            <CheckCircle2 className="w-3 h-3" />
                            <span>Mark Delivered</span>
                          </button>
                        )}

                        {(o.status === 'Delivered' || o.status === 'Cancelled') && (
                          <span className="text-[11px] font-medium text-stone-400 italic">
                            Completed
                          </span>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
