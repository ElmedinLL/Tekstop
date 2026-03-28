import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as cartApi from '../lib/cartApi'
import { queryKeys } from '../lib/queryKeys'
import { useCartStore } from '../store/useCartStore'

/** Cart changes often; keep a shorter stale window than global defaults. */
const cartStaleTimeMs = 30_000

export function useCartQuery() {
  return useQuery({
    queryKey: queryKeys.cart,
    queryFn: cartApi.getCart,
    staleTime: cartStaleTimeMs,
  })
}

function usePatchCartCache() {
  const queryClient = useQueryClient()
  const syncGuestLinesFromCart = useCartStore((s) => s.syncGuestLinesFromCart)

  return (cart: Awaited<ReturnType<typeof cartApi.getCart>>) => {
    queryClient.setQueryData(queryKeys.cart, cart)
    syncGuestLinesFromCart(cart)
  }
}

export function useAddCartItemMutation() {
  const patch = usePatchCartCache()
  const bumpBadge = useCartStore((s) => s.bumpBadge)

  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: number; quantity: number }) =>
      cartApi.addCartItem(productId, quantity),
    onSuccess: (data) => {
      patch(data)
      bumpBadge()
    },
  })
}

export function useUpdateCartLineMutation() {
  const patch = usePatchCartCache()

  return useMutation({
    mutationFn: ({
      productId,
      cartItemId,
      quantity,
    }: {
      productId: number
      cartItemId: number
      quantity: number
    }) => cartApi.updateCartLineQuantity(productId, cartItemId, quantity),
    onSuccess: patch,
  })
}

export function useRemoveCartLineMutation() {
  const patch = usePatchCartCache()

  return useMutation({
    mutationFn: ({ productId, cartItemId }: { productId: number; cartItemId: number }) =>
      cartApi.removeCartLine(productId, cartItemId),
    onSuccess: patch,
  })
}

export function useClearCartMutation() {
  const queryClient = useQueryClient()
  const syncGuestLinesFromCart = useCartStore((s) => s.syncGuestLinesFromCart)

  return useMutation({
    mutationFn: cartApi.clearCart,
    onSuccess: (data) => {
      queryClient.setQueryData(queryKeys.cart, data)
      syncGuestLinesFromCart(data)
      useCartStore.setState({ guestLinesSnapshot: [] })
    },
  })
}
