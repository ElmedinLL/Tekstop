import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { CartDto, GuestCartLine } from '../types/cart'

type CartStore = {
  cartDrawerOpen: boolean
  setCartDrawerOpen: (open: boolean) => void
  /** Persisted guest lines used for merge after login. */
  guestLinesSnapshot: GuestCartLine[]
  /** When cart API returns a guest session, mirror lines for merge-after-login. */
  syncGuestLinesFromCart: (cart: CartDto) => void
  clearGuestSnapshot: () => void
  badgeBump: number
  bumpBadge: () => void
}

function linesFromCart(cart: CartDto): GuestCartLine[] {
  return cart.lines.map((l) => ({ productId: l.productId, quantity: l.quantity }))
}

export const useCartStore = create<CartStore>()(
  persist(
    (set) => ({
      cartDrawerOpen: false,
      setCartDrawerOpen: (open) => set({ cartDrawerOpen: open }),
      guestLinesSnapshot: [],
      badgeBump: 0,

      bumpBadge: () => set((s) => ({ badgeBump: s.badgeBump + 1 })),

      syncGuestLinesFromCart: (cart) => {
        if (!cart.isAuthenticated) {
          set({ guestLinesSnapshot: linesFromCart(cart) })
        }
      },

      clearGuestSnapshot: () => set({ guestLinesSnapshot: [] }),
    }),
    {
      name: 'techvault-cart',
      partialize: (state) => ({ guestLinesSnapshot: state.guestLinesSnapshot }),
    },
  ),
)
