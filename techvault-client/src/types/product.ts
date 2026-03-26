/** Matches TechVault.API `ProductDto` (camelCase JSON). */
export type ProductCategoryDto = {
  id: number
  name: string
  slug: string
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
