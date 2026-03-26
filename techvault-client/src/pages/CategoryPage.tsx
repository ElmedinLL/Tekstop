import { Link, useParams, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { ProductCard } from '../components/ProductCard'
import { ProductFilters } from '../components/ProductFilters'
import { ProductGrid } from '../components/ProductGrid'
import { api } from '../lib/api'
import { buildCategoryProductsApiParams, normalizeSortParam } from '../lib/categoryPageParams'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { ProductFilterKeys } from '../lib/productFilterParams'
import type { CategoryDetail } from '../types/category'
import type { PagedProductsResponse } from '../types/product'

const SORT_OPTIONS = [
  { value: 'newest', label: 'Newest' },
  { value: 'price_asc', label: 'Price: Low to high' },
  { value: 'price_desc', label: 'Price: High to low' },
  { value: 'stock_desc', label: 'Popular' },
] as const

async function fetchCategory(slug: string): Promise<CategoryDetail> {
  const { data } = await api.get<CategoryDetail>(`/categories/${encodeURIComponent(slug)}`)
  return data
}

async function fetchCategoryProducts(slug: string, searchParams: URLSearchParams): Promise<PagedProductsResponse> {
  const params = buildCategoryProductsApiParams(searchParams)
  const { data } = await api.get<PagedProductsResponse>(`/products/category/${encodeURIComponent(slug)}`, {
    params,
    paramsSerializer: {
      indexes: null,
    },
  })
  return data
}

export function CategoryPage() {
  const { slug: slugParam } = useParams<{ slug: string }>()
  const slug = slugParam?.trim() ?? ''
  const validSlug = slug.length > 0

  const [searchParams, setSearchParams] = useSearchParams()
  const sortInUrl = searchParams.get(ProductFilterKeys.sort)
  const normalizedSort = normalizeSortParam(sortInUrl)
  const sortSelectValue = SORT_OPTIONS.some((o) => o.value === normalizedSort) ? normalizedSort : 'newest'

  const {
    data: category,
    isPending: categoryPending,
    isError: categoryError,
    error: categoryErr,
  } = useQuery({
    queryKey: ['category', slug],
    queryFn: () => fetchCategory(slug),
    enabled: validSlug,
  })

  const {
    data: pageData,
    isPending: productsPending,
    isError: productsError,
    error: productsErr,
  } = useQuery({
    queryKey: ['category-products', slug, searchParams.toString()],
    queryFn: () => fetchCategoryProducts(slug, searchParams),
    enabled: validSlug && !!category,
  })

  const categoryNotFound =
    categoryError && axios.isAxiosError(categoryErr) && categoryErr.response?.status === 404

  const productsNotFound =
    productsError && axios.isAxiosError(productsErr) && productsErr.response?.status === 404

  const totalPages = pageData?.totalPages ?? 0
  const page =
    pageData?.page ??
    (Number.parseInt(searchParams.get(ProductFilterKeys.page) ?? '1', 10) || 1)

  const onSortChange = (value: string) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev)
        next.set(ProductFilterKeys.sort, value)
        next.set(ProductFilterKeys.page, '1')
        return next
      },
      { replace: false },
    )
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || (totalPages > 0 && nextPage > totalPages)) {
      return
    }
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev)
        next.set(ProductFilterKeys.page, String(nextPage))
        return next
      },
      { replace: false },
    )
  }

  const showGridLoading = categoryPending || productsPending

  const items = pageData?.items ?? []

  if (!validSlug) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-xl font-semibold text-slate-900">Invalid category</h1>
        <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
          Back to home
        </Link>
      </div>
    )
  }

  if (categoryNotFound) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-xl font-semibold text-slate-900">Category not found</h1>
        <p className="mt-2 text-slate-600">This category does not exist or was removed.</p>
        <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
          Back to home
        </Link>
      </div>
    )
  }

  if (productsNotFound) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <h1 className="text-xl font-semibold text-slate-900">Nothing here</h1>
        <p className="mt-2 text-slate-600">Products for this category could not be loaded.</p>
        <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
          Back to home
        </Link>
      </div>
    )
  }

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
            {category?.name ?? 'Category'}
          </li>
        </ol>
      </nav>

      <div className="flex flex-col gap-8 lg:flex-row lg:items-start lg:gap-10">
        <ProductFilters hideCategories className="lg:max-w-none lg:w-72 lg:shrink-0" />

        <div className="min-w-0 flex-1 space-y-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
            <div>
              <h1 className="text-2xl font-semibold tracking-tight text-slate-900 md:text-3xl">
                {category?.name ?? 'Products'}
              </h1>
              {category && (
                <p className="mt-1 text-sm text-slate-500">
                  {category.productCount} {category.productCount === 1 ? 'product' : 'products'}
                  {pageData != null && ` · Showing ${items.length} on this page`}
                </p>
              )}
            </div>

            <div className="flex items-center gap-2">
              <label htmlFor="category-sort" className="text-sm text-slate-600 whitespace-nowrap">
                Sort by
              </label>
              <select
                id="category-sort"
                value={sortSelectValue}
                onChange={(e) => onSortChange(e.target.value)}
                className="min-w-[12rem] rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                {SORT_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <ProductGrid loading={showGridLoading} skeletonCount={8}>
            {!showGridLoading
              ? items.map((p) => (
                  <ProductCard
                    key={p.id}
                    id={p.id}
                    name={p.name}
                    imageUrl={p.imageUrl ? resolveApiAssetUrl(p.imageUrl) : null}
                    price={p.price}
                    compareAtPrice={p.compareAtPrice}
                    stockQuantity={p.stockQuantity}
                    productTo={`/products/${p.id}`}
                  />
                ))
              : null}
          </ProductGrid>

          {!showGridLoading && items.length === 0 && (
            <p className="rounded-lg border border-slate-200 bg-white py-12 text-center text-slate-600">
              No products match your filters.
            </p>
          )}

          {!showGridLoading && totalPages > 1 && (
            <nav className="flex flex-wrap items-center justify-center gap-2 border-t border-slate-200 pt-8" aria-label="Pagination">
              <button
                type="button"
                onClick={() => goToPage(page - 1)}
                disabled={page <= 1}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Previous
              </button>
              <span className="px-2 text-sm text-slate-600">
                Page {page} of {totalPages}
              </span>
              <button
                type="button"
                onClick={() => goToPage(page + 1)}
                disabled={page >= totalPages}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Next
              </button>
            </nav>
          )}
        </div>
      </div>
    </div>
  )
}
