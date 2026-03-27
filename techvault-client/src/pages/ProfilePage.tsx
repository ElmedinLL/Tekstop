import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import {
  messageFromUnknownError,
  notifyWishlistActionError,
  notifyWishlistItemRemoved,
  toast,
} from '../lib/notifications'
import { z } from 'zod'
import { useAuth } from '../auth/AuthContext'
import { OrderStatusBadge } from '../components/OrderStatusBadge'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { fetchOrderList } from '../lib/orders'
import { changePassword, fetchUserProfile, updateUserProfile, uploadUserAvatar } from '../lib/users'
import { fetchWishlist, removeFromWishlist } from '../lib/wishlist'

const TAB_IDS = ['personal', 'orders', 'wishlist', 'password'] as const
type TabId = (typeof TAB_IDS)[number]

function isTabId(v: string | null): v is TabId {
  return v != null && (TAB_IDS as readonly string[]).includes(v)
}

const profileSchema = z.object({
  firstName: z.string().min(1, 'Required'),
  lastName: z.string().min(1, 'Required'),
  phone: z.string().optional(),
})

type ProfileForm = z.infer<typeof profileSchema>

const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Required'),
    newPassword: z.string().min(8, 'At least 8 characters'),
    confirmPassword: z.string().min(1, 'Required'),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type PasswordForm = z.infer<typeof passwordSchema>

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

export function ProfilePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const rawTab = searchParams.get('tab')
  const activeTab: TabId = isTabId(rawTab) ? rawTab : 'personal'

  const setTab = (tab: TabId) => {
    setSearchParams(tab === 'personal' ? {} : { tab })
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Profile</h1>
      <p className="mt-1 text-sm text-slate-600">Manage your account, orders, wishlist, and security.</p>

      <div
        className="mt-8 flex flex-wrap gap-1 border-b border-slate-200"
        role="tablist"
        aria-label="Profile sections"
      >
        {(
          [
            ['personal', 'Personal info'],
            ['orders', 'My orders'],
            ['wishlist', 'Wishlist'],
            ['password', 'Change password'],
          ] as const
        ).map(([id, label]) => {
          const selected = activeTab === id
          return (
            <button
              key={id}
              type="button"
              role="tab"
              aria-selected={selected}
              onClick={() => setTab(id)}
              className={`rounded-t-lg px-4 py-2.5 text-sm font-medium transition ${
                selected
                  ? 'border-b-2 border-blue-600 text-blue-700'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              {label}
            </button>
          )
        })}
      </div>

      <div className="mt-8" role="tabpanel">
        {activeTab === 'personal' && <PersonalTab />}
        {activeTab === 'orders' && <OrdersTab />}
        {activeTab === 'wishlist' && <WishlistTab />}
        {activeTab === 'password' && <PasswordTab />}
      </div>
    </div>
  )
}

function PersonalTab() {
  const { user, updateLocalUser } = useAuth()
  const queryClient = useQueryClient()
  const profileQuery = useQuery({
    queryKey: ['profile'],
    queryFn: fetchUserProfile,
  })

  const [avatarFile, setAvatarFile] = useState<File | null>(null)
  const previewUrl = useMemo(() => (avatarFile ? URL.createObjectURL(avatarFile) : null), [avatarFile])

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl)
      }
    }
  }, [previewUrl])

  const profile = profileQuery.data

  const form = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    values: profile
      ? {
          firstName: profile.firstName,
          lastName: profile.lastName,
          phone: profile.phoneNumber ?? '',
        }
      : undefined,
  })

  const saveMutation = useMutation({
    mutationFn: updateUserProfile,
    onSuccess: (data) => {
      void queryClient.invalidateQueries({ queryKey: ['profile'] })
      updateLocalUser({
        firstName: data.firstName,
        lastName: data.lastName,
        profilePicture: data.avatarUrl ?? user?.profilePicture,
      })
      toast.success('Profile saved.')
    },
    onError: (e) => {
      toast.error(messageFromUnknownError(e, 'Could not save profile.'))
    },
  })

  const avatarMutation = useMutation({
    mutationFn: uploadUserAvatar,
    onSuccess: (data) => {
      void queryClient.invalidateQueries({ queryKey: ['profile'] })
      updateLocalUser({ profilePicture: data.avatarUrl })
      setAvatarFile(null)
      toast.success('Photo updated.')
    },
    onError: (e) => {
      toast.error(messageFromUnknownError(e, 'Upload failed.'))
    },
  })

  const displayAvatarUrl =
    previewUrl ||
    (profile?.avatarUrl ? resolveApiAssetUrl(profile.avatarUrl) : '') ||
    (user?.profilePicture ? resolveApiAssetUrl(user.profilePicture) : '')

  if (profileQuery.isPending) {
    return <p className="text-sm text-slate-500">Loading profile…</p>
  }

  if (profileQuery.isError) {
    return <p className="text-sm text-rose-600">Could not load profile.</p>
  }

  return (
    <div className="grid gap-10 md:grid-cols-[minmax(0,1fr)_minmax(0,2fr)]">
      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Photo</h2>
        <div className="mt-4 flex flex-col items-center gap-4">
          <div className="h-28 w-28 overflow-hidden rounded-full border-2 border-slate-200 bg-slate-100">
            {displayAvatarUrl ? (
              <img src={displayAvatarUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-3xl text-slate-400" aria-hidden>
                {(user?.firstName?.[0] ?? '?').toUpperCase()}
              </div>
            )}
          </div>
          <label className="cursor-pointer rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 hover:bg-slate-50">
            Choose image
            <input
              type="file"
              accept="image/jpeg,image/png,image/gif,image/webp,image/bmp"
              className="sr-only"
              onChange={(e) => {
                const f = e.target.files?.[0]
                setAvatarFile(f ?? null)
                e.target.value = ''
              }}
            />
          </label>
          {avatarFile && (
            <button
              type="button"
              disabled={avatarMutation.isPending}
              onClick={() => avatarMutation.mutate(avatarFile)}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {avatarMutation.isPending ? 'Uploading…' : 'Upload photo'}
            </button>
          )}
          <p className="text-center text-xs text-slate-500">JPEG, PNG, GIF, WebP, or BMP. Max size depends on server limits.</p>
        </div>
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Personal info</h2>
        <form
          className="mt-6 grid gap-4 sm:grid-cols-2"
          onSubmit={form.handleSubmit((v) =>
            saveMutation.mutate({
              firstName: v.firstName.trim(),
              lastName: v.lastName.trim(),
              phone: v.phone?.trim() || undefined,
            }),
          )}
        >
          <div className="sm:col-span-2">
            <label className="text-xs font-medium text-slate-600">Email</label>
            <input
              className="mt-1 w-full cursor-not-allowed rounded border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600"
              readOnly
              value={profile?.email ?? ''}
            />
            <p className="mt-1 text-xs text-slate-500">Email cannot be changed here.</p>
          </div>
          <div>
            <label className="text-xs font-medium text-slate-600">First name</label>
            <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm" {...form.register('firstName')} />
            {form.formState.errors.firstName && (
              <p className="mt-1 text-xs text-rose-600">{form.formState.errors.firstName.message}</p>
            )}
          </div>
          <div>
            <label className="text-xs font-medium text-slate-600">Last name</label>
            <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm" {...form.register('lastName')} />
            {form.formState.errors.lastName && (
              <p className="mt-1 text-xs text-rose-600">{form.formState.errors.lastName.message}</p>
            )}
          </div>
          <div className="sm:col-span-2">
            <label className="text-xs font-medium text-slate-600">Phone</label>
            <input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm" {...form.register('phone')} />
          </div>
          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={saveMutation.isPending}
              className="rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {saveMutation.isPending ? 'Saving…' : 'Save changes'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}

function OrdersTab() {
  const { data, isPending, isError } = useQuery({
    queryKey: ['orders', 'me', 1],
    queryFn: () => fetchOrderList(1),
  })

  const orders = data?.items ?? []

  if (isPending) {
    return <p className="text-sm text-slate-500">Loading orders…</p>
  }

  if (isError) {
    return <p className="text-sm text-rose-600">Could not load orders.</p>
  }

  if (orders.length === 0) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-600 shadow-sm">
        No orders yet.{' '}
        <Link to="/" className="font-medium text-blue-600 hover:underline">
          Continue shopping
        </Link>
      </div>
    )
  }

  return (
    <ul className="space-y-3">
      {orders.map((order) => (
        <li key={order.id}>
          <Link
            to={`/orders/${order.id}`}
            className="block rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:border-blue-300"
          >
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p className="text-sm font-semibold text-slate-900">{order.orderNumber}</p>
                <p className="mt-0.5 text-xs text-slate-500">{formatDate(order.placedAtUtc)}</p>
              </div>
              <OrderStatusBadge status={order.status} />
            </div>
            <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-sm">
              <span className="text-slate-600">
                {order.lineItemCount} line{order.lineItemCount === 1 ? '' : 's'}
              </span>
              <span className="font-semibold text-slate-900">{formatMoney(order.total)}</span>
            </div>
          </Link>
        </li>
      ))}
    </ul>
  )
}

function WishlistTab() {
  const queryClient = useQueryClient()
  const { data, isPending, isError } = useQuery({
    queryKey: ['wishlist'],
    queryFn: fetchWishlist,
  })

  const removeMu = useMutation({
    mutationFn: removeFromWishlist,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['wishlist'] })
      notifyWishlistItemRemoved()
    },
    onError: () => notifyWishlistActionError('Could not remove item.'),
  })

  const items = data ?? []

  if (isPending) {
    return <p className="text-sm text-slate-500">Loading wishlist…</p>
  }

  if (isError) {
    return <p className="text-sm text-rose-600">Could not load wishlist.</p>
  }

  if (items.length === 0) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-600 shadow-sm">
        Your wishlist is empty.{' '}
        <Link to="/" className="font-medium text-blue-600 hover:underline">
          Browse products
        </Link>
      </div>
    )
  }

  return (
    <ul className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-white shadow-sm">
      {items.map((item) => (
        <li key={item.productId} className="flex flex-wrap items-center gap-4 p-4">
          <Link to={`/products/${item.productId}`} className="shrink-0">
            {item.imageUrl ? (
              <img
                src={resolveApiAssetUrl(item.imageUrl)}
                alt=""
                className="h-16 w-16 rounded-lg border border-slate-100 object-cover"
              />
            ) : (
              <div className="flex h-16 w-16 items-center justify-center rounded-lg bg-slate-100 text-slate-400">—</div>
            )}
          </Link>
          <div className="min-w-0 flex-1">
            <Link to={`/products/${item.productId}`} className="font-medium text-slate-900 hover:text-blue-600 hover:underline">
              {item.name}
            </Link>
            <p className="mt-1 text-sm font-semibold text-slate-800">{formatMoney(item.price)}</p>
          </div>
          <button
            type="button"
            disabled={removeMu.isPending}
            onClick={() => removeMu.mutate(item.productId)}
            className="rounded-lg border border-rose-200 px-3 py-1.5 text-sm font-medium text-rose-700 hover:bg-rose-50 disabled:opacity-50"
          >
            Remove
          </button>
        </li>
      ))}
    </ul>
  )
}

function PasswordTab() {
  const form = useForm<PasswordForm>({ resolver: zodResolver(passwordSchema) })
  const mutation = useMutation({
    mutationFn: changePassword,
    onSuccess: () => {
      toast.success('Password updated.')
      form.reset()
    },
    onError: (e) => {
      toast.error(messageFromUnknownError(e, 'Could not update password.'))
    },
  })

  return (
    <section className="max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Change password</h2>
      <form
        className="mt-6 space-y-4"
        onSubmit={form.handleSubmit((v) =>
          mutation.mutate({ currentPassword: v.currentPassword, newPassword: v.newPassword }),
        )}
      >
        <div>
          <label className="text-xs font-medium text-slate-600">Current password</label>
          <input
            type="password"
            autoComplete="current-password"
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
            {...form.register('currentPassword')}
          />
          {form.formState.errors.currentPassword && (
            <p className="mt-1 text-xs text-rose-600">{form.formState.errors.currentPassword.message}</p>
          )}
        </div>
        <div>
          <label className="text-xs font-medium text-slate-600">New password</label>
          <input
            type="password"
            autoComplete="new-password"
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
            {...form.register('newPassword')}
          />
          {form.formState.errors.newPassword && (
            <p className="mt-1 text-xs text-rose-600">{form.formState.errors.newPassword.message}</p>
          )}
        </div>
        <div>
          <label className="text-xs font-medium text-slate-600">Confirm new password</label>
          <input
            type="password"
            autoComplete="new-password"
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2 text-sm"
            {...form.register('confirmPassword')}
          />
          {form.formState.errors.confirmPassword && (
            <p className="mt-1 text-xs text-rose-600">{form.formState.errors.confirmPassword.message}</p>
          )}
        </div>
        <button
          type="submit"
          disabled={mutation.isPending}
          className="rounded-lg bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
        >
          {mutation.isPending ? 'Updating…' : 'Update password'}
        </button>
      </form>
    </section>
  )
}
