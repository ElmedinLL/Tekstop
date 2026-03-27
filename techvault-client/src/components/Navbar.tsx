import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useCartStore } from '../store/useCartStore'

type NavbarProps = {
  onOpenCart?: () => void
}

export function Navbar({ onOpenCart }: NavbarProps) {
  const { isAuthenticated, user, logout } = useAuth()
  const count = useCartStore((s) => s.itemCount())
  const badgeBump = useCartStore((s) => s.badgeBump)

  return (
    <header className="border-b border-slate-200 bg-white">
      <nav className="mx-auto flex max-w-5xl flex-wrap items-center gap-4 px-4 py-3 text-sm font-medium">
        <Link className="text-blue-600 hover:underline" to="/">
          Home
        </Link>
        <Link className="text-blue-600 hover:underline" to="/about">
          About
        </Link>
        <Link className="text-blue-600 hover:underline" to="/cart">
          Cart
        </Link>
        <button
          type="button"
          id="nav-cart-icon"
          onClick={() => onOpenCart?.()}
          className="relative inline-flex items-center gap-1 rounded-md text-blue-600 hover:underline"
          aria-label="Open mini cart"
        >
          <span aria-hidden>🛒</span>
          <span
            key={badgeBump}
            className="inline-flex min-w-[1.25rem] justify-center rounded-full bg-blue-600 px-1.5 text-xs font-bold text-white animate-[cartBadge_0.45s_ease-out]"
          >
            {count}
          </span>
        </button>
        <Link className="text-blue-600 hover:underline" to="/account">
          Account
        </Link>
        <Link className="text-blue-600 hover:underline" to="/admin/orders">
          Admin
        </Link>
        {isAuthenticated ? (
          <>
            <span className="text-slate-500">
              {user?.firstName} {user?.lastName}
            </span>
            <button
              type="button"
              className="text-slate-600 underline decoration-slate-300 hover:text-slate-900"
              onClick={() => void logout()}
            >
              Sign out
            </button>
          </>
        ) : (
          <>
            <Link className="text-blue-600 hover:underline" to="/login">
              Sign in
            </Link>
            <Link className="text-blue-600 hover:underline" to="/register">
              Register
            </Link>
          </>
        )}
      </nav>
      <style>{`
        @keyframes cartBadge {
          0% { transform: scale(1); }
          40% { transform: scale(1.25); }
          100% { transform: scale(1); }
        }
      `}</style>
    </header>
  )
}
