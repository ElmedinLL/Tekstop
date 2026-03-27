import { loadStripe, type Stripe } from '@stripe/stripe-js'

const key = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY as string | undefined

/** Resolves when Stripe.js is ready; `null` if no publishable key is configured. */
export const stripePromise: Promise<Stripe | null> | null =
  key != null && key.length > 0 ? loadStripe(key) : null
