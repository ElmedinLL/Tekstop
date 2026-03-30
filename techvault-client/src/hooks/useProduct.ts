import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ProductDetail } from '../types/product'

/**
 * Uses validateStatus so 404 does not reject at the axios layer (quieter browser / IDE tooling).
 * Missing products resolve to null so the query succeeds and does not throw to the error boundary.
 */
export async function fetchProductDetail(id: number): Promise<ProductDetail | null> {
  const res = await api.get<ProductDetail>(`/products/${id}`, {
    validateStatus: (status) => status === 200 || status === 404,
  })
  if (res.status === 404) {
    return null
  }
  return res.data
}

export function useProduct(id: number | null | undefined) {
  const validId = typeof id === 'number' && Number.isFinite(id) && id > 0 ? id : null

  return useQuery({
    queryKey: ['product', validId],
    queryFn: () => fetchProductDetail(validId!),
    enabled: validId != null,
  })
}

