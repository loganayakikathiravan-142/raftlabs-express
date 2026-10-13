export type OrderStatus =
  | 'OrderReceived'
  | 'Preparing'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled';

export interface MenuItem {
  id: string;
  name: string;
  description: string;
  imageUrl: string;
  price: number;
  category: string;
  isAvailable: boolean;
  createdAtUtc: string;
}

export interface CartItem {
  item: MenuItem;
  quantity: number;
}

export interface OrderItemDto {
  id: string;
  menuItemId: string;
  itemNameSnapshot: string;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  customerName: string;
  deliveryAddress: string;
  phoneNumber: string;
  status: OrderStatus;
  statusText: string;
  subtotal: number;
  deliveryFee: number;
  totalAmount: number;
  createdAtUtc: string;
  updatedAtUtc?: string;
  items: OrderItemDto[];
}

export interface CreateOrderItemRequest {
  menuItemId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerName: string;
  deliveryAddress: string;
  phoneNumber: string;
  items: CreateOrderItemRequest[];
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
