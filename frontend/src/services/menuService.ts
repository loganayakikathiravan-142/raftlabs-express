import { MenuItem } from '../types';
import { apiClient } from './api';

export const menuService = {
  async getMenuItems(category?: string, search?: string): Promise<MenuItem[]> {
    const params = new URLSearchParams();
    if (category && category !== 'All') {
      params.append('category', category);
    }
    if (search && search.trim()) {
      params.append('search', search.trim());
    }

    const queryString = params.toString() ? `?${params.toString()}` : '';
    return apiClient<MenuItem[]>(`/api/v1/menu${queryString}`);
  },

  async getMenuItemById(id: string): Promise<MenuItem> {
    return apiClient<MenuItem>(`/api/v1/menu/${id}`);
  },
};
