/** Matches TechVault.API `CategoryListItemDto` list response. */
export type CategoryListItem = {
  id: number
  name: string
  slug: string
  description?: string | null
  imageUrl?: string | null
  parentCategoryId?: number | null
  displayOrder: number
  isActive: boolean
  productCount: number
}

/** Matches TechVault.API `CategoryDetailDto` (GET `/categories/{slug}`). */
export type CategoryDetail = {
  id: number
  name: string
  slug: string
  description?: string | null
  imageUrl?: string | null
  parentCategoryId?: number | null
  displayOrder: number
  isActive: boolean
  productCount: number
}

export type CreateCategoryBody = {
  name: string
  slug?: string | null
  description?: string | null
  imageUrl?: string | null
  parentCategoryId?: number | null
  displayOrder: number
  isActive: boolean
}

export type UpdateCategoryBody = CreateCategoryBody
