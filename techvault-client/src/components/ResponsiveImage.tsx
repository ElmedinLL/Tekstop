import type { HTMLAttributes } from 'react'

type ResponsiveImageProps = {
  src: string
  alt: string
  width: number
  height: number
  sizes?: string
  priority?: boolean
  className?: string
} & Pick<HTMLAttributes<HTMLImageElement>, 'onError'>

/**
 * P233: Vite SPA alternative to next/image — explicit dimensions, decoding, optional priority (fetchpriority).
 */
export function ResponsiveImage({
  src,
  alt,
  width,
  height,
  sizes,
  priority,
  className,
  onError,
}: ResponsiveImageProps) {
  return (
    <img
      src={src}
      alt={alt}
      width={width}
      height={height}
      sizes={sizes}
      decoding="async"
      loading={priority ? 'eager' : 'lazy'}
      fetchPriority={priority ? 'high' : 'auto'}
      className={className}
      onError={onError}
    />
  )
}
