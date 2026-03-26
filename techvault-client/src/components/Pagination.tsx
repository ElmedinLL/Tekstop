import { useEffect, useMemo, useRef, type ReactNode } from 'react'
import { useSearchParams } from 'react-router-dom'

function clampPage(page: number, min: number, max: number) {
  if (!Number.isFinite(page)) return min
  if (page < min) return min
  if (page > max) return max
  return page
}

function buildPageItems(currentPage: number, totalPages: number): Array<number | '…'> {
  if (totalPages <= 1) return [1]

  // If small, show everything.
  if (totalPages <= 7) return Array.from({ length: totalPages }, (_, i) => i + 1)

  const siblingCount = 1
  const leftBound = Math.max(2, currentPage - siblingCount)
  const rightBound = Math.min(totalPages - 1, currentPage + siblingCount)

  const items: Array<number | '…'> = [1]

  if (leftBound > 2) {
    items.push('…')
  }

  for (let p = leftBound; p <= rightBound; p++) {
    items.push(p)
  }

  if (rightBound < totalPages - 1) {
    items.push('…')
  }

  items.push(totalPages)
  return items
}

export type PaginationProps = {
  totalPages: number
  /**
   * URL query param key to sync with. Defaults to `page` → `?page=`.
   */
  queryParamKey?: string
  /**
   * Optional render override for prev/next.
   */
  renderPrev?: ReactNode
  renderNext?: ReactNode
  /**
   * Optional className override for the pagination wrapper.
   */
  className?: string
}

export function Pagination({
  totalPages,
  queryParamKey = 'page',
  renderPrev = 'Previous',
  renderNext = 'Next',
  className = '',
}: PaginationProps) {
  const [searchParams, setSearchParams] = useSearchParams()

  const raw = searchParams.get(queryParamKey)
  const parsed = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : 1

  const currentPage = clampPage(parsed, 1, Math.max(1, totalPages || 1))
  const previousPageRef = useRef(currentPage)

  const pageItems = useMemo(() => buildPageItems(currentPage, Math.max(1, totalPages)), [currentPage, totalPages])

  useEffect(() => {
    // Scroll to top when the page actually changes.
    const prev = previousPageRef.current
    if (prev !== currentPage) {
      window.scrollTo({ top: 0, behavior: 'instant' as ScrollBehavior })
    }
    previousPageRef.current = currentPage
  }, [currentPage])

  const goToPage = (next: number) => {
    const clamped = clampPage(next, 1, Math.max(1, totalPages || 1))
    if (clamped === currentPage) return

    setSearchParams((prev) => {
      const nextParams = new URLSearchParams(prev)
      nextParams.set(queryParamKey, String(clamped))
      return nextParams
    })
  }

  if (totalPages <= 1) return null

  return (
    <nav className={`flex flex-wrap items-center justify-center gap-2 border-t border-slate-200 pt-8 ${className}`}>
      <button
        type="button"
        onClick={() => goToPage(currentPage - 1)}
        disabled={currentPage <= 1}
        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {renderPrev}
      </button>

      <div className="flex items-center gap-2">
        {pageItems.map((item, idx) => {
          if (item === '…') {
            return (
              <span key={`ellipsis-${idx}`} className="px-2 text-sm text-slate-500">
                …
              </span>
            )
          }

          const isActive = item === currentPage
          return (
            <button
              key={item}
              type="button"
              onClick={() => goToPage(item)}
              aria-current={isActive ? 'page' : undefined}
              className={`rounded-lg border px-3 py-2 text-sm font-medium shadow-sm ${
                isActive
                  ? 'border-blue-600 bg-blue-50 text-blue-700'
                  : 'border-slate-300 bg-white text-slate-700 hover:bg-slate-50'
              }`}
            >
              {item}
            </button>
          )
        })}
      </div>

      <button
        type="button"
        onClick={() => goToPage(currentPage + 1)}
        disabled={currentPage >= totalPages}
        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {renderNext}
      </button>
    </nav>
  )
}

