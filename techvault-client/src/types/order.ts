export type CreateOrderBody = {
  addressId: number
  paymentMethod: string
  couponCode?: string | null
  shippingMethod: string
  cartItems: { productId: number; quantity: number }[]
}

export type OrderLineItem = {
  productId: number
  productName: string
  productSku: string
  unitPrice: number
  quantity: number
  lineTotal: number
  imageUrl?: string | null
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
  confirmedAtUtc?: string | null
  processingAtUtc?: string | null
  paidAtUtc?: string | null
  estimatedDeliveryUtc?: string | null
  shippedAtUtc?: string | null
  deliveredAtUtc?: string | null
  cancelledAtUtc?: string | null
  trackingUrl?: string | null
  /** Snapshot from order; may be absent on older API responses. */
  shippingFullName?: string | null
  shippingLine1?: string | null
  shippingLine2?: string | null
  shippingCity?: string | null
  shippingRegion?: string | null
  shippingPostalCode?: string | null
  shippingCountry?: string | null
  shippingPhone?: string | null
  items: OrderLineItem[]
}
