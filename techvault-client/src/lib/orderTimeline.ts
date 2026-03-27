import type { OrderDto } from '../types/order'

export type OrderTimelineEntry = {
  id: string
  title: string
  date?: string | null
  detail?: string
  link?: string | null
  linkLabel?: string
}

export type OrderTimelineStepState = 'complete' | 'current' | 'upcoming'

export function formatOrderTimelineDate(iso: string | null | undefined) {
  if (!iso) return null
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

export function buildOrderTimeline(order: OrderDto): OrderTimelineEntry[] {
  if (order.status === 'Cancelled') {
    return [
      { id: 'placed', title: 'Order placed', date: order.placedAtUtc },
      {
        id: 'cancelled',
        title: 'Order cancelled',
        date: order.cancelledAtUtc,
        detail: 'This order will not be charged or shipped.',
      },
    ]
  }

  const paymentDate = order.paidAtUtc ?? order.confirmedAtUtc
  const isAwaitingPayment = order.status === 'PendingPayment'

  const entries: OrderTimelineEntry[] = [{ id: 'placed', title: 'Order placed', date: order.placedAtUtc }]

  entries.push({
    id: 'payment',
    title: isAwaitingPayment ? 'Awaiting payment' : 'Payment',
    date: paymentDate,
    detail: isAwaitingPayment ? 'Complete payment to confirm your order.' : undefined,
  })

  entries.push({
    id: 'processing',
    title: 'Processing',
    date: order.processingAtUtc,
    detail: 'We are preparing your items for shipment.',
  })

  entries.push({
    id: 'shipped',
    title: 'Shipped',
    date: order.shippedAtUtc,
    link: order.trackingUrl,
    linkLabel: 'Tracking link',
  })

  entries.push({
    id: 'delivered',
    title: 'Delivered',
    date: order.deliveredAtUtc,
    detail:
      !order.deliveredAtUtc && order.estimatedDeliveryUtc
        ? `Estimated delivery by ${formatOrderTimelineDate(order.estimatedDeliveryUtc) ?? ''}`.trim()
        : undefined,
  })

  if (order.status === 'Refunded') {
    entries.push({
      id: 'refunded',
      title: 'Refunded',
      detail: 'Payment was returned to your original payment method.',
    })
  }

  return entries
}

export function getOrderTimelineStepState(
  entry: OrderTimelineEntry,
  index: number,
  entries: OrderTimelineEntry[],
  order: OrderDto,
): OrderTimelineStepState {
  if (order.status === 'Cancelled') {
    if (entry.id === 'cancelled') return entry.date ? 'complete' : 'current'
    return entry.date ? 'complete' : 'upcoming'
  }

  if (entry.id === 'refunded') return 'complete'

  if (entry.date) return 'complete'

  const firstPendingIdx = entries.findIndex((e) => !e.date && e.id !== 'refunded')
  if (firstPendingIdx === index) return 'current'
  return 'upcoming'
}
