import { Helmet } from 'react-helmet-async'
import { formatPageTitle, SITE_NAME } from '../lib/siteMeta'

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
  /** Product detail route: Open Graph, Twitter Card, canonical URL */
  productOpenGraph?: ProductOpenGraph
}

export function Seo({ title, description, noindex, productOpenGraph }: SeoProps) {
  return (
    <Helmet prioritizeSeoTags>
      <title>{formatPageTitle(title)}</title>
      {description ? <meta name="description" content={description} /> : null}
      {noindex ? <meta name="robots" content="noindex,nofollow" /> : null}
      {productOpenGraph ? (
        <>
          <link rel="canonical" href={productOpenGraph.url} />
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
    </Helmet>
  )
}
