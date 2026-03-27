import {
  buildOrderTimeline,
  formatOrderTimelineDate,
  getOrderTimelineStepState,
  type OrderTimelineEntry,
  type OrderTimelineSource,
  type OrderTimelineStepState,
} from '../lib/orderTimeline'

type IconProps = { className?: string }

function IconPlaced({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"
      />
    </svg>
  )
}

function IconPayment({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <rect x="2" y="5" width="20" height="14" rx="2" />
      <path strokeLinecap="round" d="M2 10h20" />
    </svg>
  )
}

function IconProcessing({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
      />
    </svg>
  )
}

function IconShipped({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path strokeLinecap="round" strokeLinejoin="round" d="M1 3h15v13H1V3z" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M16 8h4l3 3v5h-7V8z" />
      <circle cx="5.5" cy="18.5" r="2.5" fill="none" />
      <circle cx="18.5" cy="18.5" r="2.5" fill="none" />
    </svg>
  )
}

function IconDelivered({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path strokeLinecap="round" strokeLinejoin="round" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
    </svg>
  )
}

function IconCancelled({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path strokeLinecap="round" strokeLinejoin="round" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  )
}

function IconRefunded({ className }: IconProps) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden>
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6"
      />
    </svg>
  )
}

function TimelineStepIcon({ entryId, state }: { entryId: string; state: OrderTimelineStepState }) {
  const iconClass = 'h-5 w-5'
  const muted = state === 'upcoming'

  switch (entryId) {
    case 'placed':
      return <IconPlaced className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'payment':
      return <IconPayment className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'processing':
      return <IconProcessing className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'shipped':
      return <IconShipped className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'delivered':
      return <IconDelivered className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'cancelled':
      return <IconCancelled className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    case 'refunded':
      return <IconRefunded className={`${iconClass} ${muted ? 'opacity-70' : ''}`} />
    default:
      return (
        <svg className={`${iconClass} ${muted ? 'opacity-70' : ''}`} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
          <circle cx="12" cy="12" r="3" />
        </svg>
      )
  }
}

function stepNodeClasses(entryId: string, state: OrderTimelineStepState) {
  if (entryId === 'cancelled') {
    if (state === 'complete') return 'bg-rose-500 text-white shadow-sm'
    if (state === 'current') return 'bg-rose-600 text-white shadow-md ring-4 ring-rose-100'
    return 'bg-rose-50 text-rose-300 ring-2 ring-rose-100'
  }

  if (entryId === 'refunded') {
    return state === 'complete'
      ? 'bg-violet-600 text-white shadow-sm'
      : 'bg-violet-50 text-violet-300 ring-2 ring-violet-100'
  }

  switch (state) {
    case 'complete':
      return 'bg-emerald-500 text-white shadow-sm'
    case 'current':
      return 'bg-blue-600 text-white shadow-md ring-4 ring-blue-100'
    default:
      return 'bg-slate-100 text-slate-400 ring-2 ring-slate-200/80'
  }
}

type OrderTrackingTimelineProps = {
  order: OrderTimelineSource
  title?: string
  className?: string
}

export function OrderTrackingTimeline({ order, title = 'Order tracking', className = '' }: OrderTrackingTimelineProps) {
  const entries = buildOrderTimeline(order)

  return (
    <section className={`rounded-xl border border-slate-200 bg-white p-5 shadow-sm ${className}`.trim()}>
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">{title}</h2>
      <ol className="mt-5" aria-label="Order status history">
        {entries.map((entry, index) => (
          <TimelineRow
            key={entry.id}
            entry={entry}
            state={getOrderTimelineStepState(entry, index, entries, order)}
            isLast={index === entries.length - 1}
          />
        ))}
      </ol>
    </section>
  )
}

function TimelineRow({
  entry,
  state,
  isLast,
}: {
  entry: OrderTimelineEntry
  state: OrderTimelineStepState
  isLast: boolean
}) {
  const dateLabel = entry.date ? formatOrderTimelineDate(entry.date) : null

  return (
    <li className="relative flex gap-4">
      {!isLast && (
        <span
          className="absolute left-5 top-11 bottom-0 w-px -translate-x-1/2 bg-slate-200"
          aria-hidden
        />
      )}
      <div
        className={`relative z-10 flex h-10 w-10 shrink-0 items-center justify-center rounded-full ${stepNodeClasses(entry.id, state)}`}
        aria-hidden
      >
        <TimelineStepIcon entryId={entry.id} state={state} />
      </div>
      <div className={`min-w-0 flex-1 pt-0.5 ${isLast ? 'pb-0' : 'pb-10'}`}>
        <p className="text-sm font-semibold text-slate-900">{entry.title}</p>
        {dateLabel && <p className="mt-1 text-sm text-slate-600">{dateLabel}</p>}
        {!dateLabel && state === 'current' && (
          <p className="mt-1 text-xs font-medium uppercase tracking-wide text-blue-600">In progress</p>
        )}
        {entry.detail && <p className="mt-1 text-sm text-slate-500">{entry.detail}</p>}
        {entry.link && (
          <a
            href={entry.link}
            target="_blank"
            rel="noopener noreferrer"
            className="mt-2 inline-flex items-center gap-1 text-sm font-medium text-blue-600 hover:underline"
          >
            {entry.linkLabel ?? 'Open link'}
            <span aria-hidden className="text-xs">
              ↗
            </span>
          </a>
        )}
      </div>
    </li>
  )
}
