export type CartLineDto = {
  cartItemId: number
  productId: number
  name: string
  imageUrl: string | null
  unitPrice: number
  compareAtPrice: number | null
  quantity: number
  lineTotal: number
  lineDiscount: number
}

export type CartDto = {
  lines: CartLineDto[]
  subTotal: number
  discountTotal: number
  totalItemCount: number
  isAuthenticated: boolean
}

export type GuestCartLine = {
  productId: number
  quantity: number
}
