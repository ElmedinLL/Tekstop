import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { api } from '../lib/api'
import { fetchProductReviews } from '../lib/reviews'
import type { ProductDetail } from '../types/product'
import { notifyCartAdded, notifyCartError } from '../lib/notifications'
import { ProductImageGallery } from '../components/ProductImageGallery'
import { ReviewForm } from '../components/ReviewForm'
import { ReviewList } from '../components/ReviewList'
import { flyToCart } from '../lib/flyToCart'
import { useProductReviewStatus } from '../hooks/useProductReviewStatus'
import { useAddCartItemMutation } from '../hooks/useCart'
import { Seo } from '../components/Seo'
import { getSiteOrigin, productDescriptionForMeta } from '../lib/siteMeta'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { useCartStore } from '../store/useCartStore'

async function fetchProductById(id: number): Promise<ProductDetail> {
  const { data } = await api.get<ProductDetail>(`/products/${id}`)
  return data
}

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

export function ProductDetailPage() {
  const { id: idParam } = useParams<{ id: string }>()
  const id = idParam != null ? Number.parseInt(idParam, 10) : Number.NaN
  const validId = Number.isFinite(id) && id > 0

  const {
    data: product,
    isPending,
    isError,
    error,
  } = useQuery({
    queryKey: ['product', id],
    queryFn: () => fetchProductById(id),
    enabled: validId,
  })

  const reviewsQuery = useQuery({
    queryKey: ['product', id, 'reviews'],
    queryFn: () => fetchProductReviews(id),
    enabled: validId && !!product,
  })

  const myReviewStatus = useProductReviewStatus(id)

  const reviewsForList = useMemo(() => {
    const rows = reviewsQuery.data ?? []
    const mineId = myReviewStatus.data?.review?.id
    if (mineId == null) {
      return rows
    }
    return rows.filter((r) => r.id !== mineId)
  }, [reviewsQuery.data, myReviewStatus.data?.review?.id])

  const [quantity, setQuantity] = useState(1)
  const addToCartBtnRef = useRef<HTMLButtonElement>(null)
  const addCartMu = useAddCartItemMutation()
  const setCartDrawerOpen = useCartStore((s) => s.setCartDrawerOpen)

  useEffect(() => {
    setQuantity(1)
  }, [id])

  const maxQty = product?.stock ?? 0
  const inStock = maxQty > 0

  useEffect(() => {
    if (product && quantity > maxQty) {
      setQuantity(Math.max(1, maxQty))
    }
  }, [product, quantity, maxQty])

  const specEntries = useMemo(() => {
    if (!product?.specs) {
      return []
    }
    return Object.entries(product.specs)
  }, [product])

  const notFound =
    isError && axios.isAxiosError(error) && error.response?.status === 404

  if (!validId) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <Seo title="Invalid product" description="This product link is not valid." />
        <h1 className="text-xl font-semibold text-slate-900">Invalid product</h1>
        <p className="mt-2 text-slate-600">Check the link and try again.</p>
        <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
          Back to home
        </Link>
      </div>
    )
  }

  if (isPending) {
    return (
      <div className="mx-auto max-w-6xl px-4 py-10">
        <Seo title="Product" description="Loading product details…" />
        <div className="animate-pulse space-y-6 lg:grid lg:grid-cols-2 lg:gap-10 lg:space-y-0">
          <div className="aspect-[4/3] rounded-xl bg-slate-200" />
          <div className="space-y-4">
            <div className="h-8 w-3/4 rounded bg-slate-200" />
            <div className="h-6 w-1/4 rounded bg-slate-200" />
            <div className="h-24 rounded bg-slate-200" />
          </div>
        </div>
      </div>
    )
  }

  if (notFound || isError) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <Seo title="Product not found" description="This product is unavailable or may have been removed." />
        <h1 className="text-xl font-semibold text-slate-900">Product not found</h1>
        <p className="mt-2 text-slate-600">This product may have been removed or is unavailable.</p>
        <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
          Back to home
        </Link>
      </div>
    )
  }

  if (!product) {
    return null
  }

  const handleAddToCart = async () => {
    if (!inStock || !product) {
      return
    }
    try {
      await addCartMu.mutateAsync({ productId: product.id, quantity })
      flyToCart(addToCartBtnRef.current)
      setCartDrawerOpen(true)
      notifyCartAdded(product.name)
    } catch {
      notifyCartError()
    }
  }

  const pageUrl = `${getSiteOrigin()}/products/${product.id}`
  const metaDesc = productDescriptionForMeta(product)
  const ogImage =
    product.images.length > 0 ? resolveApiAssetUrl(product.images[0]) : undefined

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <Seo
        title={product.name}
        description={metaDesc}
        productOpenGraph={{
          title: product.name,
          description: metaDesc,
          url: pageUrl,
          image: ogImage || undefined,
        }}
      />
      <nav className="mb-8 text-sm text-slate-500" aria-label="Breadcrumb">
        <ol className="flex flex-wrap items-center gap-2">
          <li>
            <Link to="/" className="hover:text-blue-600">
              Home
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li>
            <Link to={`/category/${encodeURIComponent(product.category.slug)}`} className="hover:text-blue-600">
              {product.category.name}
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li className="font-medium text-slate-800">{product.name}</li>
        </ol>
      </nav>

      <div className="grid gap-10 lg:grid-cols-2 lg:gap-12">
        <ProductImageGallery images={product.images} productName={product.name} />

        <div className="space-y-6">
          <header>
            <h1 className="text-2xl font-semibold tracking-tight text-slate-900 md:text-3xl">{product.name}</h1>
            <p className="mt-1 text-sm text-slate-500">SKU: {product.sku}</p>
            <p className="mt-4 text-3xl font-semibold text-slate-900">{formatMoney(product.price)}</p>
            {!inStock && (
              <p className="mt-2 inline-block rounded bg-slate-200 px-2 py-1 text-sm font-medium text-slate-700">
                Out of stock
              </p>
            )}
          </header>

          {product.description && (
            <div>
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Description</h2>
              <p className="mt-2 whitespace-pre-wrap text-slate-700">{product.description}</p>
            </div>
          )}

          {specEntries.length > 0 && (
            <div>
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Specifications</h2>
              <dl className="mt-3 divide-y divide-slate-200 rounded-lg border border-slate-200">
                {specEntries.map(([key, value]) => (
                  <div key={key} className="grid grid-cols-1 gap-1 px-3 py-2.5 sm:grid-cols-3 sm:gap-4">
                    <dt className="text-sm font-medium text-slate-600">{key}</dt>
                    <dd className="text-sm text-slate-900 sm:col-span-2">{value}</dd>
                  </div>
                ))}
              </dl>
            </div>
          )}

          <div className="flex flex-wrap items-end gap-4 border-t border-slate-200 pt-6">
            <div>
              <label htmlFor="product-qty" className="block text-sm font-medium text-slate-700">
                Quantity
              </label>
              <input
                id="product-qty"
                type="number"
                min={1}
                max={Math.max(1, maxQty)}
                disabled={!inStock}
                value={inStock ? Math.min(quantity, maxQty) : 1}
                onChange={(e) => {
                  const v = Number.parseInt(e.target.value, 10)
                  if (Number.isNaN(v)) {
                    return
                  }
                  setQuantity(Math.min(Math.max(1, v), maxQty))
                }}
                className="mt-1 w-24 rounded-lg border border-slate-300 px-3 py-2 text-slate-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:bg-slate-100"
              />
            </div>
            <button
              ref={addToCartBtnRef}
              type="button"
              disabled={!inStock}
              onClick={() => void handleAddToCart()}
              className={`rounded-lg px-6 py-2.5 text-sm font-semibold shadow-sm transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 ${
                inStock ? 'bg-blue-600 text-white hover:bg-blue-700' : 'cursor-not-allowed bg-slate-200 text-slate-500'
              }`}
            >
              {inStock ? 'Add to cart' : 'Unavailable'}
            </button>
          </div>
        </div>
      </div>

      <section className="mt-16 border-t border-slate-200 pt-12" aria-labelledby="reviews-heading">
        <h2 id="reviews-heading" className="text-lg font-semibold text-slate-900">
          Customer reviews
        </h2>

        <div className="mt-8 max-w-xl">
          <ReviewForm productId={product.id} />
        </div>

        {reviewsQuery.isPending && (
          <p className="mt-8 text-sm text-slate-500" role="status">
            Loading reviews…
          </p>
        )}

        {reviewsQuery.isError && (
          <p className="mt-8 text-sm text-rose-600">Could not load reviews.</p>
        )}

        {!reviewsQuery.isPending && !reviewsQuery.isError && (
          <>
            <ReviewList key={product.id} reviews={reviewsForList} pageSize={8} queryParamKey="reviewPage" className="mt-8" />
            {(reviewsQuery.data?.length ?? 0) === 0 && !myReviewStatus.data?.review ? (
              <p className="mt-8 text-sm text-slate-600">No reviews yet. Be the first to share your experience.</p>
            ) : null}
          </>
        )}
      </section>
    </div>
  )
}
