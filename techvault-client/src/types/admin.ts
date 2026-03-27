export type RevenueByMonth = {
  year: number
  month: number
  revenue: number
}

export type AdminStats = {
  totalRevenue: number
  totalOrders: number
  totalUsers: number
  productsCount: number
  revenueByMonth: RevenueByMonth[]
}

export type AdminOrderListItem = {
  id: number
  orderNumber: string
  status: string
  placedAtUtc: string
  total: number
  currency: string
  lineItemCount: number
  customerEmail: string | null
}

export type AdminOrderListResult = {
  items: AdminOrderListItem[]
  page: number
  pageSize: number
  totalCount: number
}

export type LowStockProduct = {
  id: number
  name: string
  sku: string
  stockQuantity: number
  isPublished: boolean
  categoryName: string
}
