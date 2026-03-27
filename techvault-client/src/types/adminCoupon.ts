/** Matches API `DiscountType`: 0 = Percent, 1 = Fixed */
export type AdminDiscountType = 0 | 1

export type AdminCoupon = {
  id: number
  code: string
  discountType: AdminDiscountType
  discountValue: number
  minOrderValue: number
  expiresAtUtc: string | null
  usageLimit: number | null
  usageCount: number
  isActive: boolean
}

export type CreateAdminCouponBody = {
  code: string
  discountType: AdminDiscountType
  discountValue: number
  minOrderValue: number
  expiresAtUtc: string | null
  usageLimit: number | null
  isActive: boolean
}
