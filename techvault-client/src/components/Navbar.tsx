import { useQuery } from '@tanstack/react-query'
import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useCartQuery } from '../hooks/useCart'
import { fetchCategories } from '../lib/categories'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { useCartStore } from '../store/useCartStore'
import { useCompareStore } from '../store/useCompareStore'

type NavbarProps = {
  onOpenCart?: () => void
}

function hasAdminRole(roles: string[] | undefined): boolean {
  return roles?.some((r) => r.toLowerCase() === 'admin') ?? false
}

function useScrollShadow(threshold = 6) {
  const [scrolled, setScrolled] = useState(false)
  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > threshold)
    onScroll()
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [threshold])
  return scrolled
}

function useClickOutside(open: boolean, onClose: () => void) {
  const ref = useRef<HTMLDivElement>(null)
  useEffect(() => {
    if (!open) return
    const onDown = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        onClose()
      }
    }
    document.addEventListener('mousedown', onDown)
    return () => document.removeEventListener('mousedown', onDown)
  }, [open, onClose])
  return ref
}

function LogoIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 32 32" fill="none" aria-hidden>
      <rect width="32" height="32" rx="8" className="fill-blue-600" />
      <path
        d="M9 11h14v2H9V11zm0 5h10v2H9v-2zm0 5h7v2H9v-2z"
        className="fill-white"
        opacity="0.95"
      />
    </svg>
  )
}

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 20 20" fill="none" stroke="currentColor" aria-hidden>
      <circle cx="8.5" cy="8.5" r="5" strokeWidth="1.75" />
      <path d="M12.5 12.5L17 17" strokeWidth="1.75" strokeLinecap="round" />
    </svg>
  )
}

function CompareIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <path strokeLinecap="round" strokeWidth="1.75" d="M4 7h6M4 12h10M4 17h8" />
      <path strokeLinecap="round" strokeWidth="1.75" d="M14 7h6M16 12h4M18 17h2" />
    </svg>
  )
}

function HeartIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <path
        strokeWidth="1.75"
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"
      />
    </svg>
  )
}

function CartIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <path
        strokeWidth="1.75"
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M6 6h15l-1.5 9h-12L4.5 6H2"
      />
      <circle cx="9" cy="20" r="1" fill="currentColor" stroke="none" />
      <circle cx="18" cy="20" r="1" fill="currentColor" stroke="none" />
    </svg>
  )
}

function ChevronDownIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden>
      <path
        fillRule="evenodd"
        d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.94a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z"
        clipRule="evenodd"
      />
    </svg>
  )
}

function UserIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <path
        strokeWidth="1.75"
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"
      />
    </svg>
  )
}

const iconBtn =
  'inline-flex h-10 w-10 items-center justify-center rounded-lg text-slate-600 transition hover:bg-slate-100 hover:text-slate-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2'

export function Navbar({ onOpenCart }: NavbarProps) {
  const navigate = useNavigate()
  const { isAuthenticated, user, logout } = useAuth()
  const { data: cart } = useCartQuery()
  const count = cart?.totalItemCount ?? 0
  const badgeBump = useCartStore((s) => s.badgeBump)
  const compareCount = useCompareStore((s) => s.ids.length)
  const scrolled = useScrollShadow()

  const [search, setSearch] = useState('')
  const [catOpen, setCatOpen] = useState(false)
  const [userOpen, setUserOpen] = useState(false)

  const closeCats = useCallback(() => setCatOpen(false), [])
  const closeUser = useCallback(() => setUserOpen(false), [])

  const catRef = useClickOutside(catOpen, closeCats)
  const userRef = useClickOutside(userOpen, closeUser)

  const { data: categories = [], isPending: catsLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: fetchCategories,
    staleTime: 10 * 60_000,
  })

  const activeCategories = categories.filter((c) => c.isActive)

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    const q = search.trim()
    if (!q) {
      navigate('/search')
      return
    }
    navigate(`/search?q=${encodeURIComponent(q)}`)
  }

  const avatarSrc =
    user?.profilePicture != null && user.profilePicture !== ''
      ? resolveApiAssetUrl(user.profilePicture)
      : null

  const displayName =
    user?.firstName || user?.lastName
      ? `${user?.firstName ?? ''} ${user?.lastName ?? ''}`.trim()
      : user?.email ?? 'Account'

  return (
    <header
      className={`sticky top-0 z-50 border-b border-slate-200/80 bg-white/95 backdrop-blur-sm transition-shadow duration-200 ${
        scrolled ? 'shadow-md shadow-slate-900/10' : ''
      }`}
    >
      <nav className="mx-auto max-w-7xl px-4" aria-label="Main">
        <div className="flex flex-wrap items-center gap-3 py-3 sm:gap-4">
          <div className="flex min-w-0 shrink-0 items-center gap-2 sm:gap-3">
            <Link
              to="/"
              className="flex items-center gap-2 rounded-lg pr-1 text-slate-900 outline-none ring-blue-500/0 transition hover:text-blue-700 focus-visible:ring-2 focus-visible:ring-blue-500"
            >
              <LogoIcon className="h-9 w-9 shrink-0" />
              <span className="hidden text-lg font-semibold tracking-tight sm:inline">TechVault</span>
            </Link>

            <Link
              to="/about"
              className="hidden text-sm font-medium text-slate-600 transition hover:text-blue-700 md:inline"
            >
              About
            </Link>

            <div className="relative" ref={catRef}>
              <button
                type="button"
                className="inline-flex h-10 items-center gap-1 rounded-lg border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 shadow-sm transition hover:border-slate-300 hover:bg-slate-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
                aria-expanded={catOpen}
                aria-haspopup="true"
                aria-controls="nav-categories-panel"
                onClick={() => {
                  setCatOpen((o) => !o)
                  setUserOpen(false)
                }}
              >
                Categories
                <ChevronDownIcon className="h-4 w-4 text-slate-500" />
              </button>
              {catOpen && (
                <div
                  id="nav-categories-panel"
                  className="absolute left-0 top-full z-50 mt-1.5 max-h-[min(24rem,70vh)] w-56 overflow-auto rounded-xl border border-slate-200 bg-white py-1 shadow-lg"
                  role="menu"
                >
                  {catsLoading && (
                    <p className="px-3 py-2.5 text-sm text-slate-500" role="presentation">
                      Loading…
                    </p>
                  )}
                  {!catsLoading && activeCategories.length === 0 && (
                    <p className="px-3 py-2.5 text-sm text-slate-500">No categories</p>
                  )}
                  {activeCategories.map((c) => (
                    <Link
                      key={c.id}
                      to={`/category/${encodeURIComponent(c.slug)}`}
                      role="menuitem"
                      className="block px-3 py-2.5 text-sm text-slate-700 transition hover:bg-slate-50 hover:text-blue-700"
                      onClick={closeCats}
                    >
                      {c.name}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          </div>

          <form
            onSubmit={onSearch}
            className="order-3 flex min-w-0 w-full flex-1 basis-full sm:order-none sm:max-w-xl md:max-w-2xl"
            role="search"
          >
            <label htmlFor="nav-search" className="sr-only">
              Search products
            </label>
            <div className="flex w-full overflow-hidden rounded-lg border border-slate-200 bg-slate-50/80 shadow-sm transition focus-within:border-blue-400 focus-within:bg-white focus-within:ring-2 focus-within:ring-blue-500/20">
              <div className="flex items-center pl-3 text-slate-400">
                <SearchIcon className="h-5 w-5" />
              </div>
              <input
                id="nav-search"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search products, brands, SKU…"
                autoComplete="off"
                className="min-w-0 flex-1 border-0 bg-transparent px-3 py-2.5 text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-0"
              />
              <button
                type="submit"
                className="hidden shrink-0 bg-blue-600 px-4 text-sm font-semibold text-white transition hover:bg-blue-700 sm:block"
              >
                Search
              </button>
            </div>
          </form>

          <div className="ml-auto flex shrink-0 items-center gap-0.5 sm:gap-1">
            <Link
              to="/compare"
              className={`${iconBtn} relative`}
              aria-label={`Compare products, ${compareCount} selected`}
              title="Compare"
              onClick={() => setUserOpen(false)}
            >
              <CompareIcon className="h-[22px] w-[22px]" />
              {compareCount > 0 && (
                <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-[1.25rem] items-center justify-center rounded-full bg-slate-700 px-1 text-[11px] font-bold leading-none text-white">
                  {compareCount}
                </span>
              )}
            </Link>
            <Link
              to="/wishlist"
              className={iconBtn}
              aria-label="Wishlist"
              title="Wishlist"
              onClick={() => setUserOpen(false)}
            >
              <HeartIcon className="h-[22px] w-[22px]" />
            </Link>

            <button
              type="button"
              id="nav-cart-icon"
              onClick={() => {
                onOpenCart?.()
                setUserOpen(false)
                setCatOpen(false)
              }}
              className={`${iconBtn} relative`}
              aria-label={`Open cart, ${count} items`}
            >
              <CartIcon className="h-[22px] w-[22px]" />
              <span
                key={badgeBump}
                className="absolute -right-0.5 -top-0.5 flex h-5 min-w-[1.25rem] items-center justify-center rounded-full bg-blue-600 px-1 text-[11px] font-bold leading-none text-white animate-[navCartBadge_0.45s_ease-out]"
              >
                {count > 99 ? '99+' : count}
              </span>
            </button>

            <div className="relative" ref={userRef}>
              <button
                type="button"
                className={`${iconBtn} gap-1.5 px-2 sm:w-auto sm:min-w-[2.5rem]`}
                aria-expanded={userOpen}
                aria-haspopup="true"
                aria-controls="nav-user-panel"
                onClick={() => {
                  setUserOpen((o) => !o)
                  setCatOpen(false)
                }}
              >
                {avatarSrc ? (
                  <img
                    src={avatarSrc}
                    alt=""
                    className="h-8 w-8 rounded-full object-cover ring-2 ring-slate-200"
                  />
                ) : (
                  <UserIcon className="h-[22px] w-[22px]" />
                )}
                <ChevronDownIcon className="hidden h-4 w-4 text-slate-500 sm:block" />
              </button>
              {userOpen && (
                <div
                  id="nav-user-panel"
                  className="absolute right-0 top-full z-50 mt-1.5 w-56 rounded-xl border border-slate-200 bg-white py-1 shadow-lg"
                  role="menu"
                >
                  {isAuthenticated ? (
                    <>
                      <div className="border-b border-slate-100 px-3 py-2.5">
                        <p className="truncate text-sm font-semibold text-slate-900">{displayName}</p>
                        <p className="truncate text-xs text-slate-500">{user?.email}</p>
                      </div>
                      <Link
                        to="/account"
                        role="menuitem"
                        className="block px-3 py-2.5 text-sm text-slate-700 hover:bg-slate-50"
                        onClick={closeUser}
                      >
                        Account settings
                      </Link>
                      <Link
                        to="/orders"
                        role="menuitem"
                        className="block px-3 py-2.5 text-sm text-slate-700 hover:bg-slate-50"
                        onClick={closeUser}
                      >
                        My orders
                      </Link>
                      <Link
                        to="/wishlist"
                        role="menuitem"
                        className="block px-3 py-2.5 text-sm text-slate-700 hover:bg-slate-50 sm:hidden"
                        onClick={closeUser}
                      >
                        Wishlist
                      </Link>
                      {hasAdminRole(user?.roles) && (
                        <Link
                          to="/admin"
                          role="menuitem"
                          className="block px-3 py-2.5 text-sm text-slate-700 hover:bg-slate-50"
                          onClick={closeUser}
                        >
                          Admin panel
                        </Link>
                      )}
                      <Link
                        to="/about"
                        role="menuitem"
                        className="block border-t border-slate-100 px-3 py-2.5 text-sm text-slate-600 hover:bg-slate-50 md:hidden"
                        onClick={closeUser}
                      >
                        About
                      </Link>
                      <button
                        type="button"
                        role="menuitem"
                        className="w-full px-3 py-2.5 text-left text-sm font-medium text-rose-700 hover:bg-rose-50"
                        onClick={() => {
                          closeUser()
                          void logout()
                        }}
                      >
                        Sign out
                      </button>
                    </>
                  ) : (
                    <>
                      <Link
                        to="/login"
                        role="menuitem"
                        className="block px-3 py-2.5 text-sm font-medium text-slate-900 hover:bg-slate-50"
                        onClick={closeUser}
                      >
                        Sign in
                      </Link>
                      <Link
                        to="/register"
                        role="menuitem"
                        className="block px-3 py-2.5 text-sm text-slate-700 hover:bg-slate-50"
                        onClick={closeUser}
                      >
                        Create account
                      </Link>
                      <Link
                        to="/about"
                        role="menuitem"
                        className="block border-t border-slate-100 px-3 py-2.5 text-sm text-slate-600 hover:bg-slate-50 md:hidden"
                        onClick={closeUser}
                      >
                        About
                      </Link>
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </nav>
      <style>{`
        @keyframes navCartBadge {
          0% { transform: scale(1); }
          40% { transform: scale(1.2); }
          100% { transform: scale(1); }
        }
      `}</style>
    </header>
  )
}
