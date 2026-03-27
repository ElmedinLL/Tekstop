import type { CreateOrderBody, OrderDto } from '../types/order'
import { api } from './api'

export async function createOrder(body: CreateOrderBody) {
  const { data } = await api.post<OrderDto>('/orders', body)
  return data
}

export async function fetchOrder(orderId: number) {
  const { data } = await api.get<OrderDto>(`/orders/${orderId}`)
  return data
}
