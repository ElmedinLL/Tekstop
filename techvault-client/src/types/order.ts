export type CreateOrderBody = {
  addressId: number
  paymentMethod: string
  couponCode?: string | null
  shippingMethod: string
  cartItems: { productId: number; quantity: number }[]
}

export type OrderDto = {
  id: number
  orderNumber: string
  status: string
  subTotal: number
  taxAmount: number
  shippingAmount: number
  discountAmount: number
  total: number
  currency: string
  paymentMethod?: string | null
  couponCode?: string | null
  placedAtUtc: string
  estimatedDeliveryUtc?: string | null
  items: {
    productId: number
    productName: string
    productSku: string
    unitPrice: number
    quantity: number
    lineTotal: number
  }[]
}
