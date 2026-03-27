import type { AdminCoupon, CreateAdminCouponBody } from '../types/adminCoupon'
import { api } from './api'

export async function fetchAdminCoupons() {
  const { data } = await api.get<AdminCoupon[]>('/admin/coupons')
  return data
}

export async function createAdminCoupon(body: CreateAdminCouponBody) {
  const { data } = await api.post<AdminCoupon>('/admin/coupons', body)
  return data
}

export async function updateAdminCoupon(id: number, body: CreateAdminCouponBody) {
  const { data } = await api.put<AdminCoupon>(`/admin/coupons/${id}`, body)
  return data
}

export async function deleteAdminCoupon(id: number) {
  await api.delete(`/admin/coupons/${id}`)
}
