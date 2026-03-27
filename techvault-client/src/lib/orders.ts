import type { CreateOrderBody, OrderDto } from '../types/order'
import { api } from './api'

export type OrderSummary = {
  id: number
  orderNumber: string
  status: string
  total: number
  placedAtUtc: string
  lineItemCount: number
}

export type OrderListResponse = {
  items: OrderSummary[]
  page: number
  pageSize: number
  totalCount: number
}

export async function fetchOrderList(page = 1) {
  const { data } = await api.get<OrderListResponse>('/orders', { params: { page } })
  return data
}

export async function createOrder(body: CreateOrderBody) {
  const { data } = await api.post<OrderDto>('/orders', body)
  return data
}

export async function fetchOrder(orderId: number) {
  const { data } = await api.get<OrderDto>(`/orders/${orderId}`)
  return data
}

export async function cancelOrder(orderId: number) {
  await api.post(`/orders/${orderId}/cancel`)
}
