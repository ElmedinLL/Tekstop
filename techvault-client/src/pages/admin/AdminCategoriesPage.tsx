import { useCallback, useEffect, useRef, useState, type ChangeEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import {
  createCategory,
  deleteCategory,
  fetchCategories,
  updateCategory,
  uploadCategoryImage,
} from '../../lib/categories'
import { messageFromUnknownError, toast } from '../../lib/notifications'
import { resolveApiAssetUrl } from '../../lib/assetUrl'
import type { CategoryListItem } from '../../types/category'

function slugifyName(name: string) {
  return (
    name
      .trim()
      .toLowerCase()
      .replace(/'/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '') || 'category'
  )
}

function CategoryThumb({ url }: { url: string | null | undefined }) {
  const [err, setErr] = useState(false)
  const src = url ? resolveApiAssetUrl(url) : ''
  if (!src || err) {
    return (
      <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-xs font-medium text-slate-400">
        —
      </div>
    )
  }
  return (
    <img
      src={src}
      alt=""
      className="h-12 w-12 shrink-0 rounded-lg object-cover ring-1 ring-slate-200"
      onError={() => setErr(true)}
    />
  )
}

type FormState = {
  name: string
  slug: string
  description: string
  imageUrl: string
  displayOrder: number
  isActive: boolean
}

const emptyForm = (): FormState => ({
  name: '',
  slug: '',
  description: '',
  imageUrl: '',
  displayOrder: 0,
  isActive: true,
})

export function AdminCategoriesPage() {
  const queryClient = useQueryClient()
  const slugTouchedRef = useRef(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const listQuery = useQuery({
    queryKey: ['categories'],
    queryFn: fetchCategories,
  })

  const [modalOpen, setModalOpen] = useState(false)
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create')
  const [editSlug, setEditSlug] = useState<string | null>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [uploadingImage, setUploadingImage] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<CategoryListItem | null>(null)

  const openCreate = () => {
    slugTouchedRef.current = false
    setModalMode('create')
    setEditSlug(null)
    setForm(emptyForm())
    setModalOpen(true)
  }

  const openEdit = (row: CategoryListItem) => {
    slugTouchedRef.current = true
    setModalMode('edit')
    setEditSlug(row.slug)
    setForm({
      name: row.name,
      slug: row.slug,
      description: row.description ?? '',
      imageUrl: row.imageUrl ?? '',
      displayOrder: row.displayOrder,
      isActive: row.isActive,
    })
    setModalOpen(true)
  }

  const closeModal = () => {
    setModalOpen(false)
    setEditSlug(null)
  }

  const saveMutation = useMutation({
    mutationFn: async () => {
      const body = {
        name: form.name.trim(),
        slug: form.slug.trim() || undefined,
        description: form.description.trim() || null,
        imageUrl: form.imageUrl.trim() || null,
        parentCategoryId: null as number | null,
        displayOrder: form.displayOrder,
        isActive: form.isActive,
      }
      if (!body.name) {
        throw new Error('Name is required.')
      }
      if (modalMode === 'create') {
        return createCategory(body)
      }
      if (!editSlug) {
        throw new Error('Missing category.')
      }
      return updateCategory(editSlug, body)
    },
    onSuccess: async () => {
      toast.success(modalMode === 'create' ? 'Category created.' : 'Category saved.')
      closeModal()
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
    },
    onError: (e: unknown) => {
      if (isAxiosError(e) && typeof e.response?.data === 'string' && e.response.data.trim()) {
        toast.error(e.response.data)
        return
      }
      toast.error(messageFromUnknownError(e, 'Could not save category.'))
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (slug: string) => deleteCategory(slug),
    onSuccess: async () => {
      toast.success('Category deleted.')
      setDeleteTarget(null)
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
    },
    onError: (e: unknown) => {
      if (isAxiosError(e) && typeof e.response?.data === 'string' && e.response.data.trim()) {
        toast.error(e.response.data)
        return
      }
      toast.error(messageFromUnknownError(e, 'Could not delete category.'))
    },
  })

  const onNameChange = (name: string) => {
    setForm((f) => {
      const next = { ...f, name }
      if (modalMode === 'create' && !slugTouchedRef.current) {
        next.slug = slugifyName(name)
      }
      return next
    })
  }

  const onSlugChange = (slug: string) => {
    slugTouchedRef.current = true
    setForm((f) => ({ ...f, slug }))
  }

  const onPickImage = useCallback(async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return
    setUploadingImage(true)
    try {
      const res = await uploadCategoryImage(file)
      setForm((f) => ({ ...f, imageUrl: res.url }))
      toast.success('Image uploaded.')
    } catch (err) {
      toast.error(messageFromUnknownError(err, 'Upload failed.'))
    } finally {
      setUploadingImage(false)
    }
  }, [])

  useEffect(() => {
    if (!modalOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeModal()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [modalOpen])

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Categories</h1>
          <p className="mt-1 text-sm text-slate-600">Manage storefront categories and hero images.</p>
        </div>
        <button
          type="button"
          onClick={openCreate}
          className="inline-flex items-center justify-center rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700"
        >
          New category
        </button>
      </div>

      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        {listQuery.isPending && (
          <div className="space-y-2 p-4">
            {[0, 1, 2, 3, 4].map((i) => (
              <div key={i} className="h-14 animate-pulse rounded-lg bg-slate-100" />
            ))}
          </div>
        )}

        {listQuery.isError && (
          <p className="p-6 text-sm text-rose-600">Could not load categories.</p>
        )}

        {listQuery.data && listQuery.data.length === 0 && (
          <p className="p-6 text-sm text-slate-600">No categories yet.</p>
        )}

        {listQuery.data && listQuery.data.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80">
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Image
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Name
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Slug
                  </th>
                  <th scope="col" className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Products
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Status
                  </th>
                  <th scope="col" className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {listQuery.data.map((row) => (
                  <tr key={row.id} className="bg-white hover:bg-slate-50/80">
                    <td className="px-4 py-3">
                      <CategoryThumb url={row.imageUrl} />
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-900">{row.name}</td>
                    <td className="px-4 py-3 font-mono text-xs text-slate-600">{row.slug}</td>
                    <td className="px-4 py-3 text-right tabular-nums text-slate-900">{row.productCount}</td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                          row.isActive ? 'bg-emerald-500/10 text-emerald-800' : 'bg-slate-200 text-slate-700'
                        }`}
                      >
                        {row.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex flex-wrap justify-end gap-2">
                        <button
                          type="button"
                          onClick={() => openEdit(row)}
                          className="text-sm font-medium text-blue-600 hover:underline"
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          onClick={() => setDeleteTarget(row)}
                          className="text-sm font-medium text-rose-600 hover:underline"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {modalOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4"
          role="presentation"
          onClick={closeModal}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="admin-category-modal-title"
            className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="admin-category-modal-title" className="text-lg font-semibold text-slate-900">
              {modalMode === 'create' ? 'New category' : 'Edit category'}
            </h2>

            <div className="mt-4 space-y-4">
              <div>
                <label htmlFor="cat-name" className="text-xs font-medium text-slate-700">
                  Name
                </label>
                <input
                  id="cat-name"
                  value={form.name}
                  onChange={(e) => onNameChange(e.target.value)}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  autoComplete="off"
                />
              </div>

              <div>
                <label htmlFor="cat-slug" className="text-xs font-medium text-slate-700">
                  Slug
                </label>
                <input
                  id="cat-slug"
                  value={form.slug}
                  onChange={(e) => onSlugChange(e.target.value)}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  autoComplete="off"
                />
              </div>

              <div>
                <label htmlFor="cat-desc" className="text-xs font-medium text-slate-700">
                  Description
                </label>
                <textarea
                  id="cat-desc"
                  rows={3}
                  value={form.description}
                  onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <div>
                <label htmlFor="cat-image-url" className="text-xs font-medium text-slate-700">
                  Image URL
                </label>
                <input
                  id="cat-image-url"
                  value={form.imageUrl}
                  onChange={(e) => setForm((f) => ({ ...f, imageUrl: e.target.value }))}
                  placeholder="/images/categories/…"
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  autoComplete="off"
                />
                <div className="mt-2 flex flex-wrap items-center gap-2">
                  <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={onPickImage} />
                  <button
                    type="button"
                    disabled={uploadingImage}
                    onClick={() => fileInputRef.current?.click()}
                    className="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-xs font-medium text-slate-800 shadow-sm hover:bg-slate-50 disabled:opacity-50"
                  >
                    {uploadingImage ? 'Uploading…' : 'Upload image'}
                  </button>
                  {form.imageUrl.trim() ? (
                    <span className="text-xs text-slate-500">Preview:</span>
                  ) : null}
                  {form.imageUrl.trim() ? (
                    <img
                      src={resolveApiAssetUrl(form.imageUrl.trim())}
                      alt=""
                      className="h-10 w-10 rounded border border-slate-200 object-cover"
                    />
                  ) : null}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label htmlFor="cat-order" className="text-xs font-medium text-slate-700">
                    Display order
                  </label>
                  <input
                    id="cat-order"
                    type="number"
                    value={form.displayOrder}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, displayOrder: Number.parseInt(e.target.value, 10) || 0 }))
                    }
                    className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                <div className="flex items-end">
                  <label className="flex cursor-pointer items-center gap-2 text-sm text-slate-800">
                    <input
                      type="checkbox"
                      checked={form.isActive}
                      onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                      className="accent-blue-600"
                    />
                    Active
                  </label>
                </div>
              </div>
            </div>

            <div className="mt-6 flex flex-wrap justify-end gap-2">
              <button
                type="button"
                onClick={closeModal}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-sm hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={saveMutation.isPending}
                onClick={() => void saveMutation.mutateAsync()}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {saveMutation.isPending ? 'Saving…' : modalMode === 'create' ? 'Create' : 'Save'}
              </button>
            </div>
          </div>
        </div>
      )}

      {deleteTarget && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-slate-900/50 p-4"
          role="presentation"
          onClick={() => setDeleteTarget(null)}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="admin-cat-delete-title"
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="admin-cat-delete-title" className="text-lg font-semibold text-slate-900">
              Delete category?
            </h2>
            <p className="mt-2 text-sm text-slate-600">
              <span className="font-medium text-slate-900">{deleteTarget.name}</span> will be removed. This only
              succeeds if the category has no products and no child categories.
            </p>
            <div className="mt-6 flex flex-wrap justify-end gap-2">
              <button
                type="button"
                onClick={() => setDeleteTarget(null)}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-sm hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={deleteMutation.isPending}
                onClick={() => void deleteMutation.mutateAsync(deleteTarget.slug)}
                className="rounded-lg bg-rose-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-rose-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {deleteMutation.isPending ? 'Deleting…' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
