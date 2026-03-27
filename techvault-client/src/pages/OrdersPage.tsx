import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { OrderStatusBadge } from '../components/OrderStatusBadge'
import { api } from '../lib/api'

type OrderItem = {
  productName: string
  quantity: number
}

type OrderSummary = {
  id: number
  status: string
  orderedAtUtc: string
  totalAmount: number
  currency: string
  items: OrderItem[]
}

const mockOrders: OrderSummary[] = [
  {
    id: 41027,
    status: 'Pending',
    orderedAtUtc: '2026-03-24T14:15:00Z',
    totalAmount: 219.98,
    currency: 'USD',
    items: [
      { productName: 'NovaGraph RTX 4060', quantity: 1 },
      { productName: 'FlashForge 1TB NVMe SSD', quantity: 1 },
    ],
  },
  {
    id: 41011,
    status: 'Shipped',
    orderedAtUtc: '2026-03-20T09:45:00Z',
    totalAmount: 89.99,
    currency: 'USD',
    items: [
      { productName: 'KeyForge Mechanical Keyboard', quantity: 1 },
      { productName: 'GlideAir Wireless Mouse', quantity: 1 },
      { productName: 'CableCraft USB-C Cable', quantity: 2 },
    ],
  },
  {
    id: 40985,
    status: 'Delivered',
    orderedAtUtc: '2026-03-12T18:10:00Z',
    totalAmount: 149.0,
    currency: 'USD',
    items: [{ productName: 'RAMBurst 32GB DDR5 Kit', quantity: 1 }],
  },
]

const statusOrder = ['All', 'Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] as const

async function fetchOrders(): Promise<OrderSummary[]> {
  try {
    const { data } = await api.get<OrderSummary[]>('/orders/my')
    return data
  } catch {
    return mockOrders
  }
}

function formatCurrency(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function buildItemsPreview(items: OrderItem[]) {
  if (items.length === 0) {
    return 'No items'
  }

  const [first, second, ...rest] = items
  const firstText = `${first.quantity}x ${first.productName}`
  const secondText = second ? `, ${second.quantity}x ${second.productName}` : ''
  const restText = rest.length > 0 ? ` +${rest.length} more` : ''
  return `${firstText}${secondText}${restText}`
}

export function OrdersPage() {
  const [activeStatus, setActiveStatus] = useState<(typeof statusOrder)[number]>('All')
  const { data, isPending } = useQuery({
    queryKey: ['orders', 'me'],
    queryFn: fetchOrders,
  })

  const orders = data ?? []
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

        {!isPending && filteredOrders.length === 0 && (
          <div className="rounded-lg border border-slate-200 bg-white p-5 text-sm text-slate-600">
            No orders found for this status.
          </div>
        )}

        {!isPending &&
          filteredOrders.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="block rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition hover:border-blue-300 hover:shadow"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="min-w-0">
                  <p className="text-sm font-semibold text-slate-900">Order #{order.id}</p>
                  <p className="mt-1 text-sm text-slate-500">{formatDate(order.orderedAtUtc)}</p>
                </div>
                <OrderStatusBadge status={order.status} />
              </div>

              <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-sm">
                <p className="min-w-0 truncate text-slate-600">{buildItemsPreview(order.items)}</p>
                <p className="font-semibold text-slate-900">{formatCurrency(order.totalAmount, order.currency)}</p>
              </div>
            </Link>
          ))}
      </div>
    </div>
  )
}
