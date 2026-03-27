import { useMemo, useState, type MouseEvent, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../auth/AuthContext'
import {
  notifyWishlistAuthRequired,
  notifyWishlistError,
  notifyWishlistToggled,
} from '../lib/notifications'
import { addToWishlist, fetchWishlist, removeFromWishlist } from '../lib/wishlist'

export type ProductCardProps = {
  id: number | string
  name: string
  imageUrl?: string | null
  /** Price in major units (e.g. dollars). */
  price: number
  compareAtPrice?: number | null
  /** Average rating 0–5; if omitted, stars are hidden. */
  rating?: number
  reviewCount?: number
  stockQuantity: number
  currency?: string
  /** In-app route (e.g. `/products/my-slug`). Image and title link here. */
  productTo?: string
  onAddToCart?: (id: ProductCardProps['id']) => void
  /** Optional: called after wishlist is updated successfully. */
  onWishlistChange?: (id: ProductCardProps['id'], inWishlist: boolean) => void
  className?: string
  /**
   * Optional query to visually highlight in the product title (used by search results).
   * Example: "probook" will highlight matching substrings within `name`.
   */
  highlightQuery?: string | null
}

const priceFormatter = (currency: string) =>
  new Intl.NumberFormat(undefined, { style: 'currency', currency, maximumFractionDigits: 2 })

function escapeRegExp(input: string) {
  return input.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}

function highlightText(text: string, query: string | null | undefined): ReactNode {
  const q = query?.trim() ?? ''
  if (!q) {
    return text
  }

  const tokens = q
    .split(/\s+/)
    .map((t) => t.trim())
    .filter(Boolean)
    .slice(0, 8)
    .sort((a, b) => b.length - a.length)

  if (tokens.length === 0) {
    return text
  }

  const re = new RegExp(tokens.map(escapeRegExp).join('|'), 'ig')

  // Fast path: no match at all.
  re.lastIndex = 0
  const first = re.exec(text)
  if (!first || first.index == null) {
    return text
  }

  re.lastIndex = 0

  const nodes: ReactNode[] = []
  let lastIndex = 0
  let match: RegExpExecArray | null

  while ((match = re.exec(text)) !== null) {
    const start = match.index
    const end = start + match[0].length

    if (start > lastIndex) {
      nodes.push(text.slice(lastIndex, start))
    }

    nodes.push(
      <mark key={`${start}-${end}`} className="rounded bg-yellow-200 px-0.5 text-slate-900">
        {text.slice(start, end)}
      </mark>,
    )

    lastIndex = end

    // Avoid infinite loops on zero-length matches.
    if (re.lastIndex === start) {
      re.lastIndex++
    }
  }

  if (lastIndex < text.length) {
    nodes.push(text.slice(lastIndex))
  }

  return <>{nodes}</>
}

function StarIcon({ className = '' }: { className?: string }) {
  return (
    <svg className={`h-4 w-4 shrink-0 ${className}`} viewBox="0 0 20 20" aria-hidden>
      <path
        fill="currentColor"
        d="M10 1.5l2.6 5.3 5.9.9-4.3 4.2 1 5.9L10 15.9 4.8 17.8l1-5.9L1.5 7.7l5.9-.9L10 1.5z"
      />
    </svg>
  )
}

function StarRating({ rating }: { rating: number }) {
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
          <span key={i} className="relative inline-block h-4 w-4 shrink-0">
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

function HeartIcon({ filled, className = '' }: { filled: boolean; className?: string }) {
  return (
    <svg
      className={`h-5 w-5 transition-colors duration-200 ${className}`}
      viewBox="0 0 24 24"
      fill={filled ? 'currentColor' : 'none'}
      stroke="currentColor"
      strokeWidth="2"
      aria-hidden
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
      />
    </svg>
  )
}

export function ProductCard({
  id,
  name,
  imageUrl,
  price,
  compareAtPrice,
  rating,
  reviewCount,
  stockQuantity,
  currency = 'USD',
  productTo,
  onAddToCart,
  onWishlistChange,
  className = '',
  highlightQuery = null,
}: ProductCardProps) {
  const [imgFailed, setImgFailed] = useState(false)
  const [heartPop, setHeartPop] = useState(false)
  const { isAuthenticated, isInitializing } = useAuth()
  const queryClient = useQueryClient()

  const productIdNum = useMemo(() => {
    const n = typeof id === 'number' ? id : Number.parseInt(String(id), 10)
    return Number.isFinite(n) && n > 0 ? n : null
  }, [id])

  const wishlistQuery = useQuery({
    queryKey: ['wishlist'],
    queryFn: fetchWishlist,
    enabled: isAuthenticated && !isInitializing && productIdNum != null,
  })

  const wishlisted =
    productIdNum != null &&
    (wishlistQuery.data?.some((w) => w.productId === productIdNum) ?? false)

  const wishlistToggleMu = useMutation({
    mutationFn: async () => {
      if (productIdNum == null) {
        throw new Error('Invalid product')
      }
      if (wishlisted) {
        await removeFromWishlist(productIdNum)
        return false
      }
      await addToWishlist(productIdNum)
      return true
    },
    onSuccess: (inWishlist) => {
      void queryClient.invalidateQueries({ queryKey: ['wishlist'] })
      onWishlistChange?.(id, inWishlist)
      setHeartPop(true)
      window.setTimeout(() => setHeartPop(false), 450)
      notifyWishlistToggled(inWishlist)
    },
    onError: () => {
      notifyWishlistError()
    },
  })

  const inStock = stockQuantity > 0
  const fmt = priceFormatter(currency)
  const showRating = typeof rating === 'number' && !Number.isNaN(rating)

  const handleWishlistClick = (e: MouseEvent<HTMLButtonElement>) => {
    e.preventDefault()
    e.stopPropagation()
    if (isInitializing) {
      return
    }
    if (!isAuthenticated) {
      notifyWishlistAuthRequired()
      return
    }
    if (productIdNum == null || wishlistToggleMu.isPending) {
      return
    }
    wishlistToggleMu.mutate()
  }

  const imageBlock = (
    <div className="relative aspect-[4/3] overflow-hidden rounded-t-xl bg-slate-100">
      {imageUrl && !imgFailed ? (
        <img
          src={imageUrl}
          alt=""
          className={`h-full w-full object-cover transition ${!inStock ? 'opacity-60 grayscale' : ''}`}
          onError={() => setImgFailed(true)}
        />
      ) : (
        <div
          className={`flex h-full w-full items-center justify-center text-sm text-slate-400 ${!inStock ? 'opacity-70' : ''}`}
          aria-hidden
        >
          No image
        </div>
      )}
      {!inStock && (
        <span className="absolute left-2 top-2 rounded bg-slate-900/85 px-2 py-0.5 text-xs font-medium text-white">
          Out of stock
        </span>
      )}
      <button
        type="button"
        onClick={handleWishlistClick}
        disabled={wishlistToggleMu.isPending || productIdNum == null}
        className={`absolute right-2 top-2 rounded-full bg-white/95 p-2 shadow-sm ring-1 ring-slate-200/80 transition-[transform,background-color] duration-300 ease-out will-change-transform hover:bg-white focus:outline-none focus-visible:ring-2 focus-visible:ring-red-400 disabled:opacity-60 ${
          wishlisted ? 'text-red-500' : 'text-slate-500 hover:text-red-400'
        } ${heartPop ? 'scale-[1.22]' : 'scale-100'}`}
        aria-label={wishlisted ? 'Remove from wishlist' : 'Add to wishlist'}
        aria-pressed={wishlisted}
      >
        <HeartIcon filled={wishlisted} />
      </button>
    </div>
  )

  const titleBlock = (
    <h3 className="line-clamp-2 min-h-[2.5rem] text-sm font-semibold leading-snug text-slate-900">
      {productTo ? (
        <Link to={productTo} className="hover:text-blue-700 focus:outline-none focus-visible:text-blue-700">
          {highlightText(name, highlightQuery)}
        </Link>
      ) : (
        highlightText(name, highlightQuery)
      )}
    </h3>
  )

  return (
    <article
      className={`flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition hover:shadow-md ${!inStock ? 'opacity-95' : ''} ${className}`}
    >
      {productTo ? (
        <Link to={productTo} className="block shrink-0 focus:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-blue-500">
          <span className="sr-only">{name}</span>
          {imageBlock}
        </Link>
      ) : (
        imageBlock
      )}

      <div className="flex flex-1 flex-col gap-2 p-4">
        {titleBlock}

        {showRating && (
          <div className="flex flex-wrap items-center gap-2">
            <StarRating rating={rating} />
            {typeof reviewCount === 'number' && reviewCount > 0 && (
              <span className="text-xs text-slate-500">({reviewCount})</span>
            )}
          </div>
        )}

        <div className="mt-auto flex flex-wrap items-baseline gap-2">
          <span className="text-lg font-semibold text-slate-900">{fmt.format(price)}</span>
          {compareAtPrice != null && compareAtPrice > price && (
            <span className="text-sm text-slate-400 line-through">{fmt.format(compareAtPrice)}</span>
          )}
        </div>

        <button
          type="button"
          disabled={!inStock}
          onClick={() => {
            if (inStock) onAddToCart?.(id)
          }}
          className={`mt-1 w-full rounded-lg px-3 py-2 text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 ${
            inStock
              ? 'bg-blue-600 text-white hover:bg-blue-700'
              : 'cursor-not-allowed bg-slate-200 text-slate-500'
          }`}
        >
          {inStock ? 'Add to cart' : 'Unavailable'}
        </button>
      </div>
    </article>
  )
}
