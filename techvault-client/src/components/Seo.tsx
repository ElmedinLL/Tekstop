import { Helmet } from 'react-helmet-async'
import { formatPageTitle, SITE_NAME } from '../lib/siteMeta'
import { stringifyJsonLd } from '../lib/jsonLd'

export type ProductOpenGraph = {
  title: string
  description: string
  url: string
  image?: string
}

type SeoProps = {
  title: string
  description?: string
  noindex?: boolean
  /** P232 / product OG URL — canonical when set */
  canonicalHref?: string
  /** P231: JSON-LD blobs (already serialized-safe via stringify helper in Helmet bodies) */
  jsonLdScripts?: readonly Record<string, unknown>[]
  /** Product detail route: Open Graph, Twitter Card */
  productOpenGraph?: ProductOpenGraph
}

export function Seo({
  title,
  description,
  noindex,
  canonicalHref,
  jsonLdScripts,
  productOpenGraph,
}: SeoProps) {
  const canonical = (canonicalHref?.trim() || productOpenGraph?.url?.trim() || '').trim()

  return (
    <Helmet prioritizeSeoTags>
      <title>{formatPageTitle(title)}</title>
      {description ? <meta name="description" content={description} /> : null}
      {noindex ? <meta name="robots" content="noindex,nofollow" /> : null}
      {canonical ? <link rel="canonical" href={canonical} /> : null}
      {productOpenGraph ? (
        <>
          <meta property="og:type" content="product" />
          <meta property="og:site_name" content={SITE_NAME} />
          <meta property="og:title" content={productOpenGraph.title} />
          <meta property="og:description" content={productOpenGraph.description} />
          <meta property="og:url" content={productOpenGraph.url} />
          {productOpenGraph.image ? <meta property="og:image" content={productOpenGraph.image} /> : null}
          <meta name="twitter:card" content="summary_large_image" />
          <meta name="twitter:title" content={productOpenGraph.title} />
          <meta name="twitter:description" content={productOpenGraph.description} />
          {productOpenGraph.image ? <meta name="twitter:image" content={productOpenGraph.image} /> : null}
        </>
      ) : null}
      {jsonLdScripts?.map((blob, idx) => (
        <script key={idx} type="application/ld+json">
          {stringifyJsonLd(blob)}
        </script>
      ))}
    </Helmet>
  )
}
