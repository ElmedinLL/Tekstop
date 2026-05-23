import type { ReactNode } from 'react'

export type TimelineItem = {
  id: string
  icon?: ReactNode
  title: string
  description?: string
  timestampIso: string
}

function formatTime(iso: string) {
  try {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}

/**
 * P217: Vertical admin activity timeline with staggered fade-in (CSS delays).
 */
export function TimelineComponent({ items }: { items: readonly TimelineItem[] }) {
  if (items.length === 0) {
    return <p className="text-sm text-slate-500 dark:text-slate-400">No activity yet.</p>
  }

  return (
    <ol className="relative border-s border-slate-200 ms-4 space-y-6 py-2 dark:border-slate-700" aria-label="Activity timeline">
      {items.map((item, index) => (
        <li
          key={item.id}
          className="timeline-stagger fade-up-item ms-6"
          style={{ animationDelay: `${Math.min(index, 16) * 45}ms` }}
        >
          <span className="absolute -start-3 mt-1.5 flex h-6 w-6 items-center justify-center rounded-full bg-white ring-8 ring-white dark:bg-slate-950 dark:ring-slate-900">
            {item.icon ?? (
              <span className="h-2 w-2 rounded-full bg-blue-600" aria-hidden />
            )}
          </span>
          <div className="rounded-xl border border-slate-100 bg-white/90 p-4 shadow-sm dark:border-slate-800 dark:bg-slate-950/70">
            <div className="flex flex-wrap items-baseline justify-between gap-2">
              <p className="font-semibold text-slate-900 dark:text-slate-100">{item.title}</p>
              <time className="text-xs tabular-nums text-slate-500 dark:text-slate-400" dateTime={item.timestampIso}>
                {formatTime(item.timestampIso)}
              </time>
            </div>
            {item.description ? (
              <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{item.description}</p>
            ) : null}
          </div>
        </li>
      ))}
    </ol>
  )
}
