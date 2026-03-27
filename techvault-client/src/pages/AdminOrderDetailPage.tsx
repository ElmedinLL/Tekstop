import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { fetchAdminOrder, shipAdminOrder } from '../lib/adminOrders'

function formatUtc(iso: string) {
  const d = new Date(iso)
  return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

function formatMoney(amount: number, currency: string) {
  try {
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: currency || 'USD' }).format(amount)
  } catch {
    return `${amount.toFixed(2)} ${currency}`
  }
}

function canMarkShipped(status: string) {
  return ['Confirmed', 'Processing', 'Paid'].includes(status)
}

export function AdminOrderDetailPage() {
  const { orderId: orderIdParam } = useParams<{ orderId: string }>()
  const orderId = orderIdParam ? Number.parseInt(orderIdParam, 10) : NaN
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [trackingUrl, setTrackingUrl] = useState('')

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['adminOrder', orderId],
    queryFn: () => fetchAdminOrder(orderId),
    enabled: Number.isFinite(orderId),
  })

  const shipMutation = useMutation({
    mutationFn: () => shipAdminOrder(orderId, trackingUrl.trim()),
    onSuccess: async () => {
      toast.success('Order marked as shipped.')
      setTrackingUrl('')
      await queryClient.invalidateQueries({ queryKey: ['adminOrder', orderId] })
      await queryClient.invalidateQueries({ queryKey: ['adminOrders'] })
    },
    onError: (e: unknown) => {
      if (!axios.isAxiosError(e)) {
        toast.error('Could not update order.')
        return
      }
      const d = e.response?.data
      const text =
        typeof d === 'string'
          ? d
          : d && typeof d === 'object' && ('detail' in d || 'title' in d)
            ? String((d as { detail?: string; title?: string }).detail ?? (d as { title?: string }).title)
            : null
      toast.error(text ?? e.message ?? 'Could not update order.')
    },
  })

  if (!Number.isFinite(orderId)) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10">
        <p className="text-red-600">Invalid order id.</p>
        <Link className="mt-4 inline-block text-blue-600 hover:underline" to="/admin/orders">
          Back to orders
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="text-sm font-medium text-blue-600 hover:underline"
        >
          ← Back
        </button>
        <Link className="text-sm font-medium text-slate-600 hover:text-slate-900" to="/admin/orders">
          All orders
        </Link>
      </div>

      {isLoading && <p className="mt-8 text-slate-600">Loading order…</p>}
      {isError && axios.isAxiosError(error) && error.response?.status === 404 && (
        <p className="mt-8 text-slate-700">Order not found.</p>
      )}
      {isError && !(axios.isAxiosError(error) && error.response?.status === 404) && (
        <p className="mt-8 text-red-600">{error instanceof Error ? error.message : 'Failed to load order.'}</p>
      )}

      {data && (
        <div className="mt-6 space-y-8">
          <header>
            <h1 className="text-2xl font-semibold text-slate-900">
              Order #{data.id}{' '}
              <span className="font-normal text-slate-500">{data.orderNumber}</span>
            </h1>
            <p className="mt-1 text-sm text-slate-600">
              Placed {formatUtc(data.placedAtUtc)} · Status{' '}
              <span className="font-medium text-slate-900">{data.status}</span>
            </p>
          </header>

          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Customer</h2>
            <p className="mt-2 text-slate-900">{data.customerName ?? '—'}</p>
            <p className="text-sm text-slate-600">{data.customerEmail ?? '—'}</p>
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Shipping</h2>
            <p className="mt-2 text-slate-900">{data.shippingFullName}</p>
            <p className="text-slate-700">
              {data.shippingLine1}
              {data.shippingLine2 ? <><br />{data.shippingLine2}</> : null}
            </p>
            <p className="text-slate-700">
              {data.shippingCity}, {data.shippingRegion ?? ''} {data.shippingPostalCode}
            </p>
            <p className="text-slate-700">{data.shippingCountry}</p>
            {data.shippingPhone && <p className="mt-1 text-sm text-slate-600">{data.shippingPhone}</p>}
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Totals</h2>
            <dl className="mt-3 space-y-1 text-sm">
              <div className="flex justify-between">
                <dt className="text-slate-600">Subtotal</dt>
                <dd>{formatMoney(data.subTotal, data.currency)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-slate-600">Shipping</dt>
                <dd>{formatMoney(data.shippingAmount, data.currency)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-slate-600">Tax</dt>
                <dd>{formatMoney(data.taxAmount, data.currency)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-slate-600">Discount</dt>
                <dd>-{formatMoney(data.discountAmount, data.currency)}</dd>
              </div>
              <div className="flex justify-between border-t border-slate-100 pt-2 font-semibold">
                <dt>Total</dt>
                <dd>{formatMoney(data.total, data.currency)}</dd>
              </div>
            </dl>
            {data.paymentMethod && (
              <p className="mt-3 text-sm text-slate-600">
                Payment: <span className="text-slate-900">{data.paymentMethod}</span>
              </p>
            )}
            {data.couponCode && (
              <p className="text-sm text-slate-600">
                Coupon: <span className="text-slate-900">{data.couponCode}</span>
              </p>
            )}
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Items</h2>
            <ul className="mt-3 divide-y divide-slate-100">
              {data.items.map((line) => (
                <li key={`${line.productId}-${line.productSku}`} className="flex flex-wrap justify-between gap-2 py-3 text-sm">
                  <div>
                    <p className="font-medium text-slate-900">{line.productName}</p>
                    <p className="text-xs text-slate-500">SKU {line.productSku}</p>
                  </div>
                  <div className="text-right">
                    <p>
                      {line.quantity} × {formatMoney(line.unitPrice, data.currency)}
                    </p>
                    <p className="font-medium text-slate-900">{formatMoney(line.lineTotal, data.currency)}</p>
                  </div>
                </li>
              ))}
            </ul>
          </section>

          {(data.shippedAtUtc || data.trackingUrl) && (
            <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Shipment</h2>
              {data.shippedAtUtc && (
                <p className="mt-2 text-sm text-slate-700">Shipped {formatUtc(data.shippedAtUtc)}</p>
              )}
              {data.trackingUrl && (
                <a
                  href={data.trackingUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-2 inline-block text-sm font-medium text-blue-600 hover:underline"
                >
                  Tracking link
                </a>
              )}
            </section>
          )}

          {canMarkShipped(data.status) && (
            <section className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-4">
              <h2 className="text-sm font-semibold text-slate-900">Mark as shipped</h2>
              <p className="mt-1 text-xs text-slate-600">
                Paste the carrier or tracking page URL. The customer will receive an email with this link.
              </p>
              <div className="mt-3 flex flex-col gap-2 sm:flex-row">
                <input
                  type="url"
                  name="trackingUrl"
                  value={trackingUrl}
                  onChange={(e) => setTrackingUrl(e.target.value)}
                  placeholder="https://..."
                  className="min-w-0 flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm"
                />
                <button
                  type="button"
                  disabled={shipMutation.isPending || !trackingUrl.trim()}
                  onClick={() => void shipMutation.mutateAsync()}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {shipMutation.isPending ? 'Saving…' : 'Ship order'}
                </button>
              </div>
            </section>
          )}
        </div>
      )}
    </div>
  )
}
