import { api } from './api'
import type { CartDto, GuestCartLine } from '../types/cart'

export async function getCart(): Promise<CartDto> {
  const { data } = await api.get<CartDto>('/cart')
  return data
}

export async function addCartItem(productId: number, quantity: number): Promise<CartDto> {
  const { data } = await api.post<CartDto>('/cart/items', { productId, quantity })
  return data
}

export async function updateCartLineQuantity(
  productId: number,
  cartItemId: number,
  quantity: number,
): Promise<CartDto> {
  const routeId = cartItemId > 0 ? cartItemId : productId
  const { data } = await api.put<CartDto>(`/cart/items/${routeId}`, { quantity })
  return data
}

export async function removeCartLine(productId: number, cartItemId: number): Promise<CartDto> {
  const routeId = cartItemId > 0 ? cartItemId : productId
  const { data } = await api.delete<CartDto>(`/cart/items/${routeId}`)
  return data
}

export async function clearCart(): Promise<CartDto> {
  const { data } = await api.delete<CartDto>('/cart')
  return data
}

export async function mergeCart(lines: GuestCartLine[]): Promise<CartDto> {
  const { data } = await api.post<CartDto>('/cart/merge', { lines })
  return data
}
