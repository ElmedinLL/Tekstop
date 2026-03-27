import type { CategoryListItem } from '../types/category'
import { api } from './api'

export async function fetchCategories() {
  const { data } = await api.get<CategoryListItem[]>('/categories')
  return data
}
