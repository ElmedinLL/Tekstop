import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ProductDetail } from '../types/product'

/** Thrown when GET /products/{id} returns 404 (missing, draft, or soft-deleted). */
export class ProductNotFoundError extends Error {
  readonly productId: number

  constructor(productId: number) {
    super(`Product not found: ${productId}`)
    this.name = 'ProductNotFoundError'
    this.productId = productId
  }
}

export function isProductNotFoundError(e: unknown): e is ProductNotFoundError {
  return e instanceof ProductNotFoundError
}

/** Avoid React Query retrying forever on missing products. */
export function productDetailQueryRetry(failureCount: number, error: unknown): boolean {
  if (isProductNotFoundError(error)) {
    return false
  }
  return failureCount < 3
}

/**
 * Uses validateStatus so 404 does not reject at the axios layer (quieter browser / IDE tooling).
 * Missing products still surface as ProductNotFoundError for React Query.
 */
export async function fetchProductDetail(id: number): Promise<ProductDetail> {
  const res = await api.get<ProductDetail>(`/products/${id}`, {
    validateStatus: (status) => status === 200 || status === 404,
  })
  if (res.status === 404) {
    throw new ProductNotFoundError(id)
  }
  return res.data
}

export function useProduct(id: number | null | undefined) {
  const validId = typeof id === 'number' && Number.isFinite(id) && id > 0 ? id : null

  return useQuery({
    queryKey: ['product', validId],
    queryFn: () => fetchProductDetail(validId!),
    enabled: validId != null,
    retry: productDetailQueryRetry,
  })
}

