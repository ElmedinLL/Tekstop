import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const RECENTLY_VIEWED_MAX = 10

type RecentlyViewedStore = {
  /** Most recent first, max {@link RECENTLY_VIEWED_MAX} */
  ids: number[]
  /** Pushes `id` to the front; de-duplicates; trims to max length. */
  recordView: (id: number) => void
  clear: () => void
}

export const useRecentlyViewedStore = create<RecentlyViewedStore>()(
  persist(
    (set, get) => ({
      ids: [],

      recordView: (id) => {
        if (!Number.isFinite(id) || id <= 0) {
          return
        }
        const without = get().ids.filter((x) => x !== id)
        set({ ids: [id, ...without].slice(0, RECENTLY_VIEWED_MAX) })
      },

      clear: () => set({ ids: [] }),
    }),
    { name: 'techvault-recently-viewed' },
  ),
)
