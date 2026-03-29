import type {
  AdminProductDto,
  CreateProductBody,
  ProductImageUploadResponse,
  UpdateProductBody,
} from '../types/adminProduct'
import { api } from './api'

export async function fetchAdminProduct(id: number) {
  const { data } = await api.get<AdminProductDto>(`/admin/products/${id}`)
  return data
}

export async function createProduct(body: CreateProductBody) {
  const { data } = await api.post<AdminProductDto>('/products', body)
  return data
}

export async function updateProduct(id: number, body: UpdateProductBody) {
  const { data } = await api.put<AdminProductDto>(`/products/${id}`, body)
  return data
}

export async function uploadProductImage(productId: number, file: File) {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await api.post<ProductImageUploadResponse>(`/products/${productId}/images`, formData)
  return data
}

/** Soft-deletes a product (admin); removes it from the storefront. */
export async function deleteProduct(id: number): Promise<void> {
  await api.delete(`/products/${id}`)
}
