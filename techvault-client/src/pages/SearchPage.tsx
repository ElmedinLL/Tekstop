import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { api } from '../lib/api'
import { ProductCard } from '../components/ProductCard'
import { ProductGrid } from '../components/ProductGrid'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import type { PagedProductsResponse } from '../types/product'

type SearchSuggestions = { label: string; value: string }[]

const suggestions: SearchSuggestions = [
  { label: "Try “laptop”", value: 'laptop' },
  { label: 'Try “wireless”', value: 'wireless' },
  { label: 'Try “TV-LAP-001” (SKU)', value: 'TV-LAP-001' },
]

async function fetchSearchResults(searchTerm: string): Promise<PagedProductsResponse> {
  const { data } = await api.get<PagedProductsResponse>('/products', {
    params: {
      search: searchTerm,
      page: 1,
      pageSize: 24,
    },
    paramsSerializer: {
      indexes: null,
    },
  })
  return data
}

export function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const qFromUrl = searchParams.get('q') ?? ''
  const [qInput, setQInput] = useState(qFromUrl)

  useEffect(() => {
    setQInput(qFromUrl)
  }, [qFromUrl])

  const q = qFromUrl.trim()

  const {
    data: pageData,
    isPending,
    isError,
    error,
  } = useQuery({
    queryKey: ['search-products', q],
    queryFn: () => fetchSearchResults(q),
    enabled: q.length > 0,
    staleTime: 30_000,
  })

  const totalCount = pageData?.totalCount ?? 0
  const items = pageData?.items ?? []

  const resultsNotFound =
    isError && axios.isAxiosError(error) && error.response?.status === 404

  const hasSearched = q.length > 0

  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    const next = qInput.trim()
    setSearchParams((prev) => {
      const url = new URLSearchParams(prev)
      if (!next) {
        url.delete('q')
      } else {
        url.set('q', next)
      }
      return url
    })
  }

  const activeSuggestions = useMemo(() => {
    if (!hasSearched) return suggestions
    // If user already typed something, rotate suggestions by giving them a “next try” option.
    return suggestions
  }, [hasSearched])

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <nav className="mb-6 text-sm text-slate-500" aria-label="Breadcrumb">
        <ol className="flex flex-wrap items-center gap-2">
          <li>
            <Link to="/" className="hover:text-blue-600">
              Home
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li className="font-medium text-slate-800" aria-current="page">
            Search
          </li>
        </ol>
      </nav>

      <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <form className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between" onSubmit={onSubmit}>
          <div className="flex-1 space-y-1">
            <label htmlFor="search-q" className="text-sm font-medium text-slate-800">
              Search products
            </label>
            <input
              id="search-q"
              value={qInput}
              onChange={(e) => setQInput(e.target.value)}
              placeholder="e.g. laptop, wireless, TV-LAP-001"
              className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <button
            type="submit"
            className="rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
          >
            Search
          </button>
        </form>

        {hasSearched && (
          <div className="mt-4 flex flex-wrap items-center gap-2 text-sm text-slate-600">
            <span>
              {isPending ? 'Searching…' : `${totalCount} result${totalCount === 1 ? '' : 's'}`}
            </span>
            {totalCount > 0 && (
              <span className="text-slate-400">
                Showing {items.length} on this page
              </span>
            )}
          </div>
        )}
      </div>

      {!hasSearched && (
        <div className="mt-8 rounded-lg border border-dashed border-slate-200 bg-slate-50 p-8 text-center">
          <h2 className="text-lg font-semibold text-slate-900">Start typing to search</h2>
          <p className="mt-2 text-sm text-slate-600">
            Use keywords, brands, or a product SKU. Matches are highlighted in the results.
          </p>
          <div className="mt-5 flex flex-wrap justify-center gap-2">
            {activeSuggestions.map((s) => (
              <button
                key={s.value}
                type="button"
                onClick={() => setSearchParams({ q: s.value })}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
              >
                {s.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {hasSearched && isPending && (
        <div className="mt-8">
          <ProductGrid loading={true} skeletonCount={8} />
        </div>
      )}

      {hasSearched && !isPending && (resultsNotFound || items.length === 0) && (
        <div className="mt-8 rounded-lg border border-dashed border-slate-200 bg-white p-8 text-center">
          <h2 className="text-lg font-semibold text-slate-900">No results for “{q}”</h2>
          <p className="mt-2 text-sm text-slate-600">
            Try one of the suggestions below, or search using a shorter keyword.
          </p>

          <div className="mt-5 flex flex-wrap justify-center gap-2">
            {activeSuggestions.map((s) => (
              <button
                key={s.value}
                type="button"
                onClick={() => setSearchParams({ q: s.value })}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
              >
                {s.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {hasSearched && !isPending && items.length > 0 && (
        <div className="mt-8">
          <ProductGrid loading={false} skeletonCount={8}>
            {items.map((p) => (
              <ProductCard
                key={p.id}
                id={p.id}
                name={p.name}
                highlightQuery={q}
                imageUrl={p.imageUrl ? resolveApiAssetUrl(p.imageUrl) : null}
                price={p.price}
                compareAtPrice={p.compareAtPrice}
                stockQuantity={p.stockQuantity}
                productTo={`/products/${p.id}`}
              />
            ))}
          </ProductGrid>
        </div>
      )}
    </div>
  )
}

