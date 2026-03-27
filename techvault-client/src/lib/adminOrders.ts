import type { AdminOrderDetail, AdminOrderListResult } from '../types/adminOrder'
import { api } from './api'

export type AdminOrderListParams = {
  page?: number
  status?: string
  from?: string
  to?: string
}

export async function fetchAdminOrders(params: AdminOrderListParams = {}) {
  const { data } = await api.get<AdminOrderListResult>('/admin/orders', {
    params: {
      page: params.page ?? 1,
      status: params.status && params.status !== 'all' ? params.status : undefined,
      from: params.from || undefined,
      to: params.to || undefined,
    },
  })
  return data
}

export async function fetchAdminOrder(orderId: number) {
  const { data } = await api.get<AdminOrderDetail>(`/admin/orders/${orderId}`)
  return data
}

export async function shipAdminOrder(orderId: number, trackingUrl: string) {
  await api.post(`/admin/orders/${orderId}/ship`, { trackingUrl })
}

export async function updateAdminOrderStatus(
  orderId: number,
  body: { status: string; trackingUrl?: string | null },
) {
  await api.put(`/admin/orders/${orderId}/status`, body)
}
