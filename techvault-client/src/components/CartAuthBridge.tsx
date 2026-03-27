import { useEffect, useRef } from 'react'
import { useAuth } from '../auth/AuthContext'
import { useCartStore } from '../store/useCartStore'

/** Loads cart on startup and merges persisted guest lines once after login. */
export function CartAuthBridge() {
  const { isAuthenticated, isInitializing } = useAuth()
  const fetchCart = useCartStore((s) => s.fetchCart)
  const syncAfterAuth = useCartStore((s) => s.syncAfterAuth)
  const prevAuth = useRef(false)

  useEffect(() => {
    if (isInitializing) {
      return
    }
    void fetchCart()
  }, [isInitializing, fetchCart])

  useEffect(() => {
    if (isInitializing) {
      return
    }
    if (isAuthenticated && !prevAuth.current) {
      void syncAfterAuth()
    }
    prevAuth.current = isAuthenticated
  }, [isAuthenticated, isInitializing, syncAfterAuth])

  return null
}
