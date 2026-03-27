import { api } from './api'
import type { ValidateCouponResponse } from '../types/coupon'

export async function validateCoupon(code: string, orderSubtotal: number) {
  const { data } = await api.post<ValidateCouponResponse>('/coupons/validate', {
    code,
    orderSubtotal,
  })
  return data
}
