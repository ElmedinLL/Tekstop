import type { MyReviewStatus, ProductReview } from '../types/review'
import { api } from './api'

export async function fetchProductReviews(productId: number) {
  const { data } = await api.get<ProductReview[]>(`/products/${productId}/reviews`)
  return data
}

export async function fetchMyReviewStatus(productId: number) {
  const { data } = await api.get<MyReviewStatus>(`/products/${productId}/reviews/me`)
  return data
}

export async function createProductReview(productId: number, payload: { rating: number; comment?: string | null }) {
  const { data } = await api.post<ProductReview>(`/products/${productId}/reviews`, payload)
  return data
}
