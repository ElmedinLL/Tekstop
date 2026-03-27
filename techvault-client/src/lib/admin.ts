import type { AdminOrderListResult, AdminStats, LowStockProduct } from '../types/admin'
import { api } from './api'

export async function fetchAdminStats() {
  const { data } = await api.get<AdminStats>('/admin/stats')
  return data
}

export async function fetchAdminOrders(page = 1, pageSize = 8) {
  const { data } = await api.get<AdminOrderListResult>('/admin/orders', {
    params: { page, pageSize },
  })
  return data
}

export async function fetchAdminLowStockProducts() {
  const { data } = await api.get<LowStockProduct[]>('/admin/products/low-stock')
  return data
}
