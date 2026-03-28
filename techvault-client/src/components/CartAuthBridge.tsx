import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef } from 'react'
import { useAuth } from '../auth/AuthContext'
import { useCartQuery } from '../hooks/useCart'
import * as cartApi from '../lib/cartApi'
import { queryKeys } from '../lib/queryKeys'
import { useCartStore } from '../store/useCartStore'

/** Ensures cart query runs after auth init, merges guest cart after login, and syncs guest snapshot from API. */
export function CartAuthBridge() {
  const { isAuthenticated, isInitializing } = useAuth()
  const queryClient = useQueryClient()
  const { data: cart } = useCartQuery()
  const syncGuestLinesFromCart = useCartStore((s) => s.syncGuestLinesFromCart)
  const prevAuth = useRef(false)

  useEffect(() => {
    if (cart) {
      syncGuestLinesFromCart(cart)
    }
  }, [cart, syncGuestLinesFromCart])

  useEffect(() => {
    if (isInitializing) {
      return
    }
    void queryClient.invalidateQueries({ queryKey: queryKeys.cart })
  }, [isInitializing, queryClient])

  useEffect(() => {
    if (isInitializing) {
      return
    }
    if (isAuthenticated && !prevAuth.current) {
      void (async () => {
        const lines = useCartStore.getState().guestLinesSnapshot
        if (lines.length > 0) {
          try {
            const data = await cartApi.mergeCart(lines)
            queryClient.setQueryData(queryKeys.cart, data)
            useCartStore.getState().clearGuestSnapshot()
            prevAuth.current = isAuthenticated
            return
          } catch {
            // If merge fails (e.g. expired session), still refresh cart.
          }
        }
        useCartStore.getState().clearGuestSnapshot()
        await queryClient.invalidateQueries({ queryKey: queryKeys.cart })
        prevAuth.current = isAuthenticated
      })()
    } else {
      prevAuth.current = isAuthenticated
    }
  }, [isAuthenticated, isInitializing, queryClient])

  return null
}
