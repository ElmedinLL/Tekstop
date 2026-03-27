export type AdminOrderSummary = {
  id: number
  orderNumber: string
  customerEmail: string
  customerName: string
  placedAtUtc: string
  total: number
  currency: string
  status: string
  lineItemCount: number
}

export type AdminOrderListResult = {
  items: AdminOrderSummary[]
  page: number
  pageSize: number
  totalCount: number
}

export type AdminOrderDetail = {
  id: number
  orderNumber: string
  status: string
  customerEmail?: string | null
  customerName?: string | null
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
  shippingFullName: string
  shippingLine1: string
  shippingLine2?: string | null
  shippingCity: string
  shippingRegion?: string | null
  shippingPostalCode: string
  shippingCountry: string
  shippingPhone?: string | null
  items: {
    productId: number
    productName: string
    productSku: string
    unitPrice: number
    quantity: number
    lineTotal: number
  }[]
}
