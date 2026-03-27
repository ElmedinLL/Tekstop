/** Matches TechVault.API `ProductDto` (camelCase JSON). */
export type ProductCategoryDto = {
  id: number
  name: string
  slug: string
}

/** Matches `ProductListItemDto` from paged product lists. */
export type ProductListItem = {
  id: number
  name: string
  slug: string
  price: number
  compareAtPrice: number | null
  imageUrl: string | null
  brand: string | null
  categoryName: string
  categorySlug: string
  stockQuantity: number
  isPublished: boolean
}

export type PagedProductsResponse = {
  items: ProductListItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export type ProductDetail = {
  id: number
  name: string
  slug: string
  sku: string
  description: string | null
  price: number
  stock: number
  images: string[]
  category: ProductCategoryDto
  specs: Record<string, string>
}
