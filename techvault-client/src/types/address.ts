export type AddressDto = {
  id: number
  label: string
  fullName: string
  line1: string
  line2: string | null
  city: string
  region: string | null
  postalCode: string
  country: string
  phone: string | null
  isDefaultShipping: boolean
  isDefaultBilling: boolean
  createdAtUtc: string
}
