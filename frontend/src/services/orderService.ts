import { CreateOrderRequest, OrderDto, OrderStatus, PagedResult, UpdateOrderStatusRequest } from '../types';
import { apiClient } from './api';

export const orderService = {
  async createOrder(request: CreateOrderRequest): Promise<OrderDto> {
    return apiClient<OrderDto>('/api/v1/orders', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  },

  async getOrderById(id: string): Promise<OrderDto> {
    return apiClient<OrderDto>(`/api/v1/orders/${id}`);
  },

  async getOrderByNumber(orderNumber: string): Promise<OrderDto> {
    return apiClient<OrderDto>(`/api/v1/orders/by-number/${encodeURIComponent(orderNumber.trim())}`);
  },

  async getOrders(page: number = 1, pageSize: number = 10, status?: OrderStatus): Promise<PagedResult<OrderDto>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (status) {
      params.append('status', status);
    }

    return apiClient<PagedResult<OrderDto>>(`/api/v1/orders?${params.toString()}`);
  },

  async updateOrderStatus(id: string, status: OrderStatus): Promise<OrderDto> {
    const payload: UpdateOrderStatusRequest = { status };
    return apiClient<OrderDto>(`/api/v1/orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    });
  },

  async cancelOrder(id: string): Promise<void> {
    return apiClient<void>(`/api/v1/orders/${id}`, {
      method: 'DELETE',
    });
  },
};
