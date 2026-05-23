/**
 * Helpers for JSON-LD (safe script content for Helmets script tags).
 */

export function stringifyJsonLd(data: Record<string, unknown> | Record<string, unknown>[]) {
  const payload = JSON.stringify(data)
    .replace(/</g, '\\u003c')
    .replace(/\u2028/g, '\\u2028')
    .replace(/\u2029/g, '\\u2029')
  return payload
}

export function organizationJsonLd(siteName: string, siteUrl: string) {
  return {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: siteName,
    url: siteUrl,
    sameAs: [] as string[],
  }
}

export function productListingJsonLdForCourseParity(product: {
  name: string
  description?: string | null
  url: string
  /** ISO datetime when available (pairs with storefront product `createdAtUtc`). */
  datePublished?: string
  /** Course-schema parity tag; emits as CreativeWork-aligned `dateCreated` for JSON-LD. */
  dateCreated?: string | null
  /** Storefront brand / provider Organization (Course `provider`). */
  brand?: { '@type': 'Organization'; name: string; url: string }
}) {
  return {
    '@context': 'https://schema.org',
    '@type': 'Product',
    name: product.name,
    description: product.description ?? undefined,
    url: product.url,
    datePublished: product.datePublished ?? undefined,
    ...(product.dateCreated && { dateCreated: product.dateCreated }),
    ...(product.brand && { brand: product.brand }),
  }
}
