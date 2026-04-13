import { useQueries } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { fetchProductDetail } from '../hooks/useProduct'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { COMPARE_MAX, useCompareStore } from '../store/useCompareStore'
import type { ProductDetail } from '../types/product'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

function collectSpecKeys(rows: (ProductDetail | null | undefined)[]): string[] {
  const keys = new Set<string>()
  for (const p of rows) {
    if (!p?.specs) continue
    for (const k of Object.keys(p.specs)) {
      keys.add(k)
    }
  }
  return [...keys].sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }))
}

const baseRows: { label: string; get: (p: ProductDetail) => string }[] = [
  { label: 'Price', get: (p) => formatMoney(p.price) },
  {
    label: 'Compare-at',
    get: (p) =>
      p.compareAtPrice != null && p.compareAtPrice > p.price ? formatMoney(p.compareAtPrice) : '—',
  },
  { label: 'Brand', get: (p) => p.brand?.trim() || '—' },
  { label: 'Category', get: (p) => p.category.name },
  { label: 'Stock', get: (p) => (p.stock > 0 ? `${p.stock} in stock` : 'Out of stock') },
  { label: 'SKU', get: (p) => p.sku },
]

export function ComparePage() {
  const ids = useCompareStore((s) => s.ids)
  const remove = useCompareStore((s) => s.remove)
  const clear = useCompareStore((s) => s.clear)

  const results = useQueries({
    queries: ids.map((id) => ({
      queryKey: ['product', id] as const,
      queryFn: () => fetchProductDetail(id),
      enabled: id > 0,
    })),
  })

  const columns = ids.map((id, i) => ({ id, result: results[i] }))
  const loading = columns.some(({ result }) => result?.isPending)
  const anyError = columns.some(({ result }) => result?.isError)
  const specKeys = collectSpecKeys(columns.map((c) => c.result?.data))

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:py-10">
      <Seo
        title="Compare products"
        description={`Compare up to ${COMPARE_MAX} TechVault products side by side: price, specs, and availability.`}
      />

      <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Compare products</h1>
          <p className="mt-1 text-sm text-slate-600">
            Up to {COMPARE_MAX} products · {ids.length} selected
          </p>
        </div>
        {ids.length > 0 && (
          <button
            type="button"
            onClick={() => clear()}
            className="self-start rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
          >
            Clear all
          </button>
        )}
      </div>

      {ids.length === 0 && (
        <div className="mt-12 rounded-xl border border-dashed border-slate-300 bg-white p-10 text-center shadow-sm">
          <p className="text-slate-700">No products to compare yet.</p>
          <p className="mt-2 text-sm text-slate-500">
            Use <span className="font-medium text-slate-700">Compare</span> on product cards or the product page (max{' '}
            {COMPARE_MAX}).
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <Link
              to="/search"
              className="rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-blue-700"
            >
              Search products
            </Link>
            <Link
              to="/"
              className="rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 hover:bg-slate-50"
            >
              Browse home
            </Link>
          </div>
        </div>
      )}

      {ids.length > 0 && (
        <div className="mt-8 overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
          {loading && (
            <p className="p-6 text-center text-sm text-slate-500" role="status">
              Loading product details…
            </p>
          )}
          {!loading && anyError && (
            <p className="border-b border-slate-100 p-4 text-center text-sm text-rose-600">
              Some products could not be loaded. You can remove them from the table below.
            </p>
          )}
          {!loading && (
            <table className="min-w-[720px] w-full border-collapse text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50">
                  <th
                    scope="col"
                    className="sticky left-0 z-10 w-36 min-w-[9rem] border-r border-slate-200 bg-slate-50 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500"
                  >
                    Product
                  </th>
                  {columns.map(({ id, result }) => {
                    const p = result.data
                    return (
                      <th key={id} scope="col" className="align-top px-4 py-3">
                        {result.isPending && (
                          <div className="flex h-32 items-center justify-center text-slate-400">Loading…</div>
                        )}
                        {result.isError && (
                          <div className="space-y-2 text-left">
                            <p className="text-xs text-rose-600">Could not load</p>
                            <button
                              type="button"
                              onClick={() => remove(id)}
                              className="text-xs font-medium text-rose-600 hover:underline"
                            >
                              Remove
                            </button>
                          </div>
                        )}
                        {!result.isPending && !result.isError && result.data === null && (
                          <div className="space-y-2 text-left">
                            <p className="text-xs text-slate-600">No longer available</p>
                            <button
                              type="button"
                              onClick={() => remove(id)}
                              className="text-xs font-medium text-rose-600 hover:underline"
                            >
                              Remove
                            </button>
                          </div>
                        )}
                        {p && (
                          <div className="flex max-w-[200px] flex-col gap-3">
                            <div className="aspect-square w-full overflow-hidden rounded-lg bg-slate-100">
                              {p.images[0] ? (
                                <img
                                  src={resolveApiAssetUrl(p.images[0])}
                                  alt=""
                                  className="h-full w-full object-cover"
                                />
                              ) : (
                                <div className="flex h-full items-center justify-center text-xs text-slate-400">
                                  No image
                                </div>
                              )}
                            </div>
                            <Link
                              to={`/products/${p.id}`}
                              className="font-semibold leading-snug text-slate-900 hover:text-blue-700 hover:underline"
                            >
                              {p.name}
                            </Link>
                            <button
                              type="button"
                              onClick={() => remove(p.id)}
                              className="self-start text-xs font-medium text-rose-600 hover:underline"
                            >
                              Remove
                            </button>
                          </div>
                        )}
                      </th>
                    )
                  })}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {baseRows.map((row) => (
                  <tr key={row.label}>
                    <th
                      scope="row"
                      className="sticky left-0 z-10 border-r border-slate-200 bg-white px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500"
                    >
                      {row.label}
                    </th>
                    {columns.map(({ id, result }) => (
                      <td key={id} className="px-4 py-3 text-slate-800">
                        {result.data ? row.get(result.data) : '—'}
                      </td>
                    ))}
                  </tr>
                ))}
                {specKeys.map((key) => (
                  <tr key={key}>
                    <th
                      scope="row"
                      className="sticky left-0 z-10 border-r border-slate-200 bg-white px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500"
                    >
                      {key}
                    </th>
                    {columns.map(({ id, result }) => {
                      const p = result.data
                      const v = p?.specs[key]?.trim()
                      return (
                        <td key={id} className="px-4 py-3 text-slate-800">
                          {v ? v : '—'}
                        </td>
                      )
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  )
}
