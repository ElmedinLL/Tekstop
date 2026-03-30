import { useQueries, useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { ProductCard } from '../components/ProductCard'
import { ProductGrid } from '../components/ProductGrid'
import { Seo } from '../components/Seo'
import { fetchProductDetail, productDetailQueryRetry } from '../hooks/useProduct'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { useRecentlyViewedStore } from '../store/useRecentlyViewedStore'
import type { CategoryListItem } from '../types/category'
import type { PagedProductsResponse, ProductListItem } from '../types/product'

async function fetchFeaturedProducts(take: number): Promise<ProductListItem[]> {
  const { data } = await api.get<ProductListItem[]>('/products/featured', { params: { take } })
  return Array.isArray(data) ? data : []
}

async function fetchProductsSorted(sort: string, pageSize: number): Promise<PagedProductsResponse> {
  const { data } = await api.get<PagedProductsResponse>('/products', {
    params: {
      sort,
      page: 1,
      pageSize,
    },
    paramsSerializer: {
      indexes: null,
    },
  })
  return data
}

async function fetchCategories(): Promise<CategoryListItem[]> {
  const { data } = await api.get<CategoryListItem[]>('/categories')
  return Array.isArray(data) ? data : []
}

export function HomePage() {
  const recentIds = useRecentlyViewedStore((s) => s.ids)

  const recentProductQueries = useQueries({
    queries: recentIds.map((id) => ({
      queryKey: ['product', id] as const,
      queryFn: () => fetchProductDetail(id),
      enabled: id > 0,
      staleTime: 60_000,
      retry: productDetailQueryRetry,
    })),
  })

  const {
    data: featured,
    isPending: featuredPending,
    isError: featuredError,
  } = useQuery({
    queryKey: ['featured', 8],
    queryFn: () => fetchFeaturedProducts(8),
    staleTime: 60_000,
  })

  const {
    data: categories,
    isPending: categoriesPending,
    isError: categoriesError,
  } = useQuery({
    queryKey: ['categories'],
    queryFn: fetchCategories,
    staleTime: 10 * 60_000,
  })

  const {
    data: bestSellersPage,
    isPending: bestSellersPending,
    isError: bestSellersError,
  } = useQuery({
    queryKey: ['products', 'best-sellers'],
    queryFn: () => fetchProductsSorted('stock_desc', 8),
    staleTime: 60_000,
  })

  const {
    data: newArrivalsPage,
    isPending: newArrivalsPending,
    isError: newArrivalsError,
  } = useQuery({
    queryKey: ['products', 'new-arrivals'],
    queryFn: () => fetchProductsSorted('newest', 8),
    staleTime: 60_000,
  })

  const featuredItems = featured ?? []
  const bestSellersItems = bestSellersPage?.items ?? []
  const newArrivalsItems = newArrivalsPage?.items ?? []

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <Seo
        title="Home"
        description="Shop featured tech, best sellers, and new arrivals. Browse categories and find your next upgrade on TechVault."
      />
      {/* Hero */}
      <section className="relative overflow-hidden rounded-2xl border border-slate-200 bg-gradient-to-br from-blue-600 via-sky-600 to-slate-900 p-0 text-white shadow-sm">
        <div className="grid gap-6 p-8 md:grid-cols-[1.3fr,0.7fr] md:items-center">
          <div className="space-y-4">
            <div className="inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-1 text-xs font-medium">
              Featured tech marketplace
            </div>
            <h1 className="text-3xl font-semibold tracking-tight md:text-4xl">Find your next upgrade</h1>
            <p className="text-white/90">
              Browse hand-picked products, discover categories, and catch the latest deals and restocks.
            </p>
            <div className="flex flex-wrap gap-3 pt-1">
              <Link
                to="/search?q=laptop"
                className="rounded-lg bg-white px-4 py-2 text-sm font-semibold text-slate-900 shadow-sm hover:bg-slate-100"
              >
                Search laptops
              </Link>
              <Link to="/category/laptops" className="rounded-lg bg-white/10 px-4 py-2 text-sm font-semibold hover:bg-white/15">
                Shop Laptops
              </Link>
            </div>
          </div>

          <div className="space-y-2">
            <p className="text-sm font-medium text-white/90">Featured</p>
            <div className="grid grid-cols-2 gap-3">
              {featuredPending &&
                Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="aspect-[4/3] rounded-xl bg-white/10" />
                ))}
              {!featuredPending &&
                featuredItems.slice(0, 4).map((p) => (
                  <Link
                    key={p.id}
                    to={`/products/${p.id}`}
                    className="group rounded-xl bg-white/10 p-2 transition hover:bg-white/15"
                  >
                    <div className="aspect-[4/3] overflow-hidden rounded-lg bg-white/10">
                      {p.imageUrl ? (
                        <img
                          src={resolveApiAssetUrl(p.imageUrl)}
                          alt=""
                          className="h-full w-full object-cover transition group-hover:scale-[1.03]"
                        />
                      ) : (
                        <div className="h-full w-full bg-white/5" />
                      )}
                    </div>
                    <div className="mt-2 line-clamp-2 text-xs font-semibold text-white/95">{p.name}</div>
                  </Link>
                ))}
            </div>
          </div>
        </div>
      </section>

      {/* Recently viewed */}
      {recentIds.length > 0 && (
        <section className="mt-10" aria-labelledby="recently-viewed-heading">
          <div className="flex items-end justify-between gap-4">
            <div>
              <h2 id="recently-viewed-heading" className="text-xl font-semibold tracking-tight text-slate-900">
                Recently viewed
              </h2>
              <p className="mt-1 text-sm text-slate-600">Pick up where you left off.</p>
            </div>
          </div>

          <div className="relative mt-6">
            <div
              className="-mx-4 flex gap-4 overflow-x-auto px-4 pb-2 pt-1 [scrollbar-width:thin] sm:mx-0 sm:px-0"
              role="list"
              aria-label="Recently viewed products"
            >
              {recentIds.map((rid, i) => {
                const q = recentProductQueries[i]
                if (q?.isPending) {
                  return (
                    <div
                      key={rid}
                      role="listitem"
                      className="w-[min(100%,280px)] shrink-0 snap-start sm:w-64"
                    >
                      <div className="h-[340px] animate-pulse rounded-xl border border-slate-200 bg-white shadow-sm" />
                    </div>
                  )
                }
                if (!q?.data) {
                  return null
                }
                const p = q.data
                const thumb = p.images[0] ? resolveApiAssetUrl(p.images[0]) : null
                return (
                  <div
                    key={rid}
                    role="listitem"
                    className="w-[min(100%,280px)] shrink-0 snap-start sm:w-64"
                  >
                    <ProductCard
                      id={p.id}
                      name={p.name}
                      imageUrl={thumb}
                      price={p.price}
                      compareAtPrice={p.compareAtPrice ?? null}
                      stockQuantity={p.stock}
                      productTo={`/products/${p.id}`}
                    />
                  </div>
                )
              })}
            </div>
          </div>
        </section>
      )}

      {/* Categories */}
      <section className="mt-10">
        <div className="flex items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold tracking-tight text-slate-900">Categories</h2>
            <p className="mt-1 text-sm text-slate-600">Shop by what you need.</p>
          </div>
          <Link to="/search" className="text-sm font-medium text-blue-700 hover:underline">
            View all
          </Link>
        </div>

        {categoriesPending && (
          <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="h-20 animate-pulse rounded-xl border border-slate-200 bg-white" />
            ))}
          </div>
        )}

        {!categoriesPending && (
          <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {(categories ?? [])
              .filter((c) => c.isActive)
              .sort((a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name))
              .slice(0, 12)
              .map((c) => (
                <Link
                  key={c.id}
                  to={`/category/${encodeURIComponent(c.slug)}`}
                  className="group rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:shadow-md"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <h3 className="truncate text-sm font-semibold text-slate-900">{c.name}</h3>
                      <p className="mt-1 text-xs text-slate-500">{c.productCount} product{c.productCount === 1 ? '' : 's'}</p>
                    </div>
                    <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-50 text-blue-700 transition group-hover:bg-blue-100">
                      →
                    </div>
                  </div>
                </Link>
              ))}
            {!categoriesError && (categories?.filter((c) => c.isActive).length ?? 0) === 0 && (
              <p className="col-span-full mt-6 text-center text-sm text-slate-600">No categories available.</p>
            )}
          </div>
        )}
      </section>

      {/* Best sellers */}
      <section className="mt-10">
        <div className="flex items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold tracking-tight text-slate-900">Best sellers</h2>
            <p className="mt-1 text-sm text-slate-600">Most popular right now.</p>
          </div>
        </div>

        <div className="mt-6">
          <ProductGrid loading={bestSellersPending} skeletonCount={8}>
            {!bestSellersPending &&
              bestSellersItems.map((p) => (
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
              ))}
          </ProductGrid>
        </div>
      </section>

      {/* New arrivals */}
      <section className="mt-10">
        <div className="flex items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold tracking-tight text-slate-900">New arrivals</h2>
            <p className="mt-1 text-sm text-slate-600">Freshly added to the catalog.</p>
          </div>
        </div>

        <div className="mt-6">
          <ProductGrid loading={newArrivalsPending} skeletonCount={8}>
            {!newArrivalsPending &&
              newArrivalsItems.map((p) => (
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
              ))}
          </ProductGrid>
        </div>
      </section>

      {(featuredError || categoriesError || bestSellersError || newArrivalsError) && (
        <div className="mt-8 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          Some sections could not load. If the backend is not running, start it and refresh the page.
        </div>
      )}
    </div>
  )
}
