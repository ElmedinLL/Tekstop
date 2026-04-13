import { useQuery } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { Seo } from '../../components/Seo'
import { Pagination } from '../../components/Pagination'
import { fetchAdminOrders } from '../../lib/adminOrders'

const STATUS_TABS: { value: string; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'PendingPayment', label: 'Pending payment' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Confirmed', label: 'Confirmed' },
  { value: 'Processing', label: 'Processing' },
  { value: 'Paid', label: 'Paid' },
  { value: 'Shipped', label: 'Shipped' },
  { value: 'Delivered', label: 'Delivered' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'Refunded', label: 'Refunded' },
]

function statusBadgeClass(status: string) {
  const s = status.toLowerCase()
  if (s === 'delivered') return 'bg-emerald-100 text-emerald-800 ring-emerald-600/20'
  if (s === 'shipped') return 'bg-blue-100 text-blue-800 ring-blue-600/20'
  if (s === 'cancelled' || s === 'refunded') return 'bg-red-100 text-red-800 ring-red-600/20'
  if (s === 'pendingpayment') return 'bg-amber-100 text-amber-900 ring-amber-600/20'
  if (s === 'confirmed' || s === 'processing') return 'bg-slate-100 text-slate-800 ring-slate-600/15'
  if (s === 'paid') return 'bg-green-100 text-green-800 ring-green-600/20'
  return 'bg-slate-100 text-slate-700 ring-slate-600/15'
}

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

function parsePage(raw: string | null) {
  const n = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : 1
  return Number.isFinite(n) && n >= 1 ? n : 1
}

export function AdminOrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const status = searchParams.get('status') ?? 'all'
  const from = searchParams.get('from') ?? ''
  const to = searchParams.get('to') ?? ''
  const page = parsePage(searchParams.get('page'))

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['adminOrders', page, status, from, to],
    queryFn: () =>
      fetchAdminOrders({
        page,
        status,
        from: from || undefined,
        to: to || undefined,
      }),
  })

  const setFilter = (updates: Record<string, string | undefined>) => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev)
      for (const [k, v] of Object.entries(updates)) {
        if (v === undefined || v === '') {
          next.delete(k)
        } else {
          next.set(k, v)
        }
      }
      next.set('page', '1')
      return next
    })
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1

  return (
    <div className="space-y-6">
      <Seo title="Admin orders" description="Review and update customer orders by status." />
      <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Orders</h1>
          <p className="mt-1 text-sm text-slate-600">Filter by status (Confirmed, Paid, Shipped, etc.), date, and open an order to change status.</p>
        </div>
        <Link className="text-sm font-medium text-blue-600 hover:underline" to="/admin">
          Dashboard
        </Link>
      </div>

      <div className="flex flex-col gap-4">
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
            From (UTC date)
            <input
              type="date"
              value={from}
              onChange={(e) => setFilter({ from: e.target.value || undefined })}
              className="rounded-md border border-slate-300 bg-white px-2 py-1.5 text-sm text-slate-900 shadow-sm"
            />
          </label>
          <label className="flex flex-col gap-1 text-xs font-medium text-slate-600">
            To (UTC date)
            <input
              type="date"
              value={to}
              onChange={(e) => setFilter({ to: e.target.value || undefined })}
              className="rounded-md border border-slate-300 bg-white px-2 py-1.5 text-sm text-slate-900 shadow-sm"
            />
          </label>
          {(from || to) && (
            <button
              type="button"
              onClick={() => setFilter({ from: undefined, to: undefined })}
              className="self-end rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
            >
              Clear dates
            </button>
          )}
        </div>

        <div className="flex flex-wrap gap-2 border-b border-slate-200 pb-1">
          {STATUS_TABS.map((tab) => {
            const active = status === tab.value
            return (
              <button
                key={tab.value}
                type="button"
                onClick={() => setFilter({ status: tab.value === 'all' ? undefined : tab.value })}
                className={`whitespace-nowrap rounded-t-md px-3 py-2 text-sm font-medium transition-colors ${
                  active
                    ? 'border border-b-0 border-slate-200 bg-white text-blue-700'
                    : 'border border-transparent text-slate-600 hover:bg-slate-100 hover:text-slate-900'
                }`}
              >
                {tab.label}
              </button>
            )
          })}
        </div>
      </div>

      <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="min-w-full divide-y divide-slate-200 text-left text-sm">
          <thead className="bg-slate-50 text-xs font-semibold uppercase tracking-wide text-slate-600">
            <tr>
              <th className="px-4 py-3">Order</th>
              <th className="px-4 py-3">Customer</th>
              <th className="px-4 py-3">Date</th>
              <th className="px-4 py-3 text-right">Total</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3 text-right"> </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {isLoading && (
              <tr>
                <td colSpan={6} className="px-4 py-10 text-center text-slate-500">
                  Loading orders…
                </td>
              </tr>
            )}
            {isError && (
              <tr>
                <td colSpan={6} className="px-4 py-10 text-center text-red-600">
                  {error instanceof Error ? error.message : 'Failed to load orders.'}
                </td>
              </tr>
            )}
            {!isLoading && !isError && data?.items.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-10 text-center text-slate-500">
                  No orders match your filters.
                </td>
              </tr>
            )}
            {data?.items.map((row) => (
              <tr key={row.id} className="hover:bg-slate-50/80">
                <td className="whitespace-nowrap px-4 py-3 font-medium text-slate-900">
                  #{row.id}
                  <span className="ml-2 font-normal text-slate-500">{row.orderNumber}</span>
                </td>
                <td className="max-w-[14rem] px-4 py-3">
                  <div className="truncate font-medium text-slate-900">{row.customerName}</div>
                  <div className="truncate text-xs text-slate-500">{row.customerEmail || '—'}</div>
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-slate-700">{formatUtc(row.placedAtUtc)}</td>
                <td className="whitespace-nowrap px-4 py-3 text-right font-medium text-slate-900">
                  {formatMoney(row.total, row.currency)}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset ${statusBadgeClass(row.status)}`}
                  >
                    {row.status}
                  </span>
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-right">
                  <Link
                    to={`/admin/orders/${row.id}`}
                    className="inline-flex rounded-md bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-slate-800"
                  >
                    Manage
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {data && data.totalCount > 0 && (
        <p className="text-center text-xs text-slate-500">
          Showing {(data.page - 1) * data.pageSize + 1}–
          {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount}
        </p>
      )}

      <Pagination totalPages={totalPages} queryParamKey="page" />
    </div>
  )
}
