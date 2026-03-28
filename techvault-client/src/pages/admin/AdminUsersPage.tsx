import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useAuth } from '../../auth/AuthContext'
import { Pagination } from '../../components/Pagination'
import { Seo } from '../../components/Seo'
import { fetchAdminUsers, setAdminUserBanned } from '../../lib/adminUsers'
import { resolveApiAssetUrl } from '../../lib/assetUrl'
import { messageFromUnknownError, toast } from '../../lib/notifications'
import type { AdminUserListItem } from '../../types/adminUser'

const PAGE_SIZES = [10, 20, 50] as const
const DEFAULT_PAGE_SIZE = 20

function parsePage(raw: string | null) {
  const n = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : 1
  return Number.isFinite(n) && n >= 1 ? n : 1
}

function parsePageSize(raw: string | null) {
  const n = raw != null && raw.trim() !== '' ? Number.parseInt(raw, 10) : DEFAULT_PAGE_SIZE
  if (!Number.isFinite(n)) return DEFAULT_PAGE_SIZE
  return PAGE_SIZES.includes(n as (typeof PAGE_SIZES)[number]) ? n : DEFAULT_PAGE_SIZE
}

function formatJoined(iso: string) {
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(iso))
}

function userDisplayName(u: AdminUserListItem) {
  const n = `${u.firstName} ${u.lastName}`.trim()
  return n || u.email
}

function initials(u: AdminUserListItem) {
  const a = u.firstName.trim().charAt(0) || u.email.charAt(0)
  const b = u.lastName.trim().charAt(0) || ''
  return `${a}${b}`.toUpperCase() || '?'
}

function UserAvatar({ user }: { user: AdminUserListItem }) {
  const [imgError, setImgError] = useState(false)
  const src = user.avatarUrl ? resolveApiAssetUrl(user.avatarUrl) : ''
  const showImg = Boolean(src && !imgError)

  return (
    <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full bg-slate-200 text-xs font-semibold text-slate-600 ring-2 ring-white">
      {showImg ? (
        <img
          src={src}
          alt=""
          className="h-full w-full object-cover"
          onError={() => setImgError(true)}
        />
      ) : (
        <span aria-hidden>{initials(user)}</span>
      )}
    </div>
  )
}

function BanToggle({
  row,
  disabled,
  busy,
  onToggle,
}: {
  row: AdminUserListItem
  disabled: boolean
  busy: boolean
  onToggle: () => void
}) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={row.isBanned}
      aria-busy={busy}
      disabled={disabled || busy}
      onClick={onToggle}
      title={disabled ? 'You cannot change ban for this account' : row.isBanned ? 'Unban user' : 'Ban user'}
      className={`relative inline-flex h-8 w-14 shrink-0 items-center rounded-full border transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
        row.isBanned ? 'border-rose-200 bg-rose-600' : 'border-slate-200 bg-slate-200'
      }`}
    >
      <span
        className={`inline-block h-6 w-6 transform rounded-full bg-white shadow transition ${
          row.isBanned ? 'translate-x-7' : 'translate-x-1'
        }`}
      />
      <span className="sr-only">{row.isBanned ? 'Banned' : 'Active'}</span>
    </button>
  )
}

export function AdminUsersPage() {
  const { user: currentUser } = useAuth()
  const queryClient = useQueryClient()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = parsePage(searchParams.get('page'))
  const pageSize = parsePageSize(searchParams.get('pageSize'))
  const searchFromUrl = searchParams.get('search') ?? ''
  const [searchInput, setSearchInput] = useState(searchFromUrl)

  useEffect(() => {
    setSearchInput(searchFromUrl)
  }, [searchFromUrl])

  useEffect(() => {
    const t = window.setTimeout(() => {
      const trimmed = searchInput.trim()
      const urlTrimmed = searchFromUrl.trim()
      if (trimmed === urlTrimmed) return

      setSearchParams((prev) => {
        const next = new URLSearchParams(prev)
        if (trimmed) next.set('search', trimmed)
        else next.delete('search')
        next.set('page', '1')
        return next
      })
    }, 350)
    return () => window.clearTimeout(t)
  }, [searchInput, searchFromUrl, setSearchParams])

  const queryArgs = useMemo(
    () => ({
      page,
      pageSize,
      search: searchFromUrl.trim() || undefined,
    }),
    [page, pageSize, searchFromUrl],
  )

  const listQuery = useQuery({
    queryKey: ['admin', 'users', queryArgs],
    queryFn: () => fetchAdminUsers(queryArgs),
  })

  const banMutation = useMutation({
    mutationFn: ({ userId, banned }: { userId: string; banned: boolean }) => setAdminUserBanned(userId, banned),
    onSuccess: async (_, { banned }) => {
      toast.success(banned ? 'User banned.' : 'User unbanned.')
      await queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })
    },
    onError: (e: unknown) => {
      if (isAxiosError(e) && typeof e.response?.data === 'string' && e.response.data.trim()) {
        toast.error(e.response.data)
        return
      }
      toast.error(messageFromUnknownError(e, 'Could not update ban status.'))
    },
  })

  const setPageSize = useCallback(
    (next: number) => {
      setSearchParams((prev) => {
        const n = new URLSearchParams(prev)
        n.set('pageSize', String(next))
        n.set('page', '1')
        return n
      })
    },
    [setSearchParams],
  )

  const totalCount = listQuery.data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize) || 1)

  const canToggleBan = useCallback(
    (row: AdminUserListItem) => {
      if (currentUser && row.userId === currentUser.userId) return false
      if (row.roles.some((r) => r === 'Admin')) return false
      return true
    },
    [currentUser],
  )

  return (
    <div className="space-y-6">
      <Seo
        title="Admin users"
        description="Manage TechVault user accounts, roles, and sign-in lockout."
      />
      <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Users</h1>
          <p className="mt-1 text-sm text-slate-600">Identity accounts, roles, and sign-in lockout (ban).</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <span className="whitespace-nowrap">Per page</span>
            <select
              value={pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {PAGE_SIZES.map((n) => (
                <option key={n} value={n}>
                  {n}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-100 p-4">
          <label className="block text-xs font-medium text-slate-600" htmlFor="admin-user-search">
            Search
          </label>
          <input
            id="admin-user-search"
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Email or name…"
            className="mt-1 w-full max-w-md rounded-lg border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            autoComplete="off"
          />
        </div>

        {listQuery.isPending && (
          <div className="space-y-2 p-4">
            {[0, 1, 2, 3, 4].map((i) => (
              <div key={i} className="h-14 animate-pulse rounded-lg bg-slate-100" />
            ))}
          </div>
        )}

        {listQuery.isError && (
          <p className="p-6 text-sm text-rose-600">Could not load users.</p>
        )}

        {listQuery.data && listQuery.data.items.length === 0 && (
          <p className="p-6 text-sm text-slate-600">No users match your search.</p>
        )}

        {listQuery.data && listQuery.data.items.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[880px] text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80">
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    User
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Email
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Role
                  </th>
                  <th scope="col" className="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Joined
                  </th>
                  <th scope="col" className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Orders
                  </th>
                  <th scope="col" className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-600">
                    Ban
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {listQuery.data.items.map((row) => {
                  const toggleDisabled = !canToggleBan(row)
                  const busy = banMutation.isPending && banMutation.variables?.userId === row.userId
                  return (
                    <tr key={row.userId} className="bg-white hover:bg-slate-50/80">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <UserAvatar user={row} />
                          <span className="font-medium text-slate-900">{userDisplayName(row)}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-slate-700">{row.email}</td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap gap-1">
                          {row.roles.length === 0 ? (
                            <span className="text-slate-400">—</span>
                          ) : (
                            row.roles.map((r) => (
                              <span
                                key={r}
                                className="inline-flex rounded-md bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-800 ring-1 ring-slate-200/80"
                              >
                                {r}
                              </span>
                            ))
                          )}
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-700">{formatJoined(row.joinedAtUtc)}</td>
                      <td className="px-4 py-3 text-right tabular-nums text-slate-900">{row.orderCount}</td>
                      <td className="px-4 py-3 text-right">
                        <div className="inline-flex items-center justify-end gap-2">
                          <BanToggle
                            row={row}
                            disabled={toggleDisabled}
                            busy={Boolean(busy)}
                            onToggle={() => {
                              if (toggleDisabled) return
                              banMutation.mutate({ userId: row.userId, banned: !row.isBanned })
                            }}
                          />
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}

        {listQuery.data && listQuery.data.items.length > 0 && (
          <div className="flex flex-col gap-2 border-t border-slate-100 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-xs text-slate-500">
              {listQuery.data.totalCount.toLocaleString()} user{listQuery.data.totalCount === 1 ? '' : 's'}
            </p>
            {totalPages > 1 && <Pagination totalPages={totalPages} className="border-0 pt-0" />}
          </div>
        )}
      </div>
    </div>
  )
}
