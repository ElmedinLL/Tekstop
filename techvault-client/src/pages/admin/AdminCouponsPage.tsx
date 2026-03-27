import { useCallback, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import {
  createAdminCoupon,
  deleteAdminCoupon,
  fetchAdminCoupons,
  updateAdminCoupon,
} from '../../lib/adminCoupons'
import { messageFromUnknownError, toast } from '../../lib/notifications'
import type { AdminCoupon, AdminDiscountType, CreateAdminCouponBody } from '../../types/adminCoupon'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

function formatExpiry(iso: string | null) {
  if (!iso) return '—'
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

function formatType(t: AdminDiscountType) {
  return t === 0 ? 'Percent' : 'Fixed'
}

function formatValue(c: AdminCoupon) {
  if (c.discountType === 0) {
    return `${c.discountValue}%`
  }
  return formatMoney(c.discountValue)
}

function toDatetimeLocalValue(iso: string | null | undefined): string {
  if (!iso) return ''
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

type FormFields = {
  code: string
  discountType: AdminDiscountType
  discountValue: string
  minOrderValue: string
  expiresLocal: string
  usageLimit: string
  isActive: boolean
}

const emptyForm = (): FormFields => ({
  code: '',
  discountType: 0,
  discountValue: '',
  minOrderValue: '',
  expiresLocal: '',
  usageLimit: '',
  isActive: true,
})

function fieldsToBody(f: FormFields): CreateAdminCouponBody {
  const minOrder = Number.parseFloat(f.minOrderValue)
  const discountValue = Number.parseFloat(f.discountValue)
  const usageRaw = f.usageLimit.trim()
  const usageParsed = usageRaw === '' ? null : Number.parseInt(usageRaw, 10)
  const usageLimit =
    usageParsed != null && Number.isFinite(usageParsed) && usageParsed > 0 ? usageParsed : null

  return {
    code: f.code.trim().toUpperCase(),
    discountType: f.discountType,
    discountValue: Number.isFinite(discountValue) ? discountValue : 0,
    minOrderValue: Number.isFinite(minOrder) ? minOrder : 0,
    expiresAtUtc: f.expiresLocal.trim() ? new Date(f.expiresLocal).toISOString() : null,
    usageLimit,
    isActive: f.isActive,
  }
}

function couponToFields(c: AdminCoupon): FormFields {
  return {
    code: c.code,
    discountType: c.discountType,
    discountValue: String(c.discountValue),
    minOrderValue: String(c.minOrderValue),
    expiresLocal: toDatetimeLocalValue(c.expiresAtUtc),
    usageLimit: c.usageLimit != null ? String(c.usageLimit) : '',
    isActive: c.isActive,
  }
}

export function AdminCouponsPage() {
  const queryClient = useQueryClient()
  const [createFields, setCreateFields] = useState<FormFields>(emptyForm)
  const [editOpen, setEditOpen] = useState(false)
  const [editId, setEditId] = useState<number | null>(null)
  const [editFields, setEditFields] = useState<FormFields>(emptyForm)
  const [deleteTarget, setDeleteTarget] = useState<AdminCoupon | null>(null)

  const listQuery = useQuery({
    queryKey: ['admin', 'coupons'],
    queryFn: fetchAdminCoupons,
  })

  const invalidate = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: ['admin', 'coupons'] })
  }, [queryClient])

  const createMutation = useMutation({
    mutationFn: () => createAdminCoupon(fieldsToBody(createFields)),
    onSuccess: async () => {
      toast.success('Coupon created.')
      setCreateFields(emptyForm())
      await invalidate()
    },
    onError: onMutationError,
  })

  const updateMutation = useMutation({
    mutationFn: () => {
      if (editId == null) throw new Error('Missing coupon.')
      return updateAdminCoupon(editId, fieldsToBody(editFields))
    },
    onSuccess: async () => {
      toast.success('Coupon updated.')
      setEditOpen(false)
      setEditId(null)
      await invalidate()
    },
    onError: onMutationError,
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteAdminCoupon(id),
    onSuccess: async () => {
      toast.success('Coupon deleted.')
      setDeleteTarget(null)
      await invalidate()
    },
    onError: onMutationError,
  })

  function onMutationError(e: unknown) {
    if (isAxiosError(e)) {
      const d = e.response?.data
      if (typeof d === 'string' && d.trim()) {
        toast.error(d)
        return
      }
    }
    toast.error(messageFromUnknownError(e, 'Request failed.'))
  }

  const openEdit = (c: AdminCoupon) => {
    setEditId(c.id)
    setEditFields(couponToFields(c))
    setEditOpen(true)
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Coupons</h1>
        <p className="mt-1 text-sm text-slate-600">Create discount codes with rules and usage limits.</p>
      </div>

      <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Create coupon</h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <label className="block sm:col-span-1">
            <span className="text-xs font-medium text-slate-700">Code</span>
            <input
              value={createFields.code}
              onChange={(e) => setCreateFields((f) => ({ ...f, code: e.target.value }))}
              onBlur={(e) =>
                setCreateFields((f) => ({ ...f, code: e.target.value.trim().toUpperCase() }))
              }
              placeholder="SUMMER20"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm uppercase shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              autoComplete="off"
            />
          </label>
          <label className="block">
            <span className="text-xs font-medium text-slate-700">Type</span>
            <select
              value={createFields.discountType}
              onChange={(e) =>
                setCreateFields((f) => ({
                  ...f,
                  discountType: Number(e.target.value) as AdminDiscountType,
                }))
              }
              className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value={0}>Percent off</option>
              <option value={1}>Fixed amount off</option>
            </select>
          </label>
          <label className="block">
            <span className="text-xs font-medium text-slate-700">
              {createFields.discountType === 0 ? 'Percent (0–100)' : 'Amount (USD)'}
            </span>
            <input
              type="number"
              min={0}
              step={createFields.discountType === 0 ? 1 : 0.01}
              max={createFields.discountType === 0 ? 100 : undefined}
              value={createFields.discountValue}
              onChange={(e) => setCreateFields((f) => ({ ...f, discountValue: e.target.value }))}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </label>
          <label className="block">
            <span className="text-xs font-medium text-slate-700">Minimum order (USD)</span>
            <input
              type="number"
              min={0}
              step={0.01}
              value={createFields.minOrderValue}
              onChange={(e) => setCreateFields((f) => ({ ...f, minOrderValue: e.target.value }))}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </label>
          <label className="block">
            <span className="text-xs font-medium text-slate-700">Expiry (local)</span>
            <input
              type="datetime-local"
              value={createFields.expiresLocal}
              onChange={(e) => setCreateFields((f) => ({ ...f, expiresLocal: e.target.value }))}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </label>
          <label className="block">
            <span className="text-xs font-medium text-slate-700">Usage limit</span>
            <input
              type="number"
              min={0}
              step={1}
              placeholder="Unlimited if empty"
              value={createFields.usageLimit}
              onChange={(e) => setCreateFields((f) => ({ ...f, usageLimit: e.target.value }))}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </label>
          <label className="flex items-end gap-2 pb-1">
            <input
              type="checkbox"
              checked={createFields.isActive}
              onChange={(e) => setCreateFields((f) => ({ ...f, isActive: e.target.checked }))}
              className="accent-blue-600"
            />
            <span className="text-sm text-slate-800">Active</span>
          </label>
        </div>
        <div className="mt-4">
          <button
            type="button"
            disabled={createMutation.isPending}
            onClick={() => void createMutation.mutateAsync()}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {createMutation.isPending ? 'Creating…' : 'Create coupon'}
          </button>
        </div>
      </section>

      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        {listQuery.isPending && (
          <div className="space-y-2 p-4">
            {[0, 1, 2, 3].map((i) => (
              <div key={i} className="h-12 animate-pulse rounded-lg bg-slate-100" />
            ))}
          </div>
        )}
        {listQuery.isError && (
          <p className="p-6 text-sm text-rose-600">Could not load coupons.</p>
        )}
        {listQuery.data && listQuery.data.length === 0 && (
          <p className="p-6 text-sm text-slate-600">No coupons yet.</p>
        )}
        {listQuery.data && listQuery.data.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80">
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Code</th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Type</th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Value</th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Min order
                  </th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Expires</th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Usage</th>
                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">Status</th>
                  <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {listQuery.data.map((row) => (
                  <tr key={row.id} className="hover:bg-slate-50/80">
                    <td className="px-4 py-3 font-mono font-medium text-slate-900">{row.code}</td>
                    <td className="px-4 py-3 text-slate-700">{formatType(row.discountType)}</td>
                    <td className="px-4 py-3 text-slate-900">{formatValue(row)}</td>
                    <td className="px-4 py-3 tabular-nums text-slate-700">{formatMoney(row.minOrderValue)}</td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-700">{formatExpiry(row.expiresAtUtc)}</td>
                    <td className="px-4 py-3 tabular-nums text-slate-700">
                      {row.usageCount}
                      {row.usageLimit != null ? ` / ${row.usageLimit}` : ' / ∞'}
                    </td>
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
                      <button
                        type="button"
                        onClick={() => openEdit(row)}
                        className="text-sm font-medium text-blue-600 hover:underline"
                      >
                        Edit
                      </button>
                      <span className="mx-2 text-slate-300" aria-hidden>
                        |
                      </span>
                      <button
                        type="button"
                        onClick={() => setDeleteTarget(row)}
                        className="text-sm font-medium text-rose-600 hover:underline"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {editOpen && editId != null && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4"
          role="presentation"
          onClick={() => setEditOpen(false)}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="edit-coupon-title"
            className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="edit-coupon-title" className="text-lg font-semibold text-slate-900">
              Edit coupon
            </h2>
            <div className="mt-4 grid gap-4">
              <label className="block">
                <span className="text-xs font-medium text-slate-700">Code</span>
                <input
                  value={editFields.code}
                  onChange={(e) => setEditFields((f) => ({ ...f, code: e.target.value }))}
                  onBlur={(e) =>
                    setEditFields((f) => ({ ...f, code: e.target.value.trim().toUpperCase() }))
                  }
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm uppercase shadow-sm"
                />
              </label>
              <label className="block">
                <span className="text-xs font-medium text-slate-700">Type</span>
                <select
                  value={editFields.discountType}
                  onChange={(e) =>
                    setEditFields((f) => ({
                      ...f,
                      discountType: Number(e.target.value) as AdminDiscountType,
                    }))
                  }
                  className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm"
                >
                  <option value={0}>Percent off</option>
                  <option value={1}>Fixed amount off</option>
                </select>
              </label>
              <label className="block">
                <span className="text-xs font-medium text-slate-700">
                  {editFields.discountType === 0 ? 'Percent (0–100)' : 'Amount (USD)'}
                </span>
                <input
                  type="number"
                  min={0}
                  step={editFields.discountType === 0 ? 1 : 0.01}
                  max={editFields.discountType === 0 ? 100 : undefined}
                  value={editFields.discountValue}
                  onChange={(e) => setEditFields((f) => ({ ...f, discountValue: e.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm"
                />
              </label>
              <label className="block">
                <span className="text-xs font-medium text-slate-700">Minimum order (USD)</span>
                <input
                  type="number"
                  min={0}
                  step={0.01}
                  value={editFields.minOrderValue}
                  onChange={(e) => setEditFields((f) => ({ ...f, minOrderValue: e.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm"
                />
              </label>
              <label className="block">
                <span className="text-xs font-medium text-slate-700">Expiry (local)</span>
                <input
                  type="datetime-local"
                  value={editFields.expiresLocal}
                  onChange={(e) => setEditFields((f) => ({ ...f, expiresLocal: e.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm"
                />
              </label>
              <label className="block">
                <span className="text-xs font-medium text-slate-700">Usage limit</span>
                <input
                  type="number"
                  min={0}
                  placeholder="Unlimited if empty"
                  value={editFields.usageLimit}
                  onChange={(e) => setEditFields((f) => ({ ...f, usageLimit: e.target.value }))}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm"
                />
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={editFields.isActive}
                  onChange={(e) => setEditFields((f) => ({ ...f, isActive: e.target.checked }))}
                  className="accent-blue-600"
                />
                <span className="text-sm text-slate-800">Active</span>
              </label>
            </div>
            <div className="mt-6 flex flex-wrap justify-end gap-2">
              <button
                type="button"
                onClick={() => setEditOpen(false)}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-sm hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={updateMutation.isPending}
                onClick={() => void updateMutation.mutateAsync()}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {updateMutation.isPending ? 'Saving…' : 'Save'}
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
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 className="text-lg font-semibold text-slate-900">Delete coupon?</h2>
            <p className="mt-2 text-sm text-slate-600">
              Remove <span className="font-mono font-medium text-slate-900">{deleteTarget.code}</span>? Past orders
              that referenced this code are unchanged.
            </p>
            <div className="mt-6 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setDeleteTarget(null)}
                className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-sm"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={deleteMutation.isPending}
                onClick={() => void deleteMutation.mutateAsync(deleteTarget.id)}
                className="rounded-lg bg-rose-600 px-4 py-2 text-sm font-semibold text-white shadow-sm disabled:opacity-50"
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
