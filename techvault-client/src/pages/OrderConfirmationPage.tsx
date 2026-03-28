import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Seo } from '../components/Seo'
import { fetchOrder } from '../lib/orders'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
}

export function OrderConfirmationPage() {
  const { orderId } = useParams<{ orderId: string }>()
  const id = orderId != null ? Number.parseInt(orderId, 10) : Number.NaN
  const valid = Number.isFinite(id) && id > 0

  const { data: order, isPending, isError } = useQuery({
    queryKey: ['order', id],
    queryFn: () => fetchOrder(id),
    enabled: valid,
  })

  if (!valid) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <Seo title="Order confirmation" description="Invalid order confirmation link." noindex />
        <p className="text-slate-600">Invalid order.</p>
        <Link to="/" className="mt-4 inline-block text-blue-600 hover:underline">
          Home
        </Link>
      </div>
    )
  }

  if (isPending) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center text-slate-600">
        <Seo title="Order confirmation" description="Loading your order confirmation…" noindex />
        Loading order…
      </div>
    )
  }

  if (isError || !order) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <Seo title="Order confirmation" description="Your order confirmation could not be loaded." noindex />
        <p className="text-slate-600">We could not load this order.</p>
        <Link to="/" className="mt-4 inline-block text-blue-600 hover:underline">
          Home
        </Link>
      </div>
    )
  }

  const delivery =
    order.estimatedDeliveryUtc != null
      ? formatDate(order.estimatedDeliveryUtc)
      : formatDate(new Date(new Date(order.placedAtUtc).getTime() + 5 * 24 * 60 * 60 * 1000).toISOString())

  return (
    <div className="mx-auto max-w-3xl px-4 py-12">
      <Seo
        title={`Order ${order.orderNumber} confirmed`}
        description="Your TechVault purchase is confirmed. Review your order summary and estimated delivery."
        noindex
      />
      <div className="rounded-xl border border-emerald-200 bg-emerald-50/80 px-6 py-8 text-center">
        <p className="text-sm font-semibold uppercase tracking-wide text-emerald-800">Thank you</p>
        <h1 className="mt-2 text-2xl font-semibold text-slate-900">Your order is confirmed</h1>
        <p className="mt-2 text-lg text-slate-700">
          Order <span className="font-mono font-semibold">{order.orderNumber}</span>
        </p>
        <p className="mt-4 text-sm text-slate-600">
          Estimated delivery: <span className="font-medium text-slate-900">{delivery}</span>
        </p>
      </div>

      <section className="mt-10 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-900">Order summary</h2>
        <ul className="mt-4 divide-y divide-slate-100">
          {order.items.map((item) => (
            <li key={`${item.productId}-${item.productSku}`} className="flex justify-between py-3 text-sm">
              <span className="text-slate-800">
                {item.productName} × {item.quantity}
              </span>
              <span className="font-medium text-slate-900">{formatMoney(item.lineTotal)}</span>
            </li>
          ))}
        </ul>
        <dl className="mt-4 space-y-2 border-t border-slate-200 pt-4 text-sm text-slate-600">
          <div className="flex justify-between">
            <dt>Subtotal</dt>
            <dd>{formatMoney(order.subTotal)}</dd>
          </div>
          <div className="flex justify-between">
            <dt>Shipping</dt>
            <dd>{formatMoney(order.shippingAmount)}</dd>
          </div>
          {order.discountAmount > 0 && (
            <div className="flex justify-between text-emerald-700">
              <dt>Discount</dt>
              <dd>-{formatMoney(order.discountAmount)}</dd>
            </div>
          )}
          <div className="flex justify-between text-base font-semibold text-slate-900">
            <dt>Total</dt>
            <dd>{formatMoney(order.total)}</dd>
          </div>
        </dl>
      </section>

      <div className="mt-8 text-center">
        <Link to="/" className="text-blue-600 hover:underline">
          Continue shopping
        </Link>
      </div>
    </div>
  )
}
