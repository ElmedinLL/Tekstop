import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Pagination } from '../../components/Pagination'
import { fetchAdminProducts } from '../../lib/admin'
import { resolveApiAssetUrl } from '../../lib/assetUrl'

const PAGE_SIZES = [10, 20, 50] as const
const DEFAULT_PAGE_SIZE = 20

function formatPrice(n: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)
}

type SortColumn = 'name' | 'price' | 'stock' | 'category'

const sortKeys: Record<SortColumn, readonly [string, string]> = {
  name: ['name_asc', 'name_desc'],
  price: ['price_asc', 'price_desc'],
  stock: ['stock_asc', 'stock_desc'],
  category: ['category_asc', 'category_desc'],
}

function parsePage(raw: string | null) {
  const n = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : 1
  return Number.isFinite(n) && n >= 1 ? n : 1
}

function parsePageSize(raw: string | null) {
  const n = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : DEFAULT_PAGE_SIZE
  if (!Number.isFinite(n)) return DEFAULT_PAGE_SIZE
  return PAGE_SIZES.includes(n as (typeof PAGE_SIZES)[number]) ? n : DEFAULT_PAGE_SIZE
}

type SortableThProps = {
  label: string
  column: SortColumn
  activeSort: string | null
  onChangeSort: (next: string) => void
}

function SortableTh({ label, column, activeSort, onChangeSort }: SortableThProps) {
  const [asc, desc] = sortKeys[column]
  const isAsc = activeSort === asc
  const isDesc = activeSort === desc

  return (
    <th scope="col" className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-600">
      <button
        type="button"
        onClick={() => onChangeSort(isAsc ? desc : asc)}
        className="inline-flex items-center gap-1 rounded-md text-left hover:text-slate-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
      >
        {label}
        <span className="font-normal text-slate-400" aria-hidden>
          {isAsc ? '▲' : isDesc ? '▼' : '⇅'}
        </span>
      </button>
    </th>
  )
}

export function AdminProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = parsePage(searchParams.get('page'))
  const pageSize = parsePageSize(searchParams.get('pageSize'))
  const sort = searchParams.get('sort')?.trim() || null
  const searchFromUrl = searchParams.get('search') ?? ''

  const [searchInput, setSearchInput] = useState(searchFromUrl)

  useEffect(() => {
    setSearchInput(searchFromUrl)
  }, [searchFromUrl])

  useEffect(() => {
    const t = window.setTimeout(() => {
      const trimmed = searchInput.trim()
      const urlTrimmed = searchFromUrl.trim()
      if (trimmed === urlTrimmed) return

      setSearchParams((prev) => {
        const next = new URLSearchParams(prev)
        if (trimmed) next.set('search', trimmed)
        else next.delete('search')
        next.set('page', '1')
        return next
      })
    }, 350)
    return () => window.clearTimeout(t)
  }, [searchInput, searchFromUrl, setSearchParams])

  const queryArgs = useMemo(
    () => ({
      page,
      pageSize,
      search: searchFromUrl.trim() || undefined,
      sort: sort ?? undefined,
    }),
    [page, pageSize, searchFromUrl, sort],
  )

  const listQuery = useQuery({
    queryKey: ['admin', 'products', queryArgs],
    queryFn: () => fetchAdminProducts(queryArgs),
  })

  const setSort = useCallback(
    (nextSort: string) => {
      setSearchParams((prev) => {
        const next = new URLSearchParams(prev)
        next.set('sort', nextSort)
        next.set('page', '1')
        return next
      })
    },
    [setSearchParams],
  )

  const setPageSize = useCallback(
    (next: number) => {
      setSearchParams((prev) => {
        const n = new URLSearchParams(prev)
        n.set('pageSize', String(next))
        n.set('page', '1')
        return n
      })
    },
    [setSearchParams],
  )

  const totalPages = listQuery.data?.totalPages ?? 0

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Products</h1>
          <p className="mt-1 text-sm text-slate-600">Search, sort, and paginate the catalog (including drafts).</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <span className="whitespace-nowrap">Per page</span>
            <select
              value={pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {PAGE_SIZES.map((n) => (
                <option key={n} value={n}>
                  {n}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-100 p-4">
          <label className="block text-xs font-medium text-slate-600" htmlFor="admin-product-search">
            Search
          </label>
          <input
            id="admin-product-search"
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Name or description…"
            className="mt-1 w-full max-w-md rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            autoComplete="off"
          />
        </div>

        {listQuery.isPending && (
          <div className="space-y-2 p-4">
            {[0, 1, 2, 3, 4].map((i) => (
              <div key={i} className="h-14 animate-pulse rounded-lg bg-slate-100" />
            ))}
          </div>
        )}

        {listQuery.isError && (
          <p className="p-6 text-sm text-rose-600">Could not load products.</p>
        )}

        {listQuery.data && listQuery.data.items.length === 0 && (
          <p className="p-6 text-sm text-slate-600">No products match your filters.</p>
        )}

        {listQuery.data && listQuery.data.items.length > 0 && (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[720px] text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80">
                    <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                      Image
                    </th>
                    <SortableTh
                      label="Name"
                      column="name"
                      activeSort={sort}
                      onChangeSort={setSort}
                    />
                    <SortableTh
                      label="Price"
                      column="price"
                      activeSort={sort}
                      onChangeSort={setSort}
                    />
                    <SortableTh
                      label="Stock"
                      column="stock"
                      activeSort={sort}
                      onChangeSort={setSort}
                    />
                    <SortableTh
                      label="Category"
                      column="category"
                      activeSort={sort}
                      onChangeSort={setSort}
                    />
                    <th scope="col" className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {listQuery.data.items.map((p) => {
                    const img = p.imageUrl ? resolveApiAssetUrl(p.imageUrl) : ''
                    return (
                      <tr key={p.id} className="text-slate-800">
                        <td className="px-4 py-3">
                          <div className="h-12 w-12 overflow-hidden rounded-lg border border-slate-100 bg-slate-100">
                            {img ? (
                              <img src={img} alt="" className="h-full w-full object-cover" />
                            ) : (
                              <div className="flex h-full w-full items-center justify-center text-xs text-slate-400">
                                —
                              </div>
                            )}
                          </div>
                        </td>
                        <td className="max-w-[200px] px-4 py-3">
                          <p className="truncate font-medium text-slate-900" title={p.name}>
                            {p.name}
                          </p>
                          {!p.isPublished && (
                            <span className="mt-0.5 inline-block rounded bg-amber-100 px-1.5 py-0.5 text-xs font-medium text-amber-900">
                              Draft
                            </span>
                          )}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 tabular-nums">{formatPrice(p.price)}</td>
                        <td className="whitespace-nowrap px-4 py-3 tabular-nums">{p.stockQuantity}</td>
                        <td className="max-w-[140px] truncate px-4 py-3 text-slate-600" title={p.categoryName}>
                          {p.categoryName}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 text-right">
                          <div className="flex flex-wrap items-center justify-end gap-3">
                            {p.isPublished ? (
                              <Link
                                to={`/products/${p.id}`}
                                target="_blank"
                                rel="noreferrer"
                                className="text-sm font-medium text-blue-600 hover:underline"
                              >
                                View
                              </Link>
                            ) : (
                              <span
                                className="text-sm text-slate-400"
                                title="Publish the product to open it on the storefront"
                              >
                                View
                              </span>
                            )}
                            <span className="text-sm text-slate-400" title="Product editor coming soon">
                              Edit
                            </span>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>

            <div className="flex flex-col gap-2 border-t border-slate-100 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
              <p className="text-xs text-slate-500">
                {listQuery.data.totalCount.toLocaleString()} product
                {listQuery.data.totalCount === 1 ? '' : 's'}
                {sort ? (
                  <>
                    {' '}
                    · sort: <span className="font-mono text-slate-700">{sort}</span>
                  </>
                ) : (
                  ' · sort: newest'
                )}
              </p>
              {totalPages > 1 && <Pagination totalPages={totalPages} className="border-0 pt-0" />}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
