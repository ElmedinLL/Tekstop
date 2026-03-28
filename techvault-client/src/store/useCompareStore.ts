import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const COMPARE_MAX = 3

type CompareStore = {
  ids: number[]
  has: (id: number) => boolean
  /** @returns added | duplicate | full */
  add: (id: number) => 'added' | 'duplicate' | 'full'
  remove: (id: number) => void
  /** @returns added | removed | full */
  toggle: (id: number) => 'added' | 'removed' | 'full'
  clear: () => void
}

export const useCompareStore = create<CompareStore>()(
  persist(
    (set, get) => ({
      ids: [],

      has: (id) => get().ids.includes(id),

      add: (id) => {
        const { ids } = get()
        if (ids.includes(id)) {
          return 'duplicate'
        }
        if (ids.length >= COMPARE_MAX) {
          return 'full'
        }
        set({ ids: [...ids, id] })
        return 'added'
      },

      remove: (id) => set({ ids: get().ids.filter((x) => x !== id) }),

      toggle: (id) => {
        const ids = get().ids
        if (ids.includes(id)) {
          set({ ids: ids.filter((x) => x !== id) })
          return 'removed'
        }
        if (ids.length >= COMPARE_MAX) {
          return 'full'
        }
        set({ ids: [...ids, id] })
        return 'added'
      },

      clear: () => set({ ids: [] }),
    }),
    { name: 'techvault-compare' },
  ),
)
