import type { ChangePasswordBody, UpdateProfileBody, UserProfile } from '../types/profile'
import { api } from './api'

export async function fetchUserProfile() {
  const { data } = await api.get<UserProfile>('/users/profile')
  return data
}

export async function updateUserProfile(body: UpdateProfileBody) {
  const { data } = await api.put<UserProfile>('/users/profile', body)
  return data
}

export async function uploadUserAvatar(file: File) {
  const form = new FormData()
  form.append('file', file)
  const { data } = await api.post<{ avatarUrl: string }>('/users/profile/avatar', form)
  return data
}

export async function changePassword(body: ChangePasswordBody) {
  await api.put('/users/change-password', body)
}
