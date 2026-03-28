import { useState } from 'react'
import { CardElement, useElements, useStripe } from '@stripe/react-stripe-js'
import { isAxiosError } from 'axios'
import { toast } from '../lib/notifications'
import { createOrder } from '../lib/orders'
import { createPaymentIntent } from '../lib/payments'
import type { CartDto } from '../types/cart'

const cardElementOptions = {
  style: {
    base: {
      fontSize: '16px',
      color: '#0f172a',
      '::placeholder': { color: '#94a3b8' },
    },
    invalid: { color: '#b91c1c' },
  },
}

type CheckoutStripeCardFormProps = {
  selectedAddressId: number
  shippingMethod: 'standard' | 'express'
  appliedCode: string | null
  serverCart: CartDto | null
  refetchCart: () => Promise<void>
  onSuccess: (orderId: number) => void
  onBack: () => void
}

export function CheckoutStripeCardForm({
  selectedAddressId,
  shippingMethod,
  appliedCode,
  serverCart,
  refetchCart,
  onSuccess,
  onBack,
}: CheckoutStripeCardFormProps) {
  const stripe = useStripe()
  const elements = useElements()
  const [billingName, setBillingName] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handlePay = async () => {
    if (!stripe || !elements) {
      toast.error('Stripe is still loading. Try again in a moment.')
      return
    }
    if (!billingName.trim()) {
      toast.error('Enter the name on your card.')
      return
    }
    const card = elements.getElement(CardElement)
    if (!card) {
      toast.error('Card field is not ready.')
      return
    }
    if (!serverCart?.lines.length) {
      toast.error('Your cart is empty.')
      return
    }

    setIsSubmitting(true)
    try {
      const order = await createOrder({
        addressId: selectedAddressId,
        paymentMethod: 'card',
        couponCode: appliedCode,
        shippingMethod,
        cartItems: serverCart.lines.map((l) => ({
          productId: l.productId,
          quantity: l.quantity,
        })),
      })

      const { clientSecret } = await createPaymentIntent(order.id, order.total)
      if (!clientSecret) {
        toast.error('Could not start payment. Check Stripe configuration.')
        return
      }

      const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
        payment_method: {
          card,
          billing_details: { name: billingName.trim() },
        },
      })

      if (error) {
        toast.error(error.message ?? 'Payment failed.')
        return
      }

      if (paymentIntent?.status === 'succeeded' || paymentIntent?.status === 'processing') {
        await refetchCart()
        onSuccess(order.id)
        return
      }

      toast.error('Payment was not completed. Please try again.')
    } catch (e: unknown) {
      const message = isAxiosError(e)
        ? typeof e.response?.data === 'string'
          ? e.response.data
          : e.message
        : e instanceof Error
          ? e.message
          : 'Could not place order.'
      toast.error(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="mt-6 space-y-4">
      <div>
        <label htmlFor="stripe-billing-name" className="text-xs font-medium text-slate-600">
          Name on card
        </label>
        <input
          id="stripe-billing-name"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
          value={billingName}
          onChange={(e) => setBillingName(e.target.value)}
          placeholder="Jane Doe"
          autoComplete="cc-name"
        />
      </div>
      <div>
        <label className="text-xs font-medium text-slate-600">Card</label>
        <div className="mt-1 rounded border border-slate-300 bg-white px-3 py-3">
          <CardElement options={cardElementOptions} />
        </div>
        <p className="mt-1 text-xs text-slate-500">Test: 4242 4242 4242 4242, any future expiry, any CVC.</p>
      </div>

      <div className="mt-8 flex flex-wrap gap-3">
        <button
          type="button"
          className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-800 hover:bg-slate-50"
          onClick={onBack}
          disabled={isSubmitting}
        >
          Back
        </button>
        <button
          type="button"
          disabled={isSubmitting || !selectedAddressId || !serverCart?.lines.length}
          className="rounded-lg bg-emerald-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:bg-slate-300"
          onClick={() => void handlePay()}
        >
          {isSubmitting ? 'Processing…' : 'Pay and place order'}
        </button>
      </div>
    </div>
  )
}
