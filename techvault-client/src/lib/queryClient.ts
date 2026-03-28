import { QueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { shouldThrowQueryErrorToBoundary } from './queryError'

const minute = 60 * 1000

/**
 * Default cache behavior:
 * - staleTime: data is considered fresh for 2 minutes (no background refetch).
 * - gcTime: unused cache kept 15 minutes after last subscriber unmounts (formerly cacheTime).
 * - refetchOnWindowFocus: false so switching tabs does not constantly refetch (less “flicker”).
 * - throwOnError: only escalate to the root error boundary when there is no cached data yet; a failed
 *   background refetch must not unmount the whole app (otherwise pages go blank after a few seconds).
 * - retry: up to 2 retries for network/5xx; no retry for 4xx.
 */
export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 2 * minute,
        gcTime: 15 * minute,
        retry: (failureCount, error) => {
          if (isAxiosError(error)) {
            const status = error.response?.status
            if (status !== undefined && status >= 400 && status < 500) {
              return false
            }
          }
          return failureCount < 2
        },
        retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 10_000),
        // Avoid refetching every time the browser tab regains focus (often feels like the page “keeps refreshing”).
        refetchOnWindowFocus: false,
        throwOnError: (error, query) => {
          if (query.state.data !== undefined) {
            return false
          }
          return shouldThrowQueryErrorToBoundary(error)
        },
      },
      mutations: {
        retry: 0,
      },
    },
  })
}
