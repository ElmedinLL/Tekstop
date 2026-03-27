export type AdminUserListItem = {
  userId: string
  email: string
  firstName: string
  lastName: string
  avatarUrl?: string | null
  roles: string[]
  joinedAtUtc: string
  orderCount: number
  isBanned: boolean
}

export type AdminUserListResult = {
  items: AdminUserListItem[]
  page: number
  pageSize: number
  totalCount: number
}
