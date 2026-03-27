import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { api } from '../lib/api'
import type { CartDto, GuestCartLine } from '../types/cart'

type CartStore = {
  serverCart: CartDto | null
  /** Persisted guest lines used for merge after login. */
  guestLinesSnapshot: GuestCartLine[]
  badgeBump: number
  fetchCart: () => Promise<void>
  addItem: (productId: number, quantity: number) => Promise<void>
  updateLineQuantity: (productId: number, cartItemId: number, quantity: number) => Promise<void>
  removeLine: (productId: number, cartItemId: number) => Promise<void>
  clearServerCart: () => Promise<void>
  /** POST /cart/merge then refresh; clears persisted guest snapshot. */
  syncAfterAuth: () => Promise<void>
  itemCount: () => number
  bumpBadge: () => void
}

function linesFromCart(cart: CartDto): GuestCartLine[] {
  return cart.lines.map((l) => ({ productId: l.productId, quantity: l.quantity }))
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      serverCart: null,
      guestLinesSnapshot: [],
      badgeBump: 0,

      bumpBadge: () => set((s) => ({ badgeBump: s.badgeBump + 1 })),

      itemCount: () => get().serverCart?.totalItemCount ?? 0,

      fetchCart: async () => {
        const { data } = await api.get<CartDto>('/cart')
        set({ serverCart: data })
        if (!data.isAuthenticated) {
          set({ guestLinesSnapshot: linesFromCart(data) })
        }
      },

      addItem: async (productId, quantity) => {
        const { data } = await api.post<CartDto>('/cart/items', { productId, quantity })
        set({ serverCart: data })
        if (!data.isAuthenticated) {
          set({ guestLinesSnapshot: linesFromCart(data) })
        }
        get().bumpBadge()
      },

      updateLineQuantity: async (productId, cartItemId, quantity) => {
        const routeId = cartItemId > 0 ? cartItemId : productId
        const { data } = await api.put<CartDto>(`/cart/items/${routeId}`, { quantity })
        set({ serverCart: data })
        if (!data.isAuthenticated) {
          set({ guestLinesSnapshot: linesFromCart(data) })
        }
      },

      removeLine: async (productId, cartItemId) => {
        const routeId = cartItemId > 0 ? cartItemId : productId
        const { data } = await api.delete<CartDto>(`/cart/items/${routeId}`)
        set({ serverCart: data })
        if (!data.isAuthenticated) {
          set({ guestLinesSnapshot: linesFromCart(data) })
        }
      },

      clearServerCart: async () => {
        const { data } = await api.delete<CartDto>('/cart')
        set({ serverCart: data, guestLinesSnapshot: [] })
      },

      syncAfterAuth: (() => {
        let merging = false
        return async () => {
          if (merging) {
            return
          }
          merging = true
          try {
            const lines = get().guestLinesSnapshot
            if (lines.length > 0) {
              try {
                const { data } = await api.post<CartDto>('/cart/merge', { lines })
                set({ serverCart: data, guestLinesSnapshot: [] })
                return
              } catch {
                // If merge fails (e.g. expired session), still try to load cart.
              }
            }
            set({ guestLinesSnapshot: [] })
            await get().fetchCart()
          } finally {
            merging = false
          }
        }
      })(),
    }),
    {
      name: 'techvault-cart',
      partialize: (state) => ({ guestLinesSnapshot: state.guestLinesSnapshot }),
    },
  ),
)
