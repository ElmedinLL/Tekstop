import type { ReactNode } from 'react'

const gridClassName =
  'grid grid-cols-1 gap-4 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 lg:gap-6 xl:grid-cols-4'

export type ProductGridProps = {
  children?: ReactNode
  /** When true, shows skeleton placeholders instead of children. */
  loading?: boolean
  /** Number of skeleton cards while loading (default 8). */
  skeletonCount?: number
  className?: string
}

/** Placeholder card matching ProductCard layout (image 4:3, title, rating, price, button). */
export function ProductCardSkeleton({ className = '' }: { className?: string }) {
  return (
    <div
      className={`overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm ${className}`}
      aria-hidden
    >
      <div className="aspect-[4/3] animate-pulse bg-slate-200" />
      <div className="space-y-3 p-4">
        <div className="space-y-2">
          <div className="h-4 w-11/12 max-w-[14rem] animate-pulse rounded bg-slate-200" />
          <div className="h-4 w-2/3 max-w-[10rem] animate-pulse rounded bg-slate-200" />
        </div>
        <div className="flex gap-1">
          {Array.from({ length: 5 }, (_, i) => (
            <div key={i} className="h-4 w-4 animate-pulse rounded-sm bg-slate-200" />
          ))}
        </div>
        <div className="h-6 w-24 animate-pulse rounded bg-slate-200" />
        <div className="h-10 w-full animate-pulse rounded-lg bg-slate-200" />
      </div>
    </div>
  )
}

/**
 * Responsive product grid: 1 column on small screens, 2 from `sm`, 3 from `lg`, 4 from `xl`.
 * Use `loading` to show skeleton cards with the same column layout.
 */
export function ProductGrid({ children, loading = false, skeletonCount = 8, className = '' }: ProductGridProps) {
  const count = Math.max(1, Math.min(24, skeletonCount))

  return (
    <>
      {loading && (
        <span className="sr-only" role="status">
          Loading products
        </span>
      )}
      <div className={`${gridClassName} ${className}`}>
        {loading
          ? Array.from({ length: count }, (_, i) => <ProductCardSkeleton key={i} />)
          : children}
      </div>
    </>
  )
}
