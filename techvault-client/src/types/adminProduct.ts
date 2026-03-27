import type { ProductCategoryDto } from './product'

/** Matches extended `ProductDto` from admin GET /admin/products/{id}. */
export type AdminProductDto = {
  id: number
  name: string
  slug: string
  sku: string
  shortDescription: string | null
  description: string | null
  price: number
  compareAtPrice: number | null
  stock: number
  brand: string | null
  isPublished: boolean
  averageRating?: number | null
  reviewCount?: number
  images: string[]
  category: ProductCategoryDto
  specs: Record<string, string>
}

/** Request body for POST /api/products (create). */
export type CreateProductBody = {
  name: string
  slug: string
  sku: string
  shortDescription?: string | null
  description?: string | null
  price: number
  compareAtPrice?: number | null
  stock: number
  categoryId: number
  brand?: string | null
  isPublished: boolean
  images: string[]
  specs: Record<string, string>
}

/** Request body for PUT /api/products/{id} (update). */
export type UpdateProductBody = {
  name?: string
  slug?: string
  sku?: string
  shortDescription?: string | null
  description?: string | null
  price?: number
  compareAtPrice?: number | null
  stock?: number
  categoryId?: number
  brand?: string | null
  isPublished?: boolean
  images?: string[]
  specs?: Record<string, string>
}

export type ProductImageUploadResponse = {
  id: number
  url: string
  sortOrder: number
}
