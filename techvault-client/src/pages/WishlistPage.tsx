import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  notifyMovedToCart,
  notifyWishlistActionError,
  notifyWishlistItemRemoved,
} from '../lib/notifications'
import { ProductGrid } from '../components/ProductGrid'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { fetchWishlist, removeFromWishlist } from '../lib/wishlist'
import { useCartStore } from '../store/useCartStore'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

export function WishlistPage() {
  const queryClient = useQueryClient()
  const addItem = useCartStore((s) => s.addItem)
  const setCartDrawerOpen = useCartStore((s) => s.setCartDrawerOpen)

  const { data, isPending, isError } = useQuery({
    queryKey: ['wishlist'],
    queryFn: fetchWishlist,
  })

  const removeMu = useMutation({
    mutationFn: removeFromWishlist,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['wishlist'] })
      notifyWishlistItemRemoved()
    },
    onError: () => notifyWishlistActionError('Could not remove item.'),
  })

  const moveMu = useMutation({
    mutationFn: async (productId: number) => {
      await addItem(productId, 1)
      await removeFromWishlist(productId)
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['wishlist'] })
      notifyMovedToCart()
      setCartDrawerOpen(true)
    },
    onError: () => notifyWishlistActionError('Could not move to cart.'),
  })

  const items = data ?? []

  if (isError) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10">
        <h1 className="text-2xl font-semibold text-slate-900">Wishlist</h1>
        <p className="mt-4 text-sm text-rose-600">Could not load your wishlist.</p>
      </div>
    )
  }

  if (isPending) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10">
        <h1 className="text-2xl font-semibold text-slate-900">Wishlist</h1>
        <p className="mt-1 text-sm text-slate-600">Items you save for later appear here.</p>
        <div className="mt-8">
          <ProductGrid loading skeletonCount={8} />
        </div>
      </div>
    )
  }

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10">
        <h1 className="text-2xl font-semibold text-slate-900">Wishlist</h1>
        <p className="mt-1 text-sm text-slate-600">Items you save for later appear here.</p>
        <div className="mt-10 rounded-2xl border border-dashed border-slate-200 bg-white px-6 py-16 text-center shadow-sm">
          <p className="text-base font-medium text-slate-800">Your wishlist is empty</p>
          <p className="mt-2 text-sm text-slate-600">Save products you like from product pages to find them quickly.</p>
          <Link
            to="/search"
            className="mt-6 inline-flex rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700"
          >
            Browse products
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-10">
      <h1 className="text-2xl font-semibold text-slate-900">Wishlist</h1>
      <p className="mt-1 text-sm text-slate-600">{items.length} saved item{items.length === 1 ? '' : 's'}</p>

      <div className="mt-8 grid grid-cols-1 gap-4 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 lg:gap-6 xl:grid-cols-4">
        {items.map((item) => {
          const busy = removeMu.isPending || moveMu.isPending
          const removing = removeMu.isPending && removeMu.variables === item.productId
          const moving = moveMu.isPending && moveMu.variables === item.productId

          return (
            <article
              key={item.productId}
              className="flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
            >
              <Link to={`/products/${item.productId}`} className="block aspect-[4/3] bg-slate-100">
                {item.imageUrl ? (
                  <img
                    src={resolveApiAssetUrl(item.imageUrl)}
                    alt=""
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center text-slate-400">No image</div>
                )}
              </Link>
              <div className="flex flex-1 flex-col p-4">
                <Link
                  to={`/products/${item.productId}`}
                  className="line-clamp-2 font-medium text-slate-900 hover:text-blue-600 hover:underline"
                >
                  {item.name}
                </Link>
                <p className="mt-2 text-lg font-semibold text-slate-800">{formatMoney(item.price)}</p>
                <div className="mt-4 flex flex-col gap-2 sm:flex-row">
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => moveMu.mutate(item.productId)}
                    className="flex-1 rounded-lg bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 disabled:opacity-50"
                  >
                    {moving ? 'Moving…' : 'Move to cart'}
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => removeMu.mutate(item.productId)}
                    className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
                  >
                    {removing ? 'Removing…' : 'Remove'}
                  </button>
                </div>
              </div>
            </article>
          )
        })}
      </div>
    </div>
  )
}
