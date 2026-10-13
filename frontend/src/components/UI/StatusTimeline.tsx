import React from 'react';
import { ChefHat, Bike, CheckCircle2, ClipboardCheck, XCircle } from 'lucide-react';
import { OrderStatus } from '../../types';

interface StatusTimelineProps {
  status: OrderStatus;
  updatedAt?: string;
}

interface Step {
  id: OrderStatus;
  label: string;
  description: string;
  icon: React.ElementType;
}

const steps: Step[] = [
  {
    id: 'OrderReceived',
    label: 'Order Received',
    description: 'We have received your order and sent it to the kitchen.',
    icon: ClipboardCheck,
  },
  {
    id: 'Preparing',
    label: 'Preparing',
    description: 'Our chefs are crafting your food fresh with premium ingredients.',
    icon: ChefHat,
  },
  {
    id: 'OutForDelivery',
    label: 'Out for Delivery',
    description: 'Your courier is en route with your insulated meal pack.',
    icon: Bike,
  },
  {
    id: 'Delivered',
    label: 'Delivered',
    description: 'Your food has arrived! Enjoy your culinary experience.',
    icon: CheckCircle2,
  },
];

const statusOrder: Record<OrderStatus, number> = {
  OrderReceived: 0,
  Preparing: 1,
  OutForDelivery: 2,
  Delivered: 3,
  Cancelled: -1,
};

export const StatusTimeline: React.FC<StatusTimelineProps> = ({ status }) => {
  if (status === 'Cancelled') {
    return (
      <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-center">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-rose-100 text-rose-600 mb-3">
          <XCircle className="h-8 w-8" />
        </div>
        <h4 className="text-lg font-bold text-rose-900">Order Cancelled</h4>
        <p className="mt-1 text-sm text-rose-700">
          This order has been cancelled and is no longer in progress.
        </p>
      </div>
    );
  }

  const currentStepIndex = statusOrder[status];

  return (
    <div className="w-full py-4">
      <div className="relative">
        {/* Horizontal bar on md+ */}
        <div className="hidden md:block absolute top-6 left-12 right-12 h-1 bg-stone-200 -z-0">
          <div
            className="h-full bg-orange-600 transition-all duration-700 ease-in-out"
            style={{
              width: `${(Math.max(0, currentStepIndex) / (steps.length - 1)) * 100}%`,
            }}
          />
        </div>

        {/* Steps */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 relative z-10">
          {steps.map((step, idx) => {
            const isCompleted = currentStepIndex > idx;
            const isCurrent = currentStepIndex === idx;
            const StepIcon = step.icon;

            return (
              <div
                key={step.id}
                className="flex md:flex-col items-start md:items-center text-left md:text-center gap-4 md:gap-2"
              >
                {/* Step Circle */}
                <div
                  className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl transition-all duration-300 shadow-sm ${
                    isCompleted
                      ? 'bg-emerald-600 text-white ring-4 ring-emerald-100'
                      : isCurrent
                      ? 'bg-orange-600 text-white ring-4 ring-orange-200 animate-pulse'
                      : 'bg-stone-100 text-stone-400 border border-stone-200'
                  }`}
                >
                  <StepIcon className="h-6 w-6" />
                </div>

                {/* Text */}
                <div className="flex-1">
                  <div className="flex items-center gap-2 md:justify-center">
                    <h5
                      className={`text-sm font-bold ${
                        isCurrent
                          ? 'text-orange-600'
                          : isCompleted
                          ? 'text-emerald-800'
                          : 'text-stone-400'
                      }`}
                    >
                      {step.label}
                    </h5>
                    {isCurrent && (
                      <span className="inline-block h-2 w-2 rounded-full bg-orange-500 animate-ping" />
                    )}
                  </div>
                  <p className="mt-1 text-xs text-stone-500 leading-relaxed max-w-[200px] md:mx-auto">
                    {step.description}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
