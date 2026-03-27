import { api } from './api'

export type PaymentIntentCreateResult = {
  paymentIntentId: string
  clientSecret: string
  amount: number
  currency: string
}

export async function createPaymentIntent(orderId: number, amount: number) {
  const { data } = await api.post<PaymentIntentCreateResult>('/payments/create-intent', {
    orderId,
    amount,
  })
  return data
}
