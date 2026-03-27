import type { PagedProductsResponse } from '../types/product'
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

export type AdminProductsQuery = {
  page: number
  pageSize: number
  search?: string
  sort?: string
}

export async function fetchAdminProducts(params: AdminProductsQuery) {
  const { data } = await api.get<PagedProductsResponse>('/admin/products', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      ...(params.search?.trim() ? { search: params.search.trim() } : {}),
      ...(params.sort && params.sort !== 'newest' ? { sort: params.sort } : {}),
    },
  })
  return data
}
