import { useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Pagination } from './Pagination'
import { StarRatingDisplay } from './ReviewForm'
import type { ProductReview } from '../types/review'

function StarIcon({ className = '' }: { className?: string }) {
  return (
    <svg className={`h-6 w-6 shrink-0 ${className}`} viewBox="0 0 20 20" aria-hidden>
      <path
        fill="currentColor"
        d="M10 1.5l2.6 5.3 5.9.9-4.3 4.2 1 5.9L10 15.9 4.8 17.8l1-5.9L1.5 7.7l5.9-.9L10 1.5z"
      />
    </svg>
  )
}

/** Average rating with partial star fills (0–5). */
function FractionalStars({ rating }: { rating: number }) {
  const clamped = Math.min(5, Math.max(0, rating))

  return (
    <div className="flex items-center gap-0.5" role="img" aria-label={`${clamped.toFixed(1)} out of 5 stars`}>
      {Array.from({ length: 5 }, (_, i) => {
        const starIndex = i + 1
        const fill = Math.min(1, Math.max(0, clamped - (starIndex - 1)))
        if (fill >= 1) {
          return <StarIcon key={i} className="text-amber-400" />
        }
        if (fill <= 0) {
          return <StarIcon key={i} className="text-slate-200" />
        }
        return (
          <span key={i} className="relative inline-block h-6 w-6 shrink-0">
            <StarIcon className="text-slate-200" />
            <span className="absolute inset-0 overflow-hidden" style={{ width: `${fill * 100}%` }}>
              <StarIcon className="text-amber-400" />
            </span>
          </span>
        )
      })}
    </div>
  )
}

function initialsFromDisplayName(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) {
    return '?'
  }
  if (parts.length === 1) {
    return parts[0]!.slice(0, 2).toUpperCase()
  }
  return (parts[0]![0]! + parts[parts.length - 1]![0]!).toUpperCase()
}

function avatarStyles(name: string): { bg: string; fg: string } {
  let h = 0
  for (let i = 0; i < name.length; i++) {
    h = name.charCodeAt(i) + ((h << 5) - h)
  }
  const hue = Math.abs(h) % 360
  return {
    bg: `hsl(${hue} 42% 90%)`,
    fg: `hsl(${hue} 35% 28%)`,
  }
}

function formatReviewDate(iso: string) {
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(iso))
}

function clampPage(page: number, totalPages: number) {
  const max = Math.max(1, totalPages)
  if (!Number.isFinite(page)) {
    return 1
  }
  if (page < 1) {
    return 1
  }
  if (page > max) {
    return max
  }
  return page
}

export type ReviewListProps = {
  reviews: ProductReview[]
  /** Items per page (default 8). */
  pageSize?: number
  /** URL query key for the current page (default `reviewPage` to avoid clashing with `page`). */
  queryParamKey?: string
  className?: string
}

export function ReviewList({ reviews, pageSize = 8, queryParamKey = 'reviewPage', className = '' }: ReviewListProps) {
  const [searchParams] = useSearchParams()

  const totalPages = Math.max(1, Math.ceil(reviews.length / Math.max(1, pageSize)))

  const raw = searchParams.get(queryParamKey)
  const parsed = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : 1
  const currentPage = clampPage(parsed, totalPages)

  const pageReviews = useMemo(() => {
    const start = (currentPage - 1) * pageSize
    return reviews.slice(start, start + pageSize)
  }, [reviews, currentPage, pageSize])

  const average = useMemo(() => {
    if (reviews.length === 0) {
      return 0
    }
    return reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length
  }, [reviews])

  if (reviews.length === 0) {
    return null
  }

  return (
    <div className={className}>
      <div className="rounded-2xl border border-slate-200 bg-gradient-to-br from-slate-50 to-white p-6 shadow-sm">
        <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Average rating</p>
        <div className="mt-3 flex flex-wrap items-center gap-4">
          <span className="text-4xl font-bold tabular-nums text-slate-900">{average.toFixed(1)}</span>
          <div className="flex flex-col gap-1">
            <FractionalStars rating={average} />
            <span className="text-sm text-slate-600">
              Based on {reviews.length} review{reviews.length === 1 ? '' : 's'}
            </span>
          </div>
        </div>
      </div>

      <ul className="mt-8 space-y-4" aria-label="Reviews">
        {pageReviews.map((r) => {
          const { bg, fg } = avatarStyles(r.authorDisplayName)
          const initials = initialsFromDisplayName(r.authorDisplayName)

          return (
            <li
              key={r.id}
              className="flex gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"
            >
              <div
                className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full text-sm font-semibold"
                style={{ backgroundColor: bg, color: fg }}
                aria-hidden
              >
                {initials}
              </div>
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
                  <span className="font-medium text-slate-900">{r.authorDisplayName}</span>
                  <time className="text-xs text-slate-500" dateTime={r.createdAtUtc}>
                    {formatReviewDate(r.createdAtUtc)}
                  </time>
                </div>
                <div className="mt-2">
                  <StarRatingDisplay rating={r.rating} size="sm" />
                </div>
                {r.comment ? (
                  <p className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-slate-700">{r.comment}</p>
                ) : (
                  <p className="mt-3 text-sm italic text-slate-500">No comment.</p>
                )}
              </div>
            </li>
          )
        })}
      </ul>

      <Pagination totalPages={totalPages} queryParamKey={queryParamKey} className="mt-6" />
    </div>
  )
}
