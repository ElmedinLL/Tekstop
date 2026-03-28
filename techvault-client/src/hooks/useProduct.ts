import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ProductDetail } from '../types/product'

export async function fetchProductDetail(id: number): Promise<ProductDetail> {
  const { data } = await api.get<ProductDetail>(`/products/${id}`)
  return data
}

export function useProduct(id: number | null | undefined) {
  const validId = typeof id === 'number' && Number.isFinite(id) && id > 0 ? id : null

  return useQuery({
    queryKey: ['product', validId],
    queryFn: () => fetchProductDetail(validId!),
    enabled: validId != null,
  })
}

