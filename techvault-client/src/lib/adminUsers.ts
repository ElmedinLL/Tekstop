import type { AdminUserListResult } from '../types/adminUser'
import { api } from './api'

export type AdminUserListParams = {
  page?: number
  pageSize?: number
  search?: string
}

export async function fetchAdminUsers(params: AdminUserListParams = {}) {
  const { data } = await api.get<AdminUserListResult>('/admin/users', {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
      search: params.search?.trim() || undefined,
    },
  })
  return data
}

export async function setAdminUserBanned(userId: string, banned: boolean) {
  await api.put(`/admin/users/${encodeURIComponent(userId)}/ban`, { banned })
}
