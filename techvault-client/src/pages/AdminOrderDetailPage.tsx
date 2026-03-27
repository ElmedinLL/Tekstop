import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { OrderTrackingTimeline } from '../components/OrderTrackingTimeline'
import { toast } from '../lib/notifications'
import { adminOrderDetailToTimelineSource } from '../lib/orderTimeline'
import { fetchAdminOrder, updateAdminOrderStatus } from '../lib/adminOrders'

const ADMIN_STATUS_OPTIONS = [
  'Pending',
  'Confirmed',
  'Processing',
  'Paid',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Refunded',
] as const

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

function parseAxiosMessage(e: unknown) {
  if (!axios.isAxiosError(e)) return null
  const d = e.response?.data
  if (typeof d === 'string') return d
  if (d && typeof d === 'object' && ('detail' in d || 'title' in d)) {
    return String((d as { detail?: string; title?: string }).detail ?? (d as { title?: string }).title)
  }
  return null
}

export function AdminOrderDetailPage() {
  const { orderId: orderIdParam } = useParams<{ orderId: string }>()
  const orderId = orderIdParam ? Number.parseInt(orderIdParam, 10) : NaN
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [selectedStatus, setSelectedStatus] = useState('')
  const [statusModalOpen, setStatusModalOpen] = useState(false)
  const [trackingUrlDraft, setTrackingUrlDraft] = useState('')

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['adminOrder', orderId],
    queryFn: () => fetchAdminOrder(orderId),
    enabled: Number.isFinite(orderId),
  })

  useEffect(() => {
    if (data?.status) {
      setSelectedStatus(data.status)
    }
  }, [data?.status, data?.id])

  const statusOptions = useMemo(() => {
    const base: string[] = [...ADMIN_STATUS_OPTIONS]
    if (data?.status && !base.includes(data.status)) {
      base.push(data.status)
    }
    return base
  }, [data?.status])

  const statusMutation = useMutation({
    mutationFn: async (payload: { status: string; trackingUrl?: string | null }) => {
      await updateAdminOrderStatus(orderId, payload)
    },
    onSuccess: async () => {
      toast.success('Order status updated.')
      setStatusModalOpen(false)
      setTrackingUrlDraft('')
      await queryClient.invalidateQueries({ queryKey: ['adminOrder', orderId] })
      await queryClient.invalidateQueries({ queryKey: ['adminOrders'] })
    },
    onError: (e: unknown) => {
      toast.error(parseAxiosMessage(e) ?? (e instanceof Error ? e.message : 'Could not update status.'))
    },
  })

  const effectiveSelected = data ? (selectedStatus || data.status) : ''
  const pendingStatusChange =
    data && effectiveSelected !== data.status ? effectiveSelected : null
  const confirmTargetStatus = pendingStatusChange ?? effectiveSelected

  function openStatusModal() {
    if (!data || selectedStatus === data.status) return
    setTrackingUrlDraft(data.trackingUrl?.trim() ?? '')
    setStatusModalOpen(true)
  }

  function closeStatusModal() {
    setStatusModalOpen(false)
    if (data) setSelectedStatus(data.status)
    setTrackingUrlDraft('')
  }

  function confirmStatusChange() {
    if (!data || !pendingStatusChange) return
    if (pendingStatusChange === 'Shipped') {
      const url = trackingUrlDraft.trim()
      if (!url) {
        toast.error('Tracking URL is required when marking as shipped.')
        return
      }
      statusMutation.mutate({ status: 'Shipped', trackingUrl: url })
      return
    }
    statusMutation.mutate({ status: pendingStatusChange })
  }

  if (!Number.isFinite(orderId)) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <p className="text-red-600">Invalid order id.</p>
        <Link className="mt-4 inline-block text-blue-600 hover:underline" to="/admin/orders">
          Back to orders
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
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
            <p className="mt-1 text-sm text-slate-600">Placed {formatUtc(data.placedAtUtc)}</p>
          </header>

          <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Update status</h2>
            <p className="mt-1 text-xs text-slate-600">Choose a new status and confirm. Shipped requires a tracking URL.</p>
            <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-end">
              <label className="block min-w-[12rem] flex-1">
                <span className="text-xs font-medium text-slate-700">Status</span>
                <select
                  value={effectiveSelected}
                  onChange={(e) => setSelectedStatus(e.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm"
                >
                  {statusOptions.map((s) => (
                    <option key={s} value={s}>
                      {s}
                    </option>
                  ))}
                </select>
              </label>
              <button
                type="button"
                disabled={!pendingStatusChange || statusMutation.isPending}
                onClick={openStatusModal}
                className="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Review change
              </button>
            </div>
          </section>

          <OrderTrackingTimeline order={adminOrderDetailToTimelineSource(data)} title="Status timeline" />

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
              {data.shippingLine2 ? (
                <>
                  <br />
                  {data.shippingLine2}
                </>
              ) : null}
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
        </div>
      )}

      {statusModalOpen && data && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4"
          role="presentation"
          onClick={closeStatusModal}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="admin-order-status-dialog-title"
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="admin-order-status-dialog-title" className="text-lg font-semibold text-slate-900">
              Confirm status change
            </h2>
            <p className="mt-2 text-sm text-slate-600">
              Change order <span className="font-medium text-slate-900">{data.orderNumber}</span> from{' '}
              <span className="font-medium text-slate-900">{data.status}</span> to{' '}
              <span className="font-medium text-slate-900">{confirmTargetStatus}</span>?
            </p>
            {confirmTargetStatus === 'Shipped' && (
              <div className="mt-4">
                <label htmlFor="admin-ship-tracking" className="text-xs font-medium text-slate-700">
                  Tracking URL (https)
                </label>
                <input
                  id="admin-ship-tracking"
                  type="url"
                  value={trackingUrlDraft}
                  onChange={(e) => setTrackingUrlDraft(e.target.value)}
                  placeholder="https://..."
                  className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm"
                  autoComplete="off"
                />
                <p className="mt-1 text-xs text-slate-500">The customer receives an email with this link.</p>
              </div>
            )}
            <div className="mt-6 flex flex-wrap justify-end gap-2">
              <button
                type="button"
                onClick={closeStatusModal}
                className="rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-sm hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={statusMutation.isPending}
                onClick={() => void confirmStatusChange()}
                className="rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {statusMutation.isPending ? 'Updating…' : 'Confirm'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
