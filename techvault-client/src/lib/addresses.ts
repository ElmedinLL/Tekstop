import type { AddressDto } from '../types/address'
import { api } from './api'

export type CreateAddressBody = {
  label: string
  fullName: string
  line1: string
  line2?: string | null
  city: string
  region?: string | null
  postalCode: string
  country: string
  phone?: string | null
  isDefaultShipping: boolean
  isDefaultBilling: boolean
}

export async function fetchAddresses() {
  const { data } = await api.get<AddressDto[]>('/addresses')
  return data
}

export async function createAddress(body: CreateAddressBody) {
  const { data } = await api.post<AddressDto>('/addresses', body)
  return data
}
