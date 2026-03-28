import type { ProductDetail } from '../types/product'

export const SITE_NAME = 'TechVault'

export function formatPageTitle(pageTitle: string): string {
  const t = pageTitle.trim()
  if (!t) {
    return SITE_NAME
  }
  return `${t} | ${SITE_NAME}`
}

/**
 * Canonical / Open Graph base URL. Set `VITE_SITE_URL` in production (e.g. `https://shop.example.com`)
 * so share previews use absolute URLs. Falls back to `window.location.origin` in the browser.
 */
export function getSiteOrigin(): string {
  const fromEnv = import.meta.env.VITE_SITE_URL as string | undefined
  if (fromEnv?.trim()) {
    return fromEnv.trim().replace(/\/$/, '')
  }
  if (typeof window !== 'undefined') {
    return window.location.origin
  }
  return ''
}

export function truncateMetaText(text: string, maxLen: number): string {
  const oneLine = text.replace(/\s+/g, ' ').trim()
  if (oneLine.length <= maxLen) {
    return oneLine
  }
  return `${oneLine.slice(0, maxLen - 1).trim()}…`
}

export function productDescriptionForMeta(product: ProductDetail): string {
  const short = product.shortDescription?.trim()
  if (short) {
    return truncateMetaText(short, 160)
  }
  const plain = (product.description ?? '')
    .replace(/<[^>]*>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
  if (plain) {
    return truncateMetaText(plain, 160)
  }
  return truncateMetaText(`Shop ${product.name} at ${SITE_NAME}.`, 160)
}
