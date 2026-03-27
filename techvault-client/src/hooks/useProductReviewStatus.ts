import { useQuery } from '@tanstack/react-query'
import { useAuth } from '../auth/AuthContext'
import { fetchMyReviewStatus } from '../lib/reviews'

/** Signed-in purchase/review state for a product; shared query with `ReviewForm`. */
export function useProductReviewStatus(productId: number) {
  const { isAuthenticated, isInitializing } = useAuth()

  return useQuery({
    queryKey: ['product', productId, 'reviews', 'me'],
    queryFn: () => fetchMyReviewStatus(productId),
    enabled: isAuthenticated && !isInitializing && productId > 0,
  })
}
