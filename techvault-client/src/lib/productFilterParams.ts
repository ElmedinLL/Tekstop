/** Query keys used by ProductFilters and product listing. */
export const ProductFilterKeys = {
  minPrice: 'minPrice',
  maxPrice: 'maxPrice',
  minRating: 'minRating',
  inStockOnly: 'inStockOnly',
  categories: 'categories',
  page: 'page',
} as const

export type ProductFilterKeysType = (typeof ProductFilterKeys)[keyof typeof ProductFilterKeys]

/** Parse comma-separated category slugs from the URL. */
export function parseCategorySlugs(param: string | null): string[] {
  if (param == null || param.trim() === '') {
    return []
  }
  return param
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean)
}

/** Serialize selected category slugs for the URL (comma-separated). */
export function serializeCategorySlugs(slugs: string[]): string | null {
  const unique = [...new Set(slugs.map((s) => s.trim()).filter(Boolean))]
  if (unique.length === 0) {
    return null
  }
  return unique.join(',')
}

export function parseInStockOnly(param: string | null): boolean {
  if (param == null) {
    return false
  }
  const v = param.trim().toLowerCase()
  return v === '1' || v === 'true' || v === 'yes'
}

export function parseMinRating(param: string | null): number | null {
  if (param == null || param.trim() === '') {
    return null
  }
  const n = Number(param)
  if (!Number.isFinite(n) || n < 1 || n > 5) {
    return null
  }
  return Math.floor(n)
}
