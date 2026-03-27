import { Link, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { OrderStatusBadge } from '../components/OrderStatusBadge'
import { OrderTrackingTimeline } from '../components/OrderTrackingTimeline'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { cancelOrder, fetchOrder } from '../lib/orders'
import type { OrderDto } from '../types/order'

function formatCurrency(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function formatDate(iso: string | null | undefined) {
  if (!iso) return null
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function formatPaymentMethod(raw: string | null | undefined) {
  const t = raw?.trim()
  if (!t) return 'Not specified'
  const lower = t.toLowerCase()
  if (lower === 'card') return 'Credit or debit card'
  return t.charAt(0).toUpperCase() + t.slice(1)
}

function canCancelOrder(status: string) {
  return !['Shipped', 'Delivered', 'Cancelled', 'Refunded'].includes(status)
}

function formatAddress(order: OrderDto) {
  const name = order.shippingFullName?.trim()
  const line1 = order.shippingLine1?.trim()
  if (!name && !line1) {
    return null
  }

  const line2 = order.shippingLine2?.trim()
  const city = order.shippingCity?.trim()
  const region = order.shippingRegion?.trim()
  const postal = order.shippingPostalCode?.trim()
  const country = order.shippingCountry?.trim()
  const phone = order.shippingPhone?.trim()

  const cityLine = [city, region, postal].filter(Boolean).join(', ')
  const lines = [name, line1, line2, cityLine, country].filter(Boolean) as string[]
  return { lines, phone }
}

export function OrderDetailPage() {
  const params = useParams<{ orderId: string }>()
  const orderId = Number(params.orderId)
  const queryClient = useQueryClient()

  const orderQuery = useQuery({
    queryKey: ['order', orderId],
    enabled: Number.isFinite(orderId) && orderId > 0,
    queryFn: () => fetchOrder(orderId),
  })

  const cancelMutation = useMutation({
    mutationFn: () => cancelOrder(orderId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['order', orderId] })
      void queryClient.invalidateQueries({ queryKey: ['orders', 'me'] })
    },
  })

  if (!Number.isFinite(orderId) || orderId <= 0) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Order</h1>
        <p className="mt-2 text-slate-600">Invalid order id.</p>
        <Link className="mt-5 inline-block text-sm font-medium text-blue-600 hover:underline" to="/orders">
          Back to orders
        </Link>
      </div>
    )
  }

  const { data: order, isPending, isError, error, refetch } = orderQuery

  const notFound =
    isAxiosError(error) && (error.response?.status === 404 || error.response?.status === 403)

  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <Link className="text-sm font-medium text-blue-600 hover:underline" to="/orders">
        ← Back to orders
      </Link>

      {isPending && <p className="mt-6 text-sm text-slate-500">Loading order…</p>}

      {isError && !isPending && (
        <div className="mt-6 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          {notFound ? (
            <p>We could not find that order, or you do not have access.</p>
          ) : (
            <p>{isAxiosError(error) ? error.message : 'Something went wrong loading this order.'}</p>
          )}
          <button
            type="button"
            className="mt-2 text-sm font-medium text-rose-900 underline"
            onClick={() => void refetch()}
          >
            Try again
          </button>
        </div>
      )}

      {!isPending && !isError && order && (
        <div className="mt-6 space-y-8">
          <header className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <h1 className="text-2xl font-semibold tracking-tight text-slate-900">
                Order {order.orderNumber}
              </h1>
              <p className="mt-1 text-sm text-slate-500">
                Placed {formatDate(order.placedAtUtc) ?? '—'}
              </p>
            </div>
            <OrderStatusBadge status={order.status} className="px-3" />
          </header>

          <OrderTrackingTimeline order={order} title="Tracking" />

          <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Items</h2>
            <ul className="mt-4 divide-y divide-slate-100">
              {order.items.map((item) => (
                <li key={`${item.productId}-${item.productSku}`} className="flex gap-4 py-4 first:pt-0">
                  <Link
                    to={`/products/${item.productId}`}
                    className="shrink-0 rounded-lg border border-slate-100 bg-slate-50 overflow-hidden"
                  >
                    {item.imageUrl ? (
                      <img
                        src={resolveApiAssetUrl(item.imageUrl)}
                        alt=""
                        className="h-20 w-20 object-cover"
                      />
                    ) : (
                      <div className="flex h-20 w-20 items-center justify-center text-slate-300" aria-hidden>
                        <svg className="h-8 w-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={1.5}
                            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                          />
                        </svg>
                      </div>
                    )}
                  </Link>
                  <div className="min-w-0 flex-1">
                    <Link
                      to={`/products/${item.productId}`}
                      className="font-medium text-slate-900 hover:text-blue-600 hover:underline"
                    >
                      {item.productName}
                    </Link>
                    <p className="mt-0.5 text-xs text-slate-500">SKU {item.productSku}</p>
                    <p className="mt-2 text-sm text-slate-600">
                      {formatCurrency(item.unitPrice, order.currency)} × {item.quantity}
                    </p>
                  </div>
                  <p className="shrink-0 text-sm font-semibold text-slate-900">
                    {formatCurrency(item.lineTotal, order.currency)}
                  </p>
                </li>
              ))}
            </ul>
          </section>

          <div className="grid gap-6 md:grid-cols-2">
            <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Ship to</h2>
              {(() => {
                const addr = formatAddress(order)
                if (!addr) {
                  return <p className="mt-3 text-sm text-slate-500">No shipping address on file for this order.</p>
                }
                return (
                  <div className="mt-3 text-sm text-slate-700">
                    {addr.lines.map((line) => (
                      <p key={line}>{line}</p>
                    ))}
                    {addr.phone && <p className="mt-2 text-slate-600">{addr.phone}</p>}
                  </div>
                )
              })()}
            </section>

            <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Payment</h2>
              <p className="mt-3 text-sm font-medium text-slate-900">{formatPaymentMethod(order.paymentMethod)}</p>
              {order.couponCode && (
                <p className="mt-2 text-sm text-slate-600">
                  Coupon <span className="font-mono text-slate-800">{order.couponCode}</span>
                </p>
              )}
            </section>
          </div>

          <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Order summary</h2>
            <dl className="mt-4 space-y-2 text-sm">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-600">Subtotal</dt>
                <dd className="font-medium text-slate-900">{formatCurrency(order.subTotal, order.currency)}</dd>
              </div>
              {order.discountAmount > 0 && (
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-600">Discount</dt>
                  <dd className="font-medium text-emerald-700">
                    −{formatCurrency(order.discountAmount, order.currency)}
                  </dd>
                </div>
              )}
              <div className="flex justify-between gap-4">
                <dt className="text-slate-600">Shipping</dt>
                <dd className="font-medium text-slate-900">{formatCurrency(order.shippingAmount, order.currency)}</dd>
              </div>
              {order.taxAmount > 0 && (
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-600">Tax</dt>
                  <dd className="font-medium text-slate-900">{formatCurrency(order.taxAmount, order.currency)}</dd>
                </div>
              )}
              <div className="flex justify-between gap-4 border-t border-slate-200 pt-3 text-base">
                <dt className="font-semibold text-slate-900">Total</dt>
                <dd className="font-semibold text-slate-900">{formatCurrency(order.total, order.currency)}</dd>
              </div>
            </dl>
          </section>

          {canCancelOrder(order.status) && (
            <section className="rounded-xl border border-amber-200 bg-amber-50/50 p-5">
              <h2 className="text-sm font-semibold text-slate-900">Need to cancel?</h2>
              <p className="mt-1 text-sm text-slate-600">
                You can cancel this order before it ships. Stock will be returned to inventory.
              </p>
              {cancelMutation.isError && (
                <p className="mt-2 text-sm text-rose-700">
                  {isAxiosError(cancelMutation.error)
                    ? String(cancelMutation.error.response?.data ?? cancelMutation.error.message)
                    : 'Could not cancel this order.'}
                </p>
              )}
              <button
                type="button"
                className="mt-4 rounded-lg border border-rose-300 bg-white px-4 py-2 text-sm font-semibold text-rose-700 shadow-sm hover:bg-rose-50 disabled:opacity-50"
                disabled={cancelMutation.isPending}
                onClick={() => {
                  if (!window.confirm('Cancel this order? This cannot be undone.')) return
                  cancelMutation.reset()
                  cancelMutation.mutate()
                }}
              >
                {cancelMutation.isPending ? 'Cancelling…' : 'Cancel order'}
              </button>
            </section>
          )}
        </div>
      )}
    </div>
  )
}
