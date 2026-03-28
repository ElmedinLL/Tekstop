import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { OrderStatusBadge } from '../components/OrderStatusBadge'
import { Seo } from '../components/Seo'
import { fetchOrderList, type OrderSummary } from '../lib/orders'

const statusOrder = ['All', 'Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] as const

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function lineSummary(order: OrderSummary) {
  const n = order.lineItemCount
  return `${n} line${n === 1 ? '' : 's'}`
}

export function OrdersPage() {
  const [activeStatus, setActiveStatus] = useState<(typeof statusOrder)[number]>('All')
  const { data, isPending, isError } = useQuery({
    queryKey: ['orders', 'me', 1],
    queryFn: () => fetchOrderList(1),
  })

  const orders = data?.items ?? []
  const availableStatuses = useMemo(() => {
    const found = new Set(orders.map((order) => order.status))
    const fixed = statusOrder.filter((status) => status === 'All' || found.has(status))
    return fixed.length > 1 ? fixed : statusOrder
  }, [orders])

  const filteredOrders = useMemo(
    () => (activeStatus === 'All' ? orders : orders.filter((order) => order.status === activeStatus)),
    [orders, activeStatus],
  )

  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <Seo title="My orders" description="View and track your TechVault orders." noindex />
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">My Orders</h1>
      <p className="mt-2 text-slate-600">Track order status and open any order to see full details.</p>

      <div className="mt-6 flex flex-wrap gap-2" role="tablist" aria-label="Filter orders by status">
        {availableStatuses.map((status) => {
          const isActive = status === activeStatus
          return (
            <button
              key={status}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => setActiveStatus(status)}
              className={`rounded-full border px-3 py-1.5 text-sm font-medium transition ${
                isActive
                  ? 'border-blue-600 bg-blue-600 text-white'
                  : 'border-slate-300 bg-white text-slate-700 hover:bg-slate-50'
              }`}
            >
              {status}
            </button>
          )
        })}
      </div>

      <div className="mt-6 space-y-3">
        {isPending && <p className="text-sm text-slate-500">Loading orders…</p>}

        {isError && (
          <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">
            Could not load orders.
          </div>
        )}

        {!isPending &&
          !isError &&
          filteredOrders.length === 0 && (
            <div className="rounded-lg border border-slate-200 bg-white p-5 text-sm text-slate-600">
              No orders found for this status.
            </div>
          )}

        {!isPending &&
          !isError &&
          filteredOrders.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="block rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition hover:border-blue-300 hover:shadow"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="min-w-0">
                  <p className="text-sm font-semibold text-slate-900">{order.orderNumber}</p>
                  <p className="mt-1 text-sm text-slate-500">{formatDate(order.placedAtUtc)}</p>
                </div>
                <OrderStatusBadge status={order.status} />
              </div>

              <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-sm">
                <p className="min-w-0 truncate text-slate-600">{lineSummary(order)}</p>
                <p className="font-semibold text-slate-900">{formatCurrency(order.total)}</p>
              </div>
            </Link>
          ))}
      </div>
    </div>
  )
}
