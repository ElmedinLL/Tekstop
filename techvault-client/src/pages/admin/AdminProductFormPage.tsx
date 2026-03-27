import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useForm, type Resolver } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { AdminProductImagesField } from '../../components/admin/AdminProductImagesField'
import { ProductSpecsEditor, type SpecRow } from '../../components/admin/ProductSpecsEditor'
import { createProduct, fetchAdminProduct, updateProduct, uploadProductImage } from '../../lib/adminProduct'
import { fetchCategories } from '../../lib/categories'
import { messageFromUnknownError, notifyError } from '../../lib/notifications'
import { toast } from 'sonner'

const MAX_IMAGES = 5

type PendingImage = { file: File; previewUrl: string }

function slugifyName(name: string) {
  return (
    name
      .trim()
      .toLowerCase()
      .replace(/'/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '') || 'product'
  )
}

function specsFromRows(rows: SpecRow[]): Record<string, string> {
  const out: Record<string, string> = {}
  for (const r of rows) {
    const k = r.key.trim()
    if (!k) continue
    out[k] = r.value.trim()
  }
  return out
}

function rowsFromSpecs(specs: Record<string, string>): SpecRow[] {
  const entries = Object.entries(specs)
  return entries.length > 0 ? entries.map(([key, value]) => ({ key, value })) : [{ key: '', value: '' }]
}

function parseCompareAt(s: string): number | null {
  const t = s.trim()
  if (!t) return null
  const n = Number(t)
  return Number.isFinite(n) && n > 0 ? n : null
}

const formSchema = z
  .object({
    name: z.string().min(1, 'Name is required'),
    slug: z.string(),
    sku: z.string().min(1, 'SKU is required'),
    shortDescription: z.string().optional(),
    description: z.string().optional(),
    price: z.number().positive('Price must be greater than 0'),
    compareAtPrice: z.string().optional(),
    stock: z.number().int().min(0, 'Stock cannot be negative'),
    categoryId: z.number().int().positive('Choose a category'),
    brand: z.string().optional(),
    isPublished: z.boolean(),
    images: z.array(z.string()),
    specRows: z.array(z.object({ key: z.string(), value: z.string() })),
  })
  .refine(
    (data) => {
      const keys = data.specRows.map((r) => r.key.trim().toLowerCase()).filter(Boolean)
      return new Set(keys).size === keys.length
    },
    { message: 'Each spec name must be unique.', path: ['specRows'] },
  )

type FormValues = z.infer<typeof formSchema>

type SavePayload = {
  values: FormValues
  pending: PendingImage[]
  mode: 'create' | 'edit'
  productId: number
}

export function AdminProductFormPage() {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const slugTouchedRef = useRef(false)

  const isCreate = pathname.includes('/products/new')
  const editMatch = pathname.match(/\/admin\/products\/(\d+)\/edit/)
  const editId = editMatch ? Number.parseInt(editMatch[1], 10) : NaN

  const [pendingImages, setPendingImages] = useState<PendingImage[]>([])

  const categoriesQuery = useQuery({
    queryKey: ['categories'],
    queryFn: fetchCategories,
  })

  const productQuery = useQuery({
    queryKey: ['admin', 'product', editId],
    queryFn: () => fetchAdminProduct(editId),
    enabled: !isCreate && Number.isFinite(editId) && editId > 0,
  })

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema) as Resolver<FormValues>,
    defaultValues: {
      name: '',
      slug: '',
      sku: '',
      shortDescription: '',
      description: '',
      price: 0,
      compareAtPrice: '',
      stock: 0,
      categoryId: 0,
      brand: '',
      isPublished: true,
      images: [],
      specRows: [{ key: '', value: '' }],
    },
  })

  const { register, handleSubmit, reset, watch, setValue, getValues, formState } = form
  const images = watch('images')
  const categoryId = watch('categoryId')

  useEffect(() => {
    if (!isCreate || !categoriesQuery.data?.length || categoryId !== 0) return
    setValue('categoryId', categoriesQuery.data[0].id)
  }, [isCreate, categoriesQuery.data, categoryId, setValue])

  useEffect(() => {
    const p = productQuery.data
    if (!p || isCreate) return
    slugTouchedRef.current = true
    reset({
      name: p.name,
      slug: p.slug,
      sku: p.sku,
      shortDescription: p.shortDescription ?? '',
      description: p.description ?? '',
      price: p.price,
      compareAtPrice: p.compareAtPrice != null ? String(p.compareAtPrice) : '',
      stock: p.stock,
      categoryId: p.category.id,
      brand: p.brand ?? '',
      isPublished: p.isPublished,
      images: [...p.images],
      specRows: rowsFromSpecs(p.specs ?? {}),
    })
  }, [productQuery.data, isCreate, reset])

  const removePendingAt = useCallback((index: number) => {
    setPendingImages((prev) => {
      const row = prev[index]
      if (row) URL.revokeObjectURL(row.previewUrl)
      return prev.filter((_, i) => i !== index)
    })
  }, [])

  const addFiles = useCallback(
    async (files: File[]) => {
      const currentTotal = images.length + (isCreate ? pendingImages.length : 0)
      const room = MAX_IMAGES - currentTotal
      const slice = files.slice(0, Math.max(0, room))
      if (slice.length === 0) return

      if (isCreate) {
        setPendingImages((prev) => [
          ...prev,
          ...slice.map((file) => ({ file, previewUrl: URL.createObjectURL(file) })),
        ])
        return
      }

      for (const file of slice) {
        try {
          const res = await uploadProductImage(editId, file)
          setValue('images', [...getValues('images'), res.url])
        } catch (e) {
          notifyError(messageFromUnknownError(e, 'Image upload failed'))
        }
      }
    },
    [editId, getValues, images.length, isCreate, pendingImages.length, setValue],
  )

  const saveMutation = useMutation({
    mutationFn: async ({ values, pending, mode, productId }: SavePayload) => {
      const specs = specsFromRows(values.specRows)
      const compareAt = parseCompareAt(values.compareAtPrice ?? '')
      const slug = values.slug.trim() || slugifyName(values.name)
      const brand = values.brand?.trim() || null
      const shortDesc = values.shortDescription?.trim() || null
      const desc = values.description?.trim() || null
      const pendingSnapshot = [...pending]

      if (mode === 'create') {
        const created = await createProduct({
          name: values.name.trim(),
          slug,
          sku: values.sku.trim(),
          shortDescription: shortDesc,
          description: desc,
          price: values.price,
          compareAtPrice: compareAt,
          stock: values.stock,
          categoryId: values.categoryId,
          brand,
          isPublished: values.isPublished,
          images: [],
          specs,
        })

        const uploadedUrls: string[] = []
        for (const p of pendingSnapshot) {
          const res = await uploadProductImage(created.id, p.file)
          uploadedUrls.push(res.url)
        }

        if (uploadedUrls.length > 0) {
          await updateProduct(created.id, { images: uploadedUrls })
        }

        return { mode: 'created' as const, id: created.id, pendingToRevoke: pendingSnapshot }
      }

      await updateProduct(productId, {
        name: values.name.trim(),
        slug,
        sku: values.sku.trim(),
        shortDescription: shortDesc,
        description: desc,
        price: values.price,
        compareAtPrice: compareAt,
        stock: values.stock,
        categoryId: values.categoryId,
        brand,
        isPublished: values.isPublished,
        images: values.images,
        specs,
      })
      return { mode: 'updated' as const, id: productId, pendingToRevoke: [] as PendingImage[] }
    },
    onSuccess: (result) => {
      void queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
      void queryClient.invalidateQueries({ queryKey: ['admin', 'product', result.id] })
      result.pendingToRevoke.forEach((p) => URL.revokeObjectURL(p.previewUrl))
      setPendingImages([])
      if (result.mode === 'created') {
        toast.success('Product created.')
        navigate(`/admin/products/${result.id}/edit`, { replace: true })
      } else {
        toast.success('Product saved.')
        void productQuery.refetch()
      }
    },
    onError: (e) => {
      notifyError(messageFromUnknownError(e, 'Could not save product.'))
    },
  })

  const categories = categoriesQuery.data ?? []
  const specError =
    typeof formState.errors.specRows?.message === 'string' ? formState.errors.specRows.message : undefined

  const title = isCreate ? 'New product' : 'Edit product'
  const submitting = saveMutation.isPending

  const nameReg = register('name')
  const slugReg = register('slug')

  const defaultNameBlur = () => {
    if (!isCreate || slugTouchedRef.current) return
    const name = getValues('name')
    if (name.trim()) {
      setValue('slug', slugifyName(name))
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-8 pb-12">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">{title}</h1>
          <p className="mt-1 text-sm text-slate-600">
            {isCreate ? 'Create a product, then images upload after save.' : `Product #${editId}`}
          </p>
        </div>
        <Link
          to="/admin/products"
          className="text-sm font-medium text-blue-600 hover:underline"
        >
          ← Back to products
        </Link>
      </div>

      {!isCreate && productQuery.isPending && (
        <p className="text-sm text-slate-500">Loading product…</p>
      )}
      {!isCreate && productQuery.isError && (
        <p className="text-sm text-rose-600">Could not load this product.</p>
      )}

      {(isCreate || productQuery.data) && (
        <form
          className="space-y-8"
          onSubmit={handleSubmit((v: FormValues) =>
            saveMutation.mutate({
              values: v,
              pending: pendingImages,
              mode: isCreate ? 'create' : 'edit',
              productId: editId,
            }),
          )}
          noValidate
        >
          <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Basics</h2>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <label className="text-xs font-medium text-slate-600">Name</label>
                <input
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  {...nameReg}
                  onBlur={(e) => {
                    void nameReg.onBlur(e)
                    defaultNameBlur()
                  }}
                />
                {formState.errors.name && (
                  <p className="mt-1 text-xs text-rose-600">{formState.errors.name.message}</p>
                )}
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Slug</label>
                <input
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm font-mono"
                  {...slugReg}
                  onChange={(e) => {
                    slugTouchedRef.current = true
                    void slugReg.onChange(e)
                  }}
                />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">SKU</label>
                <input
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  {...register('sku')}
                />
                {formState.errors.sku && (
                  <p className="mt-1 text-xs text-rose-600">{formState.errors.sku.message}</p>
                )}
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Category</label>
                <select
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  {...register('categoryId', { valueAsNumber: true })}
                  disabled={!categories.length}
                >
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
                {formState.errors.categoryId && (
                  <p className="mt-1 text-xs text-rose-600">{formState.errors.categoryId.message}</p>
                )}
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Brand</label>
                <input
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  {...register('brand')}
                />
              </div>
              <div className="flex items-center gap-2 sm:col-span-2">
                <input id="pub" type="checkbox" className="h-4 w-4 rounded border-slate-300" {...register('isPublished')} />
                <label htmlFor="pub" className="text-sm text-slate-700">
                  Published on storefront
                </label>
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Descriptions</h2>
            <div>
              <label className="text-xs font-medium text-slate-600">Short description</label>
              <textarea
                rows={2}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                {...register('shortDescription')}
              />
            </div>
            <div>
              <label className="text-xs font-medium text-slate-600">Full description</label>
              <textarea
                rows={6}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                {...register('description')}
              />
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Pricing & inventory</h2>
            <div className="grid gap-4 sm:grid-cols-3">
              <div>
                <label className="text-xs font-medium text-slate-600">Price (USD)</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm tabular-nums"
                  {...register('price', { valueAsNumber: true })}
                />
                {formState.errors.price && (
                  <p className="mt-1 text-xs text-rose-600">{formState.errors.price.message}</p>
                )}
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Compare-at price (optional)</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="Leave empty for none"
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm tabular-nums"
                  {...register('compareAtPrice')}
                />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600">Stock</label>
                <input
                  type="number"
                  min="0"
                  step="1"
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm tabular-nums"
                  {...register('stock', { valueAsNumber: true })}
                />
                {formState.errors.stock && (
                  <p className="mt-1 text-xs text-rose-600">{formState.errors.stock.message}</p>
                )}
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Images</h2>
            <AdminProductImagesField
              imageUrls={images}
              onImageUrlsChange={(urls) => setValue('images', urls, { shouldDirty: true })}
              pending={pendingImages}
              onPendingAdd={(files) => void addFiles(files)}
              onPendingRemove={removePendingAt}
              productId={isCreate ? null : editId}
              disabled={submitting}
            />
          </section>

          <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Specs</h2>
            <ProductSpecsEditor
              rows={watch('specRows')}
              onChange={(rows) => setValue('specRows', rows, { shouldDirty: true, shouldValidate: true })}
              error={specError}
            />
          </section>

          <div className="flex flex-wrap gap-3">
            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? 'Saving…' : isCreate ? 'Create product' : 'Save changes'}
            </button>
            <Link
              to="/admin/products"
              className="rounded-lg border border-slate-300 bg-white px-5 py-2.5 text-sm font-medium text-slate-800 hover:bg-slate-50"
            >
              Cancel
            </Link>
          </div>
        </form>
      )}
    </div>
  )
}
