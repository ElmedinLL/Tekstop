const base = 'inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold'

function badgeClasses(status: string) {
  const s = status.trim().toLowerCase()
  switch (s) {
    case 'pending':
    case 'pendingpayment':
      return 'bg-amber-100 text-amber-800'
    case 'confirmed':
    case 'paid':
      return 'bg-blue-100 text-blue-800'
    case 'shipped':
      return 'bg-purple-100 text-purple-800'
    case 'delivered':
      return 'bg-emerald-100 text-emerald-800'
    case 'cancelled':
      return 'bg-red-100 text-red-800'
    case 'processing':
      return 'bg-amber-100 text-amber-800'
    default:
      return 'bg-slate-100 text-slate-700'
  }
}

type OrderStatusBadgeProps = {
  status: string
  className?: string
}

export function OrderStatusBadge({ status, className = '' }: OrderStatusBadgeProps) {
  return (
    <span className={`${base} ${badgeClasses(status)} ${className}`.trim()}>{status}</span>
  )
}
