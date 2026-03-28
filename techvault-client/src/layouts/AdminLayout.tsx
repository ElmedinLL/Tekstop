import { useEffect, useState } from 'react'
import { Helmet } from 'react-helmet-async'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

type AdminNavItem = { to: string; label: string; end?: boolean }

const navItems: AdminNavItem[] = [
  { to: '/admin', label: 'Dashboard', end: true },
  { to: '/admin/products', label: 'Products' },
  { to: '/admin/orders', label: 'Orders' },
  { to: '/admin/users', label: 'Users' },
  { to: '/admin/categories', label: 'Categories' },
  { to: '/admin/coupons', label: 'Coupons' },
]

function MenuIcon({ open }: { open: boolean }) {
  return (
    <svg
      className="h-6 w-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={2}
      aria-hidden
    >
      {open ? (
        <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
      ) : (
        <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
      )}
    </svg>
  )
}

export function AdminLayout() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const [mobileNavOpen, setMobileNavOpen] = useState(false)

  useEffect(() => {
    setMobileNavOpen(false)
  }, [location.pathname])

  useEffect(() => {
    if (!mobileNavOpen) {
      return
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setMobileNavOpen(false)
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [mobileNavOpen])

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-lg px-3 py-2 text-sm font-medium transition ${
      isActive ? 'bg-slate-800 text-white' : 'text-slate-300 hover:bg-slate-800/80 hover:text-white'
    }`

  return (
    <div className="min-h-screen bg-slate-100 text-slate-900 md:flex">
      <Helmet>
        <meta name="robots" content="noindex,nofollow" />
      </Helmet>
      {mobileNavOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-slate-900/50 md:hidden"
          aria-label="Close menu"
          onClick={() => setMobileNavOpen(false)}
        />
      )}

      <aside
        id="admin-sidebar-nav"
        className={`fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-slate-800 bg-slate-900 text-slate-100 shadow-xl transition-transform duration-200 ease-out md:static md:translate-x-0 md:shadow-none ${
          mobileNavOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
        }`}
        aria-label="Admin navigation"
      >
        <div className="flex h-14 shrink-0 items-center justify-between border-b border-slate-800 px-4 md:h-16">
          <Link to="/admin" className="text-lg font-semibold tracking-tight text-white">
            TechVault Admin
          </Link>
          <button
            type="button"
            className="rounded-lg p-2 text-slate-400 hover:bg-slate-800 hover:text-white md:hidden"
            aria-label="Close sidebar"
            onClick={() => setMobileNavOpen(false)}
          >
            <MenuIcon open />
          </button>
        </div>

        <nav className="flex-1 space-y-0.5 overflow-y-auto p-3">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.end === true} className={linkClass}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-slate-800 p-3">
          <Link
            to="/"
            className="mb-2 block rounded-lg px-3 py-2 text-sm font-medium text-slate-400 hover:bg-slate-800 hover:text-white"
          >
            ← Storefront
          </Link>
          <p className="truncate px-3 text-xs text-slate-500">{user?.email}</p>
          <button
            type="button"
            className="mt-2 w-full rounded-lg px-3 py-2 text-left text-sm font-medium text-slate-300 hover:bg-slate-800 hover:text-white"
            onClick={() => void logout()}
          >
            Sign out
          </button>
        </div>
      </aside>

      <div className="flex min-h-screen min-w-0 flex-1 flex-col pt-14 md:pt-0">
        <header className="fixed top-0 left-0 right-0 z-30 flex h-14 items-center gap-3 border-b border-slate-200 bg-white px-4 shadow-sm md:hidden">
          <button
            type="button"
            className="rounded-lg p-2 text-slate-700 hover:bg-slate-100"
            aria-expanded={mobileNavOpen}
            aria-controls="admin-sidebar-nav"
            aria-label={mobileNavOpen ? 'Close navigation menu' : 'Open navigation menu'}
            onClick={() => setMobileNavOpen((o) => !o)}
          >
            <MenuIcon open={mobileNavOpen} />
          </button>
          <span className="text-sm font-semibold text-slate-900">Admin</span>
        </header>

        <main className="flex-1 p-4 sm:p-6 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
