import { useRef } from 'react'
import { resolveApiAssetUrl } from '../../lib/assetUrl'

const MAX_IMAGES = 5

type PendingPreview = {
  file: File
  previewUrl: string
}

type AdminProductImagesFieldProps = {
  imageUrls: string[]
  onImageUrlsChange: (urls: string[]) => void
  pending: PendingPreview[]
  onPendingAdd: (files: File[]) => void
  onPendingRemove: (index: number) => void
  productId: number | null
  disabled?: boolean
}

export function AdminProductImagesField({
  imageUrls,
  onImageUrlsChange,
  pending,
  onPendingAdd,
  onPendingRemove,
  productId,
  disabled,
}: AdminProductImagesFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const totalCount = imageUrls.length + pending.length
  const canAddMore = totalCount < MAX_IMAGES && !disabled

  const removeSaved = (index: number) => {
    onImageUrlsChange(imageUrls.filter((_, i) => i !== index))
  }

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []).filter((f) => f.type.startsWith('image/'))
    if (files.length === 0) {
      e.target.value = ''
      return
    }
    const room = MAX_IMAGES - totalCount
    const slice = files.slice(0, Math.max(0, room))
    if (productId == null) {
      onPendingAdd(slice)
    } else {
      // Parent handles upload; we still pass files up
      onPendingAdd(slice)
    }
    e.target.value = ''
  }

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs font-medium text-slate-600">
          Images ({totalCount}/{MAX_IMAGES})
        </p>
        <label
          className={`cursor-pointer rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium shadow-sm ${
            canAddMore ? 'text-slate-800 hover:bg-slate-50' : 'cursor-not-allowed opacity-50'
          }`}
        >
          Add images
          <input
            ref={inputRef}
            type="file"
            accept="image/jpeg,image/png,image/gif,image/webp,image/bmp,image/svg+xml"
            multiple
            className="sr-only"
            disabled={!canAddMore}
            onChange={onFileChange}
          />
        </label>
      </div>

      {productId == null && pending.length > 0 && (
        <p className="text-xs text-amber-800">
          Images will upload automatically after the product is created (first save).
        </p>
      )}

      <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-5">
        {imageUrls.map((url, index) => (
          <li
            key={`saved-${url}-${index}`}
            className="relative overflow-hidden rounded-lg border border-slate-200 bg-slate-100 aspect-square"
          >
            <img
              src={resolveApiAssetUrl(url)}
              alt=""
              className="h-full w-full object-cover"
            />
            <button
              type="button"
              disabled={disabled}
              onClick={() => removeSaved(index)}
              className="absolute right-1 top-1 rounded bg-slate-900/70 px-1.5 py-0.5 text-xs font-medium text-white hover:bg-slate-900 disabled:opacity-50"
            >
              Remove
            </button>
          </li>
        ))}
        {pending.map((p, index) => (
          <li
            key={`pending-${p.previewUrl}`}
            className="relative overflow-hidden rounded-lg border border-dashed border-blue-300 bg-blue-50/50 aspect-square"
          >
            <img src={p.previewUrl} alt="" className="h-full w-full object-cover opacity-90" />
            <span className="absolute bottom-1 left-1 rounded bg-blue-600/90 px-1 text-[10px] font-semibold uppercase text-white">
              New
            </span>
            <button
              type="button"
              disabled={disabled}
              onClick={() => onPendingRemove(index)}
              className="absolute right-1 top-1 rounded bg-slate-900/70 px-1.5 py-0.5 text-xs font-medium text-white hover:bg-slate-900 disabled:opacity-50"
            >
              Remove
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}
