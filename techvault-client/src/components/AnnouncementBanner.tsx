import { useEffect, useMemo, useState } from 'react'

export type AnnouncementType = 'info' | 'warning' | 'error'

export type SiteAnnouncement = {
  id: string
  /** optional course / scope key for LMS-style stacking (unused in TechVault; kept for API parity). */
  scope?: 'site' | 'course'
  courseId?: string
  type?: AnnouncementType
  title: string
  message?: string
}

const STORAGE_PREFIX = 'techvault_announcements_dismissed_'

function readDismissed(ids: readonly string[]): Set<string> {
  try {
    const raw = localStorage.getItem(STORAGE_PREFIX + 'ids')
    if (!raw) return new Set()
    const arr = JSON.parse(raw) as unknown
    if (!Array.isArray(arr)) return new Set()
    const next = new Set<string>()
    for (const id of ids) if (arr.includes(id)) next.add(id)
    return next
  } catch {
    return new Set()
  }
}

function writeDismissed(set: ReadonlySet<string>) {
  try {
    localStorage.setItem(STORAGE_PREFIX + 'ids', JSON.stringify([...set]))
  } catch {
    /* quota / privacy mode */
  }
}

const stylesByType: Record<AnnouncementType, string> = {
  info: 'border-blue-200 bg-blue-50 text-blue-950 dark:border-blue-800 dark:bg-blue-950/70 dark:text-blue-50',
  warning:
    'border-amber-200 bg-amber-50 text-amber-950 dark:border-amber-800 dark:bg-amber-950/60 dark:text-amber-50',
  error: 'border-rose-200 bg-rose-50 text-rose-950 dark:border-rose-800 dark:bg-rose-950/60 dark:text-rose-50',
}

type AnnouncementBannerProps = {
  announcements: readonly SiteAnnouncement[]
}

/** P216: Site-wide banners; dismiss persists per-announcement ID in localStorage. Accessible: role=&quot;alert&quot;. */
export function AnnouncementBanner({ announcements }: AnnouncementBannerProps) {
  const ids = useMemo(() => announcements.map((a) => a.id).join('|'), [announcements])

  const [dismissed, setDismissed] = useState<Set<string>>(() =>
    typeof window !== 'undefined' ? readDismissed(announcements.map((a) => a.id)) : new Set(),
  )

  useEffect(() => {
    setDismissed(readDismissed(announcements.map((a) => a.id)))
  }, [announcements, ids])

  const visible = announcements.filter((a) => !dismissed.has(a.id))
  if (visible.length === 0) return null

  return (
    <div className="space-y-2 border-b border-slate-200/80 bg-slate-50/90 px-3 py-2 dark:border-slate-800 dark:bg-slate-900/40">
      {visible.map((a) => {
        const tone: AnnouncementType = a.type ?? 'info'
        const panelId = `announcement-${a.id}-panel`

        const dismiss = () => {
          setDismissed((prev) => {
            const next = new Set(prev)
            next.add(a.id)
            writeDismissed(next)
            return next
          })
        }

        return (
          <div
            key={a.id}
            id={panelId}
            role="alert"
            aria-labelledby={`${panelId}-title`}
            className={`flex flex-wrap items-start justify-between gap-3 rounded-xl border px-4 py-3 text-sm shadow-sm ${stylesByType[tone]}`}
          >
            <div className="min-w-0 flex-1">
              <p id={`${panelId}-title`} className="font-semibold">
                {a.title}
              </p>
              {a.message ? <p className="mt-1 text-sm opacity-95">{a.message}</p> : null}
            </div>
            <button
              type="button"
              className="shrink-0 rounded-lg px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-current underline-offset-4 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
              aria-label={`Dismiss announcement: ${a.title}`}
              onClick={dismiss}
            >
              Dismiss
            </button>
          </div>
        )
      })}
    </div>
  )
}
