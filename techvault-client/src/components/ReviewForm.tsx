import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from '../lib/notifications'
import { isAxiosError } from 'axios'
import { useAuth } from '../auth/AuthContext'
import { createProductReview } from '../lib/reviews'
import { useProductReviewStatus } from '../hooks/useProductReviewStatus'
import type { ProductReview } from '../types/review'

function StarIcon({ className = '' }: { className?: string }) {
  return (
    <svg className={`h-8 w-8 shrink-0 ${className}`} viewBox="0 0 20 20" aria-hidden>
      <path
        fill="currentColor"
        d="M10 1.5l2.6 5.3 5.9.9-4.3 4.2 1 5.9L10 15.9 4.8 17.8l1-5.9L1.5 7.7l5.9-.9L10 1.5z"
      />
    </svg>
  )
}

/** Read-only 1–5 stars for review lists and “your review”. */
export function StarRatingDisplay({ rating, size = 'md' }: { rating: number; size?: 'sm' | 'md' }) {
  const n = Math.min(5, Math.max(1, Math.round(rating)))
  const dim = size === 'sm' ? 'h-4 w-4' : 'h-8 w-8'
  return (
    <div className="flex gap-0.5" role="img" aria-label={`${n} out of 5 stars`}>
      {Array.from({ length: 5 }, (_, i) => (
        <StarIcon key={i} className={`${dim} ${i < n ? 'text-amber-400' : 'text-slate-200'}`} />
      ))}
    </div>
  )
}

function formatReviewWhen(iso: string) {
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function ExistingReviewCard({ review }: { review: ProductReview }) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <h3 className="text-sm font-semibold text-slate-900">Your review</h3>
      <div className="mt-3">
        <StarRatingDisplay rating={review.rating} />
      </div>
      {review.comment ? (
        <p className="mt-3 whitespace-pre-wrap text-sm text-slate-700">{review.comment}</p>
      ) : (
        <p className="mt-3 text-sm italic text-slate-500">No written comment.</p>
      )}
      <p className="mt-3 text-xs text-slate-500">Submitted {formatReviewWhen(review.createdAtUtc)}</p>
    </div>
  )
}

type ReviewFormProps = {
  productId: number
}

export function ReviewForm({ productId }: ReviewFormProps) {
  const queryClient = useQueryClient()
  const { isAuthenticated, isInitializing } = useAuth()
  const { data: status, isPending, isError, isFetching } = useProductReviewStatus(productId)

  const [rating, setRating] = useState(0)
  const [hoverRating, setHoverRating] = useState(0)
  const [comment, setComment] = useState('')

  const displayStars = hoverRating || rating

  const createMu = useMutation({
    mutationFn: () =>
      createProductReview(productId, {
        rating,
        comment: comment.trim() === '' ? null : comment.trim(),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['product', productId, 'reviews', 'me'] })
      void queryClient.invalidateQueries({ queryKey: ['product', productId, 'reviews'] })
      void queryClient.invalidateQueries({ queryKey: ['product', productId] })
      toast.success('Thanks for your review.')
      setRating(0)
      setComment('')
    },
    onError: (err: unknown) => {
      if (isAxiosError(err)) {
        const d = err.response?.data
        const text =
          typeof d === 'string'
            ? d
            : d && typeof d === 'object' && 'title' in d
              ? String((d as { title?: string }).title)
              : err.message
        toast.error(text || 'Could not submit review.')
        return
      }
      toast.error('Could not submit review.')
    },
  })

  if (isInitializing) {
    return null
  }

  if (!isAuthenticated) {
    return null
  }

  if (isPending || isFetching) {
    return (
      <div className="rounded-xl border border-slate-200 bg-slate-50/80 p-5">
        <div className="h-4 w-40 animate-pulse rounded bg-slate-200" />
        <div className="mt-4 h-10 w-56 animate-pulse rounded bg-slate-200" />
        <div className="mt-4 h-24 animate-pulse rounded bg-slate-200" />
      </div>
    )
  }

  if (isError) {
    return null
  }

  if (!status?.purchased) {
    return null
  }

  if (status.review) {
    return <ExistingReviewCard review={status.review} />
  }

  const submit = (e: FormEvent) => {
    e.preventDefault()
    if (rating < 1 || rating > 5) {
      toast.error('Please choose a star rating.')
      return
    }
    createMu.mutate()
  }

  return (
    <form
      onSubmit={submit}
      className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
      aria-labelledby="review-form-heading"
    >
      <h3 id="review-form-heading" className="text-sm font-semibold text-slate-900">
        Write a review
      </h3>
      <p className="mt-1 text-xs text-slate-500">Only customers who bought this item can leave a review.</p>

      <div className="mt-4">
        <span id="rating-label" className="block text-sm font-medium text-slate-700">
          Rating
        </span>
        <div
          className="mt-2 flex gap-1"
          role="group"
          aria-labelledby="rating-label"
          onMouseLeave={() => setHoverRating(0)}
        >
          {[1, 2, 3, 4, 5].map((n) => (
            <button
              key={n}
              type="button"
              className="rounded p-0.5 transition hover:scale-105 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
              aria-label={`${n} out of 5 stars`}
              aria-pressed={rating === n}
              onMouseEnter={() => setHoverRating(n)}
              onClick={() => setRating(n)}
            >
              <StarIcon className={n <= displayStars ? 'text-amber-400' : 'text-slate-200'} />
            </button>
          ))}
        </div>
      </div>

      <div className="mt-4">
        <label htmlFor="review-comment" className="block text-sm font-medium text-slate-700">
          Comment <span className="font-normal text-slate-500">(optional)</span>
        </label>
        <textarea
          id="review-comment"
          name="comment"
          rows={4}
          maxLength={4000}
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Share your experience with this product…"
          className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm placeholder:text-slate-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
        <p className="mt-1 text-xs text-slate-500">{comment.length} / 4000</p>
      </div>

      <button
        type="submit"
        disabled={createMu.isPending}
        className="mt-4 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 disabled:opacity-50"
      >
        {createMu.isPending ? 'Submitting…' : 'Submit review'}
      </button>
    </form>
  )
}
