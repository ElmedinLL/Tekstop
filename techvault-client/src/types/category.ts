/** Matches TechVault.API `CategoryListItemDto` list response. */
export type CategoryListItem = {
  id: number
  name: string
  slug: string
  description?: string | null
  parentCategoryId?: number | null
  displayOrder: number
  isActive: boolean
  productCount: number
}
