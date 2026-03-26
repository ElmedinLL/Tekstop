import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import {
  parseCategorySlugs,
  parseInStockOnly,
  parseMinRating,
  ProductFilterKeys,
  serializeCategorySlugs,
} from '../lib/productFilterParams'
import type { CategoryListItem } from '../types/category'

export type ProductFiltersProps = {
  /** Slider and input bounds for price (major units, e.g. dollars). */
  priceBounds?: { min: number; max: number }
  className?: string
  /** When true, category checkboxes are hidden (e.g. on a category-scoped listing page). */
  hideCategories?: boolean
}

const defaultPriceBounds = { min: 0, max: 10_000 }

async function fetchCategories(): Promise<CategoryListItem[]> {
  const { data } = await api.get<CategoryListItem[]>('/categories')
  return Array.isArray(data) ? data : []
}

function useDebouncedCallback(fn: () => void, delayMs: number) {
  const t = useRef<ReturnType<typeof setTimeout> | null>(null)
  const fnRef = useRef(fn)
  fnRef.current = fn

  return useCallback(() => {
    if (t.current) {
      clearTimeout(t.current)
    }
    t.current = setTimeout(() => {
      fnRef.current()
    }, delayMs)
  }, [delayMs])
}

export function ProductFilters({
  priceBounds = defaultPriceBounds,
  className = '',
  hideCategories = false,
}: ProductFiltersProps) {
  const [searchParams, setSearchParams] = useSearchParams()
  const { min: boundMin, max: boundMax } = priceBounds

  const selectedSlugs = useMemo(
    () => parseCategorySlugs(searchParams.get(ProductFilterKeys.categories)),
    [searchParams],
  )
  const inStockOnly = useMemo(() => parseInStockOnly(searchParams.get(ProductFilterKeys.inStockOnly)), [searchParams])
  const minRating = useMemo(() => parseMinRating(searchParams.get(ProductFilterKeys.minRating)), [searchParams])

  const minPriceUrl = searchParams.get(ProductFilterKeys.minPrice)
  const maxPriceUrl = searchParams.get(ProductFilterKeys.maxPrice)
  const minPriceNum = minPriceUrl != null && minPriceUrl !== '' ? Number(minPriceUrl) : null
  const maxPriceNum = maxPriceUrl != null && maxPriceUrl !== '' ? Number(maxPriceUrl) : null

  const [priceMin, setPriceMin] = useState(() =>
    minPriceNum != null && Number.isFinite(minPriceNum) ? Math.max(boundMin, minPriceNum) : boundMin,
  )
  const [priceMax, setPriceMax] = useState(() =>
    maxPriceNum != null && Number.isFinite(maxPriceNum) ? Math.min(boundMax, maxPriceNum) : boundMax,
  )

  const priceRef = useRef({ min: priceMin, max: priceMax })
  useEffect(() => {
    priceRef.current = { min: priceMin, max: priceMax }
  }, [priceMin, priceMax])

  useEffect(() => {
    const nextMin =
      minPriceNum != null && Number.isFinite(minPriceNum) ? Math.max(boundMin, minPriceNum) : boundMin
    const nextMax =
      maxPriceNum != null && Number.isFinite(maxPriceNum) ? Math.min(boundMax, maxPriceNum) : boundMax
    const clampedMax = Math.max(nextMin, nextMax)
    setPriceMin(nextMin)
    setPriceMax(clampedMax)
    priceRef.current = { min: nextMin, max: clampedMax }
  }, [boundMin, boundMax, minPriceNum, maxPriceNum])

  const patchParams = useCallback(
    (mutate: (p: URLSearchParams) => void) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev)
          mutate(next)
          next.set(ProductFilterKeys.page, '1')
          return next
        },
        { replace: false },
      )
    },
    [setSearchParams],
  )

  const flushPriceToUrl = useCallback(() => {
    const { min, max } = priceRef.current
    patchParams((next) => {
      if (min <= boundMin) {
        next.delete(ProductFilterKeys.minPrice)
      } else {
        next.set(ProductFilterKeys.minPrice, String(min))
      }
      if (max >= boundMax) {
        next.delete(ProductFilterKeys.maxPrice)
      } else {
        next.set(ProductFilterKeys.maxPrice, String(max))
      }
    })
  }, [boundMin, boundMax, patchParams])

  const debouncedFlushPrice = useDebouncedCallback(flushPriceToUrl, 350)

  const onPriceMinChange = (v: number) => {
    const next = Math.min(v, priceRef.current.max)
    setPriceMin(next)
    priceRef.current = { min: next, max: priceRef.current.max }
    debouncedFlushPrice()
  }

  const onPriceMaxChange = (v: number) => {
    const next = Math.max(v, priceRef.current.min)
    setPriceMax(next)
    priceRef.current = { min: priceRef.current.min, max: next }
    debouncedFlushPrice()
  }

  const toggleCategory = (slug: string, checked: boolean) => {
    const set = new Set(selectedSlugs)
    if (checked) {
      set.add(slug)
    } else {
      set.delete(slug)
    }
    const serialized = serializeCategorySlugs([...set])
    patchParams((next) => {
      if (serialized == null) {
        next.delete(ProductFilterKeys.categories)
      } else {
        next.set(ProductFilterKeys.categories, serialized)
      }
    })
  }

  const setRatingFilter = (value: string) => {
    patchParams((next) => {
      if (value === '' || value === 'any') {
        next.delete(ProductFilterKeys.minRating)
      } else {
        next.set(ProductFilterKeys.minRating, value)
      }
    })
  }

  const setInStock = (checked: boolean) => {
    patchParams((next) => {
      if (!checked) {
        next.delete(ProductFilterKeys.inStockOnly)
      } else {
        next.set(ProductFilterKeys.inStockOnly, 'true')
      }
    })
  }

  const { data: categories, isPending, isError } = useQuery({
    queryKey: ['categories', 'filters'],
    queryFn: fetchCategories,
    staleTime: 5 * 60 * 1000,
    enabled: !hideCategories,
  })

  const activeCategories = useMemo(
    () =>
      (categories ?? [])
        .filter((c) => c.isActive)
        .sort((a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name)),
    [categories],
  )

  const ratingSelectValue =
    minRating == null ? 'any' : String(Math.min(5, Math.max(1, minRating)))

  return (
    <aside
      className={`w-full max-w-sm space-y-8 rounded-xl border border-slate-200 bg-white p-5 shadow-sm lg:sticky lg:top-4 ${className}`}
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Filters</h2>

      {!hideCategories && (
        <fieldset className="space-y-3">
          <legend className="text-sm font-medium text-slate-900">Category</legend>
          {isPending && <p className="text-sm text-slate-500">Loading categories…</p>}
          {isError && <p className="text-sm text-red-600">Could not load categories.</p>}
          {!isPending && !isError && activeCategories.length === 0 && (
            <p className="text-sm text-slate-500">No categories.</p>
          )}
          <ul className="max-h-56 space-y-2 overflow-y-auto pr-1">
            {activeCategories.map((c) => {
              const id = `cat-${c.slug}`
              return (
                <li key={c.id} className="flex items-start gap-2">
                  <input
                    id={id}
                    type="checkbox"
                    className="mt-1 h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                    checked={selectedSlugs.includes(c.slug)}
                    onChange={(e) => toggleCategory(c.slug, e.target.checked)}
                  />
                  <label htmlFor={id} className="flex-1 cursor-pointer text-sm text-slate-800">
                    {c.name}
                    <span className="ml-1 text-slate-400">({c.productCount})</span>
                  </label>
                </li>
              )
            })}
          </ul>
        </fieldset>
      )}

      <fieldset className="space-y-4">
        <legend className="text-sm font-medium text-slate-900">Price</legend>
        <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-slate-600">
          <span>
            Min: <strong className="text-slate-900">{formatMoney(priceMin)}</strong>
          </span>
          <span>
            Max: <strong className="text-slate-900">{formatMoney(priceMax)}</strong>
          </span>
        </div>
        <div className="space-y-3">
          <label className="block text-xs text-slate-500" htmlFor="filter-price-min">
            Minimum
          </label>
          <input
            id="filter-price-min"
            type="range"
            min={boundMin}
            max={boundMax}
            step={boundMax >= 1000 ? 10 : 1}
            value={priceMin}
            onChange={(e) => onPriceMinChange(Number(e.target.value))}
            className="h-2 w-full cursor-pointer appearance-none rounded-lg bg-slate-200 accent-blue-600"
          />
          <label className="block text-xs text-slate-500" htmlFor="filter-price-max">
            Maximum
          </label>
          <input
            id="filter-price-max"
            type="range"
            min={boundMin}
            max={boundMax}
            step={boundMax >= 1000 ? 10 : 1}
            value={priceMax}
            onChange={(e) => onPriceMaxChange(Number(e.target.value))}
            className="h-2 w-full cursor-pointer appearance-none rounded-lg bg-slate-200 accent-blue-600"
          />
        </div>
      </fieldset>

      <fieldset>
        <legend className="mb-2 text-sm font-medium text-slate-900">Minimum rating</legend>
        <label htmlFor="filter-min-rating" className="sr-only">
          Minimum rating
        </label>
        <select
          id="filter-min-rating"
          value={ratingSelectValue}
          onChange={(e) => setRatingFilter(e.target.value)}
          className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        >
          <option value="any">Any rating</option>
          <option value="4">4+ stars</option>
          <option value="3">3+ stars</option>
          <option value="2">2+ stars</option>
          <option value="1">1+ stars</option>
        </select>
      </fieldset>

      <div className="flex items-center gap-2">
        <input
          id="filter-in-stock"
          type="checkbox"
          className="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
          checked={inStockOnly}
          onChange={(e) => setInStock(e.target.checked)}
        />
        <label htmlFor="filter-in-stock" className="cursor-pointer text-sm text-slate-800">
          In stock only
        </label>
      </div>
    </aside>
  )
}

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)
}
