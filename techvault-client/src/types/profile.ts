export type UserProfile = {
  userId: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string | null
  avatarUrl?: string | null
}

export type UpdateProfileBody = {
  firstName: string
  lastName: string
  phone?: string | null
}

export type ChangePasswordBody = {
  currentPassword: string
  newPassword: string
}
