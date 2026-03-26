import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { api } from '../lib/api'
import type { ProductDetail } from '../types/product'
import { ProductImageGallery } from '../components/ProductImageGallery'

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

  const [quantity, setQuantity] = useState(1)

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

  const handleAddToCart = () => {
    if (!inStock) {
      return
    }
    // Wire to cart context / API when available
    console.info('Add to cart', { productId: product.id, quantity })
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
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
              type="button"
              disabled={!inStock}
              onClick={handleAddToCart}
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
        <p className="mt-4 text-slate-600">
          There are no reviews yet. When review submission is available, ratings and comments will appear here.
        </p>
        <div className="mt-8 rounded-lg border border-dashed border-slate-200 bg-slate-50/80 p-8 text-center text-sm text-slate-500">
          <p className="font-medium text-slate-700">Be the first to review this product</p>
          <p className="mt-2">Review posting will be enabled in a future update.</p>
        </div>
      </section>
    </div>
  )
}
