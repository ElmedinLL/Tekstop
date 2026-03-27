/** Matches `ProductReviewDto` from the API (camelCase JSON). */
export type ProductReview = {
  id: number
  rating: number
  comment: string | null
  createdAtUtc: string
  authorDisplayName: string
}

/** Matches `MyReviewStatusDto`. */
export type MyReviewStatus = {
  purchased: boolean
  review: ProductReview | null
}
