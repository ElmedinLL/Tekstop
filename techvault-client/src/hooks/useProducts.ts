import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { PagedProductsResponse, ProductListItem } from '../types/product'

export type UseProductsParams = {
  initialPage?: number
  initialPageSize?: number
  sort?: string
  /**
   * Backend accepts `category` as numeric id or category slug via `ProductListQueryParameters.Category`.
   * Pass either a number-like string (e.g. "1") or a slug (e.g. "laptops").
   */
  category?: string | null
  search?: string
  minPrice?: number | null
  maxPrice?: number | null
  /**
   * Backend expects repeated `specs` query values (e.g. specs=RAM:16GB&specs=Storage:1TB).
   */
  specs?: string[] | null
}

const clampInt = (n: unknown, fallback: number, min: number, max: number) => {
  if (typeof n !== 'number' || !Number.isFinite(n)) return fallback
  return Math.min(max, Math.max(min, Math.trunc(n)))
}

async function fetchProducts(params: {
  page: number
  pageSize: number
  sort?: string
  category?: string | null
  search?: string
  minPrice?: number | null
  maxPrice?: number | null
  specs?: string[] | null
}): Promise<PagedProductsResponse> {
  const { data } = await api.get<PagedProductsResponse>('/products', {
    params: {
      page: params.page,
      pageSize: params.pageSize,
      sort: params.sort,
      category: params.category,
      search: params.search,
      minPrice: params.minPrice ?? undefined,
      maxPrice: params.maxPrice ?? undefined,
      specs: params.specs ?? undefined,
    },
    paramsSerializer: {
      indexes: null,
    },
  })

  return data
}

export function useProducts(options: UseProductsParams = {}) {
  const [page, setPage] = useState(() => clampInt(options.initialPage, 1, 1, 1_000_000))
  const [pageSize, setPageSize] = useState(() =>
    clampInt(options.initialPageSize, 20, 1, 100),
  )

  const queryParams = useMemo(
    () => ({
      page,
      pageSize,
      sort: options.sort?.trim() ? options.sort.trim() : undefined,
      category: options.category ?? null,
      search: options.search?.trim() ? options.search.trim() : undefined,
      minPrice: options.minPrice ?? null,
      maxPrice: options.maxPrice ?? null,
      specs: options.specs ?? null,
    }),
    [page, pageSize, options],
  )

  const query = useQuery<PagedProductsResponse>({
    queryKey: ['products', queryParams],
    queryFn: () => fetchProducts(queryParams),
    staleTime: 30_000,
  })

  const data = query.data

  return {
    ...query,
    page,
    pageSize,
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    items: (data?.items ?? []) as ProductListItem[],
    setPage: (nextPage: number) => setPage(clampInt(nextPage, 1, 1, 1_000_000)),
    setPageSize: (nextSize: number) => {
      const clamped = clampInt(nextSize, 20, 1, 100)
      setPageSize(clamped)
      setPage(1)
    },
  }
}

