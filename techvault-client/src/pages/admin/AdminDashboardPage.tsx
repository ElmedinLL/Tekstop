import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { OrderStatusBadge } from '../../components/OrderStatusBadge'
import { fetchAdminLowStockProducts, fetchAdminOrders, fetchAdminStats } from '../../lib/admin'

function formatCurrency(amount: number, currency = 'USD') {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function formatDateTime(iso: string) {
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function monthLabel(year: number, month: number) {
  return new Date(year, month - 1, 1).toLocaleDateString('en-US', { month: 'short', year: '2-digit' })
}

type StatCardProps = {
  title: string
  value: string
  hint?: string
}

function StatCard({ title, value, hint }: StatCardProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{title}</p>
      <p className="mt-2 text-2xl font-semibold tracking-tight text-slate-900">{value}</p>
      {hint ? <p className="mt-1 text-xs text-slate-500">{hint}</p> : null}
    </div>
  )
}

export function AdminDashboardPage() {
  const statsQuery = useQuery({
    queryKey: ['admin', 'stats'],
    queryFn: fetchAdminStats,
  })

  const ordersQuery = useQuery({
    queryKey: ['admin', 'orders', 'dashboard'],
    queryFn: () => fetchAdminOrders(1, 8),
  })

  const lowStockQuery = useQuery({
    queryKey: ['admin', 'low-stock'],
    queryFn: fetchAdminLowStockProducts,
  })

  const chartData = useMemo(() => {
    const rows = statsQuery.data?.revenueByMonth ?? []
    return rows.map((m) => ({
      key: `${m.year}-${m.month}`,
      label: monthLabel(m.year, m.month),
      revenue: Number(m.revenue),
    }))
  }, [statsQuery.data?.revenueByMonth])

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Dashboard</h1>
        <p className="mt-1 text-sm text-slate-600">Overview of revenue, orders, and inventory.</p>
      </div>

      <section aria-label="Summary statistics">
        {statsQuery.isPending && (
          <div className="grid gap-4 sm:grid-cols-3">
            {[0, 1, 2].map((i) => (
              <div
                key={i}
                className="h-28 animate-pulse rounded-xl border border-slate-200 bg-slate-100"
              />
            ))}
          </div>
        )}
        {statsQuery.isError && (
          <p className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
            Could not load dashboard statistics.
          </p>
        )}
        {statsQuery.data && (
          <div className="grid gap-4 sm:grid-cols-3">
            <StatCard title="Revenue" value={formatCurrency(statsQuery.data.totalRevenue)} hint="Excludes cancelled & refunded" />
            <StatCard title="Orders" value={statsQuery.data.totalOrders.toLocaleString()} />
            <StatCard title="Users" value={statsQuery.data.totalUsers.toLocaleString()} hint="Registered accounts" />
          </div>
        )}
      </section>

      <section
        className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
        aria-label="Revenue by month"
      >
        <div className="flex flex-wrap items-end justify-between gap-2">
          <div>
            <h2 className="text-sm font-semibold text-slate-900">Revenue trend</h2>
            <p className="mt-0.5 text-xs text-slate-500">Last 12 months (UTC), same rules as total revenue</p>
          </div>
        </div>
        {statsQuery.isPending && <div className="mt-6 h-[280px] animate-pulse rounded-lg bg-slate-100" />}
        {statsQuery.isError && (
          <p className="mt-6 text-sm text-rose-600">Chart unavailable — stats failed to load.</p>
        )}
        {statsQuery.data && (
          <div className="mt-6 h-[280px] w-full min-w-0">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="label" tick={{ fontSize: 11, fill: '#64748b' }} axisLine={{ stroke: '#cbd5e1' }} />
                <YAxis
                  tick={{ fontSize: 11, fill: '#64748b' }}
                  axisLine={{ stroke: '#cbd5e1' }}
                  tickFormatter={(v) =>
                    new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(
                      Number(v),
                    )
                  }
                  width={56}
                />
                <Tooltip
                  contentStyle={{
                    borderRadius: '0.5rem',
                    border: '1px solid #e2e8f0',
                    fontSize: '0.875rem',
                  }}
                  formatter={(value) => [formatCurrency(Number(value ?? 0)), 'Revenue']}
                />
                <Line
                  type="monotone"
                  dataKey="revenue"
                  stroke="#2563eb"
                  strokeWidth={2}
                  dot={{ r: 3, fill: '#2563eb', strokeWidth: 0 }}
                  activeDot={{ r: 5 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}
      </section>

      <div className="grid gap-8 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-200 bg-white shadow-sm" aria-label="Recent orders">
          <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
            <h2 className="text-sm font-semibold text-slate-900">Recent orders</h2>
            <Link to="/admin/orders" className="text-xs font-medium text-blue-600 hover:underline">
              View all
            </Link>
          </div>
          {ordersQuery.isPending && (
            <div className="space-y-2 p-5">
              {[0, 1, 2, 3].map((i) => (
                <div key={i} className="h-10 animate-pulse rounded bg-slate-100" />
              ))}
            </div>
          )}
          {ordersQuery.isError && (
            <p className="p-5 text-sm text-rose-600">Could not load orders.</p>
          )}
          {ordersQuery.data && ordersQuery.data.items.length === 0 && (
            <p className="p-5 text-sm text-slate-600">No orders yet.</p>
          )}
          {ordersQuery.data && ordersQuery.data.items.length > 0 && (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[520px] text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-100 text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-5 py-3">Order</th>
                    <th className="px-3 py-3">Customer</th>
                    <th className="px-3 py-3">Status</th>
                    <th className="px-3 py-3 text-right">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {ordersQuery.data.items.map((o) => (
                    <tr key={o.id} className="text-slate-800">
                      <td className="whitespace-nowrap px-5 py-3">
                        <span className="font-medium text-slate-900">{o.orderNumber}</span>
                        <p className="text-xs text-slate-500">{formatDateTime(o.placedAtUtc)}</p>
                      </td>
                      <td className="max-w-[140px] truncate px-3 py-3 text-slate-600" title={o.customerEmail ?? ''}>
                        {o.customerEmail ?? '—'}
                      </td>
                      <td className="px-3 py-3">
                        <OrderStatusBadge status={o.status} />
                      </td>
                      <td className="whitespace-nowrap px-3 py-3 text-right font-medium tabular-nums">
                        {formatCurrency(o.total, o.currency)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="rounded-xl border border-slate-200 bg-white shadow-sm" aria-label="Low stock alerts">
          <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
            <div>
              <h2 className="text-sm font-semibold text-slate-900">Low stock</h2>
              <p className="mt-0.5 text-xs text-slate-500">Below configured threshold</p>
            </div>
            <Link to="/admin/products" className="text-xs font-medium text-blue-600 hover:underline">
              Products
            </Link>
          </div>
          {lowStockQuery.isPending && (
            <div className="space-y-2 p-5">
              {[0, 1, 2].map((i) => (
                <div key={i} className="h-12 animate-pulse rounded-lg bg-slate-100" />
              ))}
            </div>
          )}
          {lowStockQuery.isError && (
            <p className="p-5 text-sm text-rose-600">Could not load low-stock products.</p>
          )}
          {lowStockQuery.data && lowStockQuery.data.length === 0 && (
            <p className="p-5 text-sm text-slate-600">No low-stock items. You’re in good shape.</p>
          )}
          {lowStockQuery.data && lowStockQuery.data.length > 0 && (
            <ul className="divide-y divide-slate-100 p-2">
              {lowStockQuery.data.slice(0, 10).map((p) => (
                <li
                  key={p.id}
                  className="flex items-start gap-3 rounded-lg px-3 py-3 hover:bg-amber-50/80"
                >
                  <div
                    className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-amber-100 text-xs font-bold text-amber-900"
                    aria-hidden
                  >
                    {p.stockQuantity}
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium text-slate-900">{p.name}</p>
                    <p className="text-xs text-slate-500">
                      {p.sku} · {p.categoryName}
                      {!p.isPublished && (
                        <span className="ml-1.5 rounded bg-slate-200 px-1.5 py-0.5 text-slate-700">Unpublished</span>
                      )}
                    </p>
                  </div>
                </li>
              ))}
            </ul>
          )}
          {lowStockQuery.data && lowStockQuery.data.length > 10 && (
            <p className="border-t border-slate-100 px-5 py-3 text-center text-xs text-slate-500">
              +{lowStockQuery.data.length - 10} more — manage in Products
            </p>
          )}
        </section>
      </div>
    </div>
  )
}
