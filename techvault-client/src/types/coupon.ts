export type ValidateCouponResponse = {
  isValid: boolean
  message: string
  code?: string | null
  discountType?: number | null
  discountAmount: number
}
