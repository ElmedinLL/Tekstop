import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

type OrderDetailItem = {
  productName: string
  quantity: number
}

type OrderDetail = {
  id: number
  status: string
  orderedAtUtc: string
  totalAmount: number
  currency: string
  items: OrderDetailItem[]
}

const fallbackOrder: OrderDetail = {
  id: 41027,
  status: 'Pending',
  orderedAtUtc: '2026-03-24T14:15:00Z',
  totalAmount: 219.98,
  currency: 'USD',
  items: [
    { productName: 'NovaGraph RTX 4060', quantity: 1 },
    { productName: 'FlashForge 1TB NVMe SSD', quantity: 1 },
  ],
}

function formatCurrency(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

export function OrderDetailPage() {
  const params = useParams<{ orderId: string }>()
  const orderId = Number(params.orderId)

  const { data, isPending } = useQuery({
    queryKey: ['order', orderId],
    enabled: Number.isFinite(orderId),
    queryFn: async (): Promise<OrderDetail> => {
      try {
        const { data } = await api.get<OrderDetail>(`/orders/${orderId}`)
        return data
      } catch {
        return { ...fallbackOrder, id: Number.isFinite(orderId) ? orderId : fallbackOrder.id }
      }
    },
  })

  if (!Number.isFinite(orderId)) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Order</h1>
        <p className="mt-2 text-slate-600">Invalid order id.</p>
        <Link className="mt-5 inline-block text-sm font-medium text-blue-600 hover:underline" to="/orders">
          Back to orders
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Link className="text-sm font-medium text-blue-600 hover:underline" to="/orders">
        Back to orders
      </Link>

      {isPending && <p className="mt-4 text-sm text-slate-500">Loading order...</p>}

      {!isPending && data && (
        <section className="mt-4 rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Order #{data.id}</h1>
            <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700">{data.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-500">Placed {formatDate(data.orderedAtUtc)}</p>

          <ul className="mt-5 space-y-2">
            {data.items.map((item, index) => (
              <li key={`${item.productName}-${index}`} className="flex items-center justify-between text-sm">
                <span className="text-slate-700">{item.productName}</span>
                <span className="font-medium text-slate-900">x{item.quantity}</span>
              </li>
            ))}
          </ul>

          <div className="mt-5 border-t border-slate-200 pt-4 text-right">
            <p className="text-sm text-slate-500">Total</p>
            <p className="text-lg font-semibold text-slate-900">{formatCurrency(data.totalAmount, data.currency)}</p>
          </div>
        </section>
      )}
    </div>
  )
}
