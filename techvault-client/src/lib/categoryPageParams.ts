/**
 * Maps URL search params to `GET /api/products/category/{slug}` query parameters.
 * Omits keys the category endpoint does not use (`categories`, `minRating`, `inStockOnly`).
 */
const VALID_SORT = new Set([
  'newest',
  'oldest',
  'name_asc',
  'name_desc',
  'price_asc',
  'price_desc',
  'stock_desc',
])

export function normalizeSortParam(value: string | null): string {
  const v = value?.trim().toLowerCase() ?? ''
  return VALID_SORT.has(v) ? v : 'newest'
}

export function buildCategoryProductsApiParams(searchParams: URLSearchParams): Record<string, string | string[]> {
  const sort = normalizeSortParam(searchParams.get('sort'))
  const page = searchParams.get('page')?.trim() || '1'
  const pageSize = searchParams.get('pageSize')?.trim() || '20'

  const params: Record<string, string | string[]> = {
    sort,
    page,
    pageSize,
  }

  const minPrice = searchParams.get('minPrice')?.trim()
  const maxPrice = searchParams.get('maxPrice')?.trim()
  const search = searchParams.get('search')?.trim()

  if (minPrice) {
    params.minPrice = minPrice
  }
  if (maxPrice) {
    params.maxPrice = maxPrice
  }
  if (search) {
    params.search = search
  }

  const specs = searchParams.getAll('specs').map((s) => s.trim()).filter(Boolean)
  if (specs.length > 0) {
    params.specs = specs
  }

  return params
}
