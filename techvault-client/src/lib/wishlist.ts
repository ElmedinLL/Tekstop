import type { WishlistItem } from '../types/wishlist'
import { api } from './api'

export async function fetchWishlist() {
  const { data } = await api.get<WishlistItem[]>('/wishlist')
  return data
}

export async function addToWishlist(productId: number) {
  const { data } = await api.post<WishlistItem>(`/wishlist/${productId}`)
  return data
}

export async function removeFromWishlist(productId: number) {
  await api.delete(`/wishlist/${productId}`)
}
