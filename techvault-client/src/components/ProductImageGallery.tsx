import { useEffect, useMemo, useRef, useState, type KeyboardEvent as ReactKeyboardEvent } from 'react'
import { resolveApiAssetUrl } from '../lib/assetUrl'

export type ProductImageGalleryProps = {
  images: string[]
  productName: string
}

function clampIndex(i: number, length: number) {
  if (length <= 0) return 0
  const mod = i % length
  return mod < 0 ? mod + length : mod
}

export function ProductImageGallery({ images, productName }: ProductImageGalleryProps) {
  const resolved = useMemo(() => images.map((u) => resolveApiAssetUrl(u)).filter(Boolean), [images])
  const [activeIndex, setActiveIndex] = useState(0)
  const [lightboxOpen, setLightboxOpen] = useState(false)

  const thumbsRef = useRef<Array<HTMLButtonElement | null>>([])
  const closeButtonRef = useRef<HTMLButtonElement | null>(null)

  useEffect(() => {
    setActiveIndex(0)
  }, [images])

  useEffect(() => {
    if (!lightboxOpen) return
    const t = window.setTimeout(() => closeButtonRef.current?.focus(), 0)
    return () => window.clearTimeout(t)
  }, [lightboxOpen])

  useEffect(() => {
    if (!lightboxOpen) return

    const onKeyDown = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault()
        setLightboxOpen(false)
        return
      }

      if (e.key === 'ArrowLeft') {
        e.preventDefault()
        setActiveIndex((prev) => clampIndex(prev - 1, resolved.length))
        return
      }

      if (e.key === 'ArrowRight') {
        e.preventDefault()
        setActiveIndex((prev) => clampIndex(prev + 1, resolved.length))
        return
      }
    }

    window.addEventListener('keydown', onKeyDown as unknown as EventListener)
    return () => window.removeEventListener('keydown', onKeyDown as unknown as EventListener)
  }, [lightboxOpen, resolved.length])

  if (resolved.length === 0) {
    return (
      <div className="aspect-[4/3] w-full rounded-xl border border-slate-200 bg-slate-100 flex items-center justify-center text-slate-500">
        No images
      </div>
    )
  }

  const safeIndex = clampIndex(activeIndex, resolved.length)
  const mainSrc = resolved[safeIndex] ?? resolved[0]

  const setActiveAndFocusThumb = (nextIndex: number) => {
    const ni = clampIndex(nextIndex, resolved.length)
    setActiveIndex(ni)
    requestAnimationFrame(() => thumbsRef.current[ni]?.focus())
  }

  const onThumbKeyDown = (e: ReactKeyboardEvent<HTMLButtonElement>, index: number) => {
    if (e.key === 'ArrowLeft') {
      e.preventDefault()
      setActiveAndFocusThumb(index - 1)
      return
    }
    if (e.key === 'ArrowRight') {
      e.preventDefault()
      setActiveAndFocusThumb(index + 1)
      return
    }
    if (e.key === 'Home') {
      e.preventDefault()
      setActiveAndFocusThumb(0)
      return
    }
    if (e.key === 'End') {
      e.preventDefault()
      setActiveAndFocusThumb(resolved.length - 1)
      return
    }
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault()
      setActiveIndex(index)
      setLightboxOpen(true)
    }
  }

  return (
    <div className="space-y-3">
      <button
        type="button"
        onClick={() => setLightboxOpen(true)}
        aria-label={`Open image ${safeIndex + 1} of ${resolved.length} in lightbox`}
        className="block w-full"
      >
        <div className="group overflow-hidden rounded-xl border border-slate-200 bg-white">
          <img
            src={mainSrc}
            alt={productName}
            className="aspect-[4/3] w-full object-contain bg-slate-50 transition-transform duration-200 group-hover:scale-110"
          />
        </div>
      </button>

      {resolved.length > 1 && (
        <ul className="flex flex-wrap gap-2" role="list" aria-label="Image thumbnails">
          {resolved.map((src, i) => {
            const isActive = safeIndex === i
            return (
              <li key={src + i}>
                <button
                  ref={(el) => {
                    thumbsRef.current[i] = el
                  }}
                  type="button"
                  onClick={() => setActiveIndex(i)}
                  onKeyDown={(e) => onThumbKeyDown(e, i)}
                  className={`overflow-hidden rounded-lg border-2 bg-white p-0.5 transition ${
                    isActive ? 'border-blue-600 ring-1 ring-blue-600' : 'border-transparent hover:border-slate-300'
                  }`}
                  aria-label={`View image ${i + 1} of ${resolved.length}`}
                  aria-current={isActive ? 'true' : undefined}
                  tabIndex={isActive ? 0 : 0}
                >
                  <img src={src} alt="" className="h-16 w-16 object-cover" />
                </button>
              </li>
            )
          })}
        </ul>
      )}

      {lightboxOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 p-4">
          <div
            role="dialog"
            aria-modal="true"
            aria-label="Image lightbox"
            className="relative flex w-full max-w-5xl flex-col items-center"
          >
            <button
              ref={closeButtonRef}
              type="button"
              onClick={() => setLightboxOpen(false)}
              className="absolute right-2 top-2 rounded-lg bg-white/95 px-3 py-1.5 text-sm font-semibold text-slate-900 shadow-sm hover:bg-white"
              aria-label="Close lightbox"
            >
              Close
            </button>

            {resolved.length > 1 && (
              <>
                <button
                  type="button"
                  onClick={() => setActiveIndex((prev) => clampIndex(prev - 1, resolved.length))}
                  className="absolute left-2 top-1/2 -translate-y-1/2 rounded-full bg-white/95 p-3 text-slate-900 shadow-sm hover:bg-white"
                  aria-label="Previous image"
                >
                  ←
                </button>
                <button
                  type="button"
                  onClick={() => setActiveIndex((prev) => clampIndex(prev + 1, resolved.length))}
                  className="absolute right-2 top-1/2 -translate-y-1/2 rounded-full bg-white/95 p-3 text-slate-900 shadow-sm hover:bg-white"
                  aria-label="Next image"
                >
                  →
                </button>
              </>
            )}

            <div className="w-full overflow-hidden rounded-2xl border border-white/20 bg-slate-950">
              <img
                src={mainSrc}
                alt={productName}
                className="max-h-[80vh] w-full object-contain bg-slate-950"
              />
            </div>

            <div className="mt-3 text-center text-sm text-white/90">
              Image {safeIndex + 1} of {resolved.length}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

