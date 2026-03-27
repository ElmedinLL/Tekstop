import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ProductDetail } from '../types/product'

async function fetchProduct(id: number): Promise<ProductDetail> {
  const { data } = await api.get<ProductDetail>(`/products/${id}`)
  return data
}

export function useProduct(id: number | null | undefined) {
  const validId = typeof id === 'number' && Number.isFinite(id) && id > 0 ? id : null

  return useQuery({
    queryKey: ['product', validId],
    queryFn: () => fetchProduct(validId!),
    enabled: validId != null,
  })
}

