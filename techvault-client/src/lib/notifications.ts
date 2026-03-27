import { isAxiosError } from 'axios'
import { toast } from 'sonner'

export { toast }

/** Cart — product added from PDP or similar. */
export function notifyCartAdded(productName?: string) {
  toast.success(productName ? `Added “${productName}” to cart` : 'Added to cart')
}

export function notifyCartError(message = 'Could not add to cart.') {
  toast.error(message)
}

/** Wishlist — toggle success (saved vs removed). */
export function notifyWishlistToggled(inWishlist: boolean) {
  toast.success(inWishlist ? 'Saved to wishlist' : 'Removed from wishlist')
}

export function notifyWishlistError(message = 'Could not update wishlist.') {
  toast.error(message)
}

export function notifyWishlistAuthRequired() {
  toast.error('Sign in to save items to your wishlist.', {
    description: 'Use Sign in in the header.',
  })
}

/** Wishlist page / profile — item removed from list only. */
export function notifyWishlistItemRemoved() {
  toast.success('Removed from wishlist')
}

export function notifyMovedToCart() {
  toast.success('Moved to cart')
}

export function notifyWishlistActionError(message: string) {
  toast.error(message)
}

/** Generic errors (forms, checkout, API). */
export function notifyError(message: string, description?: string) {
  toast.error(message, description ? { description } : undefined)
}

/** Best-effort message from Axios error bodies (string or ProblemDetails-like). */
export function messageFromUnknownError(err: unknown, fallback: string): string {
  if (isAxiosError(err)) {
    const d = err.response?.data
    if (typeof d === 'string' && d.trim()) {
      return d
    }
    if (d && typeof d === 'object') {
      const title = (d as { title?: unknown }).title
      if (typeof title === 'string' && title.trim()) {
        return title
      }
    }
    if (err.message) {
      return err.message
    }
  }
  return fallback
}
