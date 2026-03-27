import type { CategoryDetail, CategoryListItem, CreateCategoryBody, UpdateCategoryBody } from '../types/category'
import { api } from './api'

export async function fetchCategories() {
  const { data } = await api.get<CategoryListItem[]>('/categories')
  return data
}

export async function createCategory(body: CreateCategoryBody) {
  const { data } = await api.post<CategoryDetail>('/categories', body)
  return data
}

export async function updateCategory(slug: string, body: UpdateCategoryBody) {
  const { data } = await api.put<CategoryDetail>(`/categories/${encodeURIComponent(slug)}`, body)
  return data
}

export async function deleteCategory(slug: string) {
  await api.delete(`/categories/${encodeURIComponent(slug)}`)
}

export async function uploadCategoryImage(file: File) {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await api.post<{ url: string }>('/categories/upload-image', formData)
  return data
}
