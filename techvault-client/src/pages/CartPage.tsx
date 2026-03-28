import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useCartQuery, useRemoveCartLineMutation, useUpdateCartLineMutation } from '../hooks/useCart'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { validateCoupon } from '../lib/coupon'
import { Seo } from '../components/Seo'
import { toast } from '../lib/notifications'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

export function CartPage() {
  const { data: serverCart, isPending: cartPending, isError: cartError } = useCartQuery()
  const updateLineMu = useUpdateCartLineMutation()
  const removeLineMu = useRemoveCartLineMutation()
  const couponMu = useMutation({
    mutationFn: ({ code, subTotal }: { code: string; subTotal: number }) => validateCoupon(code, subTotal),
  })
  const [couponInput, setCouponInput] = useState('')
  const [appliedDiscount, setAppliedDiscount] = useState(0)
  const [appliedCode, setAppliedCode] = useState<string | null>(null)

  const lines = serverCart?.lines ?? []
  const subTotal = serverCart?.subTotal ?? 0

  const estimatedTotal = Math.max(0, subTotal - appliedDiscount)

  const handleApplyCoupon = () => {
    if (!couponInput.trim()) {
      toast.error('Enter a coupon code.')
      return
    }
    couponMu.mutate(
      { code: couponInput.trim(), subTotal },
      {
        onSuccess: (res) => {
          if (!res.isValid) {
            toast.error(res.message)
            setAppliedDiscount(0)
            setAppliedCode(null)
            return
          }
          setAppliedDiscount(res.discountAmount)
          setAppliedCode(res.code ?? couponInput.trim())
          toast.success(res.message)
        },
        onError: () => {
          toast.error('Could not validate coupon.')
        },
      },
    )
  }

  if (cartPending) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center text-slate-600">
        <Seo title="Cart" description="Loading your shopping cart…" />
        <p>Loading cart…</p>
      </div>
    )
  }

  if (cartError || !serverCart) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center text-slate-600">
        <Seo title="Cart" description="Your shopping cart could not be loaded." />
        <p className="text-rose-600">Could not load your cart.</p>
        <Link to="/" className="mt-4 inline-block text-blue-600 hover:underline">
          Continue shopping
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <Seo
        title="Shopping cart"
        description="Review items, apply coupons, and proceed to checkout on TechVault."
      />
      <h1 className="text-2xl font-semibold text-slate-900">Shopping cart</h1>
      {lines.length === 0 ? (
        <p className="mt-8 text-slate-600">
          Your cart is empty.{' '}
          <Link to="/" className="text-blue-600 hover:underline">
            Continue shopping
          </Link>
        </p>
      ) : (
        <div className="mt-8 grid gap-10 lg:grid-cols-[1fr_320px]">
          <ul className="space-y-4">
            {lines.map((line) => (
              <li
                key={`${line.productId}-${line.cartItemId}`}
                className="flex gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
              >
                <div className="h-24 w-24 shrink-0 overflow-hidden rounded-lg bg-slate-100">
                  {line.imageUrl ? (
                    <img
                      src={resolveApiAssetUrl(line.imageUrl)}
                      alt=""
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <div className="flex h-full w-full items-center justify-center text-xs text-slate-400">No image</div>
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="font-medium text-slate-900">{line.name}</p>
                  <p className="mt-1 text-sm text-slate-500">{formatMoney(line.unitPrice)} each</p>
                  <div className="mt-3 flex flex-wrap items-center gap-3">
                    <label className="sr-only" htmlFor={`qty-${line.productId}`}>
                      Quantity
                    </label>
                    <input
                      id={`qty-${line.productId}`}
                      type="number"
                      min={1}
                      value={line.quantity}
                      onChange={(e) => {
                        const v = Number.parseInt(e.target.value, 10)
                        if (Number.isNaN(v) || v < 1) {
                          return
                        }
                        updateLineMu.mutate({ productId: line.productId, cartItemId: line.cartItemId, quantity: v })
                      }}
                      className="w-20 rounded border border-slate-300 px-2 py-1 text-sm"
                    />
                    <button
                      type="button"
                      className="text-sm text-red-600 hover:underline"
                      onClick={() => removeLineMu.mutate({ productId: line.productId, cartItemId: line.cartItemId })}
                    >
                      Remove
                    </button>
                  </div>
                </div>
                <div className="text-right">
                  <p className="font-semibold text-slate-900">{formatMoney(line.lineTotal)}</p>
                  <p className="text-xs text-slate-500">Subtotal</p>
                </div>
              </li>
            ))}
          </ul>

          <aside className="h-fit rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-semibold text-slate-900">Order summary</h2>
            <dl className="mt-4 space-y-2 text-sm">
              <div className="flex justify-between text-slate-600">
                <dt>Subtotal</dt>
                <dd>{formatMoney(subTotal)}</dd>
              </div>
              {appliedDiscount > 0 && (
                <div className="flex justify-between text-emerald-700">
                  <dt>Discount {appliedCode ? `(${appliedCode})` : ''}</dt>
                  <dd>-{formatMoney(appliedDiscount)}</dd>
                </div>
              )}
              <div className="flex justify-between border-t border-slate-200 pt-2 text-base font-semibold text-slate-900">
                <dt>Estimated total</dt>
                <dd>{formatMoney(estimatedTotal)}</dd>
              </div>
            </dl>

            <div className="mt-6 space-y-2">
              <label htmlFor="cart-coupon" className="block text-xs font-medium uppercase tracking-wide text-slate-500">
                Coupon
              </label>
              <div className="flex gap-2">
                <input
                  id="cart-coupon"
                  value={couponInput}
                  onChange={(e) => setCouponInput(e.target.value)}
                  placeholder="Code"
                  className="min-w-0 flex-1 rounded border border-slate-300 px-3 py-2 text-sm"
                />
                <button
                  type="button"
                  onClick={() => handleApplyCoupon()}
                  className="rounded bg-slate-800 px-3 py-2 text-sm font-medium text-white hover:bg-slate-900"
                >
                  Apply
                </button>
              </div>
            </div>

            <Link
              to="/checkout"
              state={{ cartSubTotal: subTotal, couponCode: appliedCode, discountAmount: appliedDiscount }}
              className="mt-6 flex w-full justify-center rounded-lg bg-blue-600 py-3 text-sm font-semibold text-white hover:bg-blue-700"
            >
              Proceed to checkout
            </Link>
          </aside>
        </div>
      )}
    </div>
  )
}
