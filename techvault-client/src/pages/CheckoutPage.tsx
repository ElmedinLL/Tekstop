import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { validateCoupon } from '../lib/coupon'
import { fetchAddresses, createAddress } from '../lib/addresses'
import { useCartStore } from '../store/useCartStore'
import type { AddressDto } from '../types/address'

const addressSchema = z.object({
  label: z.string().min(1, 'Label is required'),
  fullName: z.string().min(1, 'Name is required'),
  line1: z.string().min(1, 'Address line 1 is required'),
  line2: z.string().optional(),
  city: z.string().min(1, 'City is required'),
  region: z.string().optional(),
  postalCode: z.string().min(1, 'Postal code is required'),
  country: z.string().min(1, 'Country is required'),
  phone: z.string().optional(),
  isDefaultShipping: z.boolean(),
  isDefaultBilling: z.boolean(),
})

type AddressFormValues = z.infer<typeof addressSchema>

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

export function CheckoutPage() {
  const location = useLocation()
  const [step, setStep] = useState(1)
  const [selectedAddressId, setSelectedAddressId] = useState<number | null>(null)
  const [useNewAddress, setUseNewAddress] = useState(false)
  const [shippingMethod, setShippingMethod] = useState<'standard' | 'express'>('standard')
  const [couponInput, setCouponInput] = useState('')
  const [appliedDiscount, setAppliedDiscount] = useState(0)
  const [appliedCode, setAppliedCode] = useState<string | null>(null)

  const serverCart = useCartStore((s) => s.serverCart)
  const fetchCart = useCartStore((s) => s.fetchCart)

  const { data: addresses = [], isLoading: addressesLoading } = useQuery({
    queryKey: ['addresses'],
    queryFn: fetchAddresses,
  })

  const form = useForm<AddressFormValues>({
    resolver: zodResolver(addressSchema),
    defaultValues: {
      label: 'Home',
      fullName: '',
      line1: '',
      line2: '',
      city: '',
      region: '',
      postalCode: '',
      country: 'United States',
      phone: '',
      isDefaultShipping: true,
      isDefaultBilling: true,
    },
  })

  useEffect(() => {
    if (!useNewAddress && addresses.length > 0 && selectedAddressId === null) {
      const def = addresses.find((a) => a.isDefaultShipping) ?? addresses[0]
      setSelectedAddressId(def.id)
    }
  }, [addresses, selectedAddressId, useNewAddress])

  useEffect(() => {
    const st = location.state as { couponCode?: string | null; discountAmount?: number } | null
    if (st?.couponCode) {
      setCouponInput(st.couponCode)
      setAppliedCode(st.couponCode)
      setAppliedDiscount(st.discountAmount ?? 0)
    }
  }, [location.state])

  useEffect(() => {
    if (step === 2) {
      void fetchCart()
    }
  }, [step, fetchCart])

  const handleAddressNext = async () => {
    let addressId = selectedAddressId
    if (useNewAddress) {
      const ok = await form.trigger()
      if (!ok) {
        toast.error('Fix the address form.')
        return
      }
      const v = form.getValues()
      try {
        const created = await createAddress({
          label: v.label,
          fullName: v.fullName,
          line1: v.line1,
          line2: v.line2 || undefined,
          city: v.city,
          region: v.region || undefined,
          postalCode: v.postalCode,
          country: v.country,
          phone: v.phone || undefined,
          isDefaultShipping: v.isDefaultShipping,
          isDefaultBilling: v.isDefaultBilling,
        })
        addressId = created.id
      } catch {
        toast.error('Could not save address.')
        return
      }
    } else if (addressId === null) {
      toast.error('Select an address.')
      return
    }

    setSelectedAddressId(addressId)
    setStep(2)
  }

  if (addressesLoading && step === 1) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center text-slate-600">
        Loading addresses…
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <nav className="mb-6 text-sm text-slate-500">
        <Link to="/cart" className="text-blue-600 hover:underline">
          ← Back to cart
        </Link>
      </nav>

      <ol className="mb-8 flex gap-4 text-sm font-medium text-slate-500">
        <li className={step === 1 ? 'text-blue-600' : ''}>1. Address</li>
        <li className={step === 2 ? 'text-blue-600' : ''}>2. Shipping</li>
        <li className={step === 3 ? 'text-blue-600' : ''}>3. Payment</li>
      </ol>

      {step === 1 && (
        <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h1 className="text-xl font-semibold text-slate-900">Shipping address</h1>
          <p className="mt-1 text-sm text-slate-600">Choose a saved address or add a new one.</p>

          {addresses.length > 0 && (
            <div className="mt-6 space-y-3">
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="radio"
                  checked={!useNewAddress}
                  onChange={() => setUseNewAddress(false)}
                />
                Use a saved address
              </label>
              {!useNewAddress && (
                <ul className="ml-6 space-y-2">
                  {addresses.map((a: AddressDto) => (
                    <li key={a.id}>
                      <label className="flex cursor-pointer gap-2 rounded-lg border border-slate-200 p-3 hover:bg-slate-50">
                        <input
                          type="radio"
                          name="addr"
                          checked={selectedAddressId === a.id}
                          onChange={() => setSelectedAddressId(a.id)}
                        />
                        <span className="text-sm text-slate-800">
                          <span className="font-medium">{a.label}</span> — {a.fullName}, {a.line1}, {a.city},{' '}
                          {a.postalCode}
                        </span>
                      </label>
                    </li>
                  ))}
                </ul>
              )}
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="radio"
                  checked={useNewAddress}
                  onChange={() => setUseNewAddress(true)}
                />
                Add a new address
              </label>
            </div>
          )}

          {(useNewAddress || addresses.length === 0) && (
            <form className="mt-6 grid gap-4 sm:grid-cols-2" onSubmit={(e) => e.preventDefault()}>
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Label</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('label')} />
                {form.formState.errors.label && (
                  <p className="text-xs text-red-600">{form.formState.errors.label.message}</p>
                )}
              </div>
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Full name</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('fullName')} />
                {form.formState.errors.fullName && (
                  <p className="text-xs text-red-600">{form.formState.errors.fullName.message}</p>
                )}
              </div>
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Address line 1</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('line1')} />
              </div>
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Address line 2</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('line2')} />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">City</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('city')} />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Region / State</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('region')} />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Postal code</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('postalCode')} />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Country</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('country')} />
              </div>
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Phone</label>
                <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2" {...form.register('phone')} />
              </div>
              <label className="flex items-center gap-2 text-sm sm:col-span-2">
                <Controller
                  name="isDefaultShipping"
                  control={form.control}
                  render={({ field }) => (
                    <input
                      type="checkbox"
                      checked={field.value}
                      onChange={(e) => field.onChange(e.target.checked)}
                      onBlur={field.onBlur}
                      ref={field.ref}
                    />
                  )}
                />
                Default shipping
              </label>
              <label className="flex items-center gap-2 text-sm sm:col-span-2">
                <Controller
                  name="isDefaultBilling"
                  control={form.control}
                  render={({ field }) => (
                    <input
                      type="checkbox"
                      checked={field.value}
                      onChange={(e) => field.onChange(e.target.checked)}
                      onBlur={field.onBlur}
                      ref={field.ref}
                    />
                  )}
                />
                Default billing
              </label>
            </form>
          )}

          <button
            type="button"
            onClick={() => void handleAddressNext()}
            className="mt-8 rounded-lg bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-blue-700"
          >
            Continue to shipping
          </button>
        </section>
      )}

      {step === 2 && (
        <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h1 className="text-xl font-semibold text-slate-900">Shipping &amp; discount</h1>
          <p className="mt-1 text-sm text-slate-600">Choose delivery speed and apply a coupon if you have one.</p>

          <fieldset className="mt-6 space-y-3">
            <legend className="text-sm font-medium text-slate-800">Shipping method</legend>
            <label className="flex cursor-pointer items-center justify-between gap-4 rounded-lg border border-slate-200 p-3 hover:bg-slate-50">
              <span>
                <span className="font-medium text-slate-900">Standard</span>
                <span className="ml-2 text-sm text-slate-500">5–7 business days</span>
              </span>
              <span className="flex items-center gap-2">
                <span className="text-sm font-semibold">{formatMoney(9.99)}</span>
                <input
                  type="radio"
                  name="ship"
                  checked={shippingMethod === 'standard'}
                  onChange={() => setShippingMethod('standard')}
                />
              </span>
            </label>
            <label className="flex cursor-pointer items-center justify-between gap-4 rounded-lg border border-slate-200 p-3 hover:bg-slate-50">
              <span>
                <span className="font-medium text-slate-900">Express</span>
                <span className="ml-2 text-sm text-slate-500">2–3 business days</span>
              </span>
              <span className="flex items-center gap-2">
                <span className="text-sm font-semibold">{formatMoney(19.99)}</span>
                <input
                  type="radio"
                  name="ship"
                  checked={shippingMethod === 'express'}
                  onChange={() => setShippingMethod('express')}
                />
              </span>
            </label>
          </fieldset>

          <div className="mt-8 rounded-lg border border-slate-200 bg-slate-50 p-4">
            <h2 className="text-sm font-semibold text-slate-800">Order summary</h2>
            <dl className="mt-3 space-y-1 text-sm text-slate-600">
              <div className="flex justify-between">
                <dt>Subtotal</dt>
                <dd>{formatMoney(serverCart?.subTotal ?? 0)}</dd>
              </div>
              <div className="flex justify-between">
                <dt>Shipping</dt>
                <dd>{formatMoney(shippingMethod === 'express' ? 19.99 : 9.99)}</dd>
              </div>
              {appliedDiscount > 0 && (
                <div className="flex justify-between text-emerald-700">
                  <dt>Discount {appliedCode ? `(${appliedCode})` : ''}</dt>
                  <dd>-{formatMoney(appliedDiscount)}</dd>
                </div>
              )}
              <div className="flex justify-between border-t border-slate-200 pt-2 font-semibold text-slate-900">
                <dt>Estimated total</dt>
                <dd>
                  {formatMoney(
                    Math.max(
                      0,
                      (serverCart?.subTotal ?? 0) -
                        appliedDiscount +
                        (shippingMethod === 'express' ? 19.99 : 9.99),
                    ),
                  )}
                </dd>
              </div>
            </dl>
          </div>

          <div className="mt-6">
            <label htmlFor="checkout-coupon" className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Coupon code
            </label>
            <div className="mt-2 flex gap-2">
              <input
                id="checkout-coupon"
                value={couponInput}
                onChange={(e) => setCouponInput(e.target.value)}
                className="min-w-0 flex-1 rounded border border-slate-300 px-3 py-2 text-sm"
                placeholder="Optional"
              />
              <button
                type="button"
                className="rounded bg-slate-800 px-3 py-2 text-sm font-medium text-white hover:bg-slate-900"
                onClick={async () => {
                  if (!couponInput.trim()) {
                    toast.error('Enter a coupon code.')
                    return
                  }
                  try {
                    const res = await validateCoupon(couponInput.trim(), serverCart?.subTotal ?? 0)
                    if (!res.isValid) {
                      toast.error(res.message)
                      setAppliedDiscount(0)
                      setAppliedCode(null)
                      return
                    }
                    setAppliedDiscount(res.discountAmount)
                    setAppliedCode(res.code ?? couponInput.trim())
                    toast.success(res.message)
                  } catch {
                    toast.error('Could not validate coupon.')
                  }
                }}
              >
                Apply
              </button>
            </div>
          </div>

          <div className="mt-8 flex flex-wrap gap-3">
            <button
              type="button"
              className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-800 hover:bg-slate-50"
              onClick={() => setStep(1)}
            >
              Back
            </button>
            <button
              type="button"
              className="rounded-lg bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-blue-700"
              onClick={() => setStep(3)}
            >
              Continue to payment
            </button>
          </div>
        </section>
      )}

      {step === 3 && (
        <section className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center text-slate-600">
          <p>Payment — next commit.</p>
          <Link to="/" className="mt-4 inline-block text-blue-600 hover:underline">
            Home
          </Link>
        </section>
      )}
    </div>
  )
}
