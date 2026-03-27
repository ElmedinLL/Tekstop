import { Link, Route, Routes } from 'react-router-dom'
import { AdminRoute } from './auth/AdminRoute'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { HomePage } from './pages/HomePage'
import { AboutPage } from './pages/AboutPage'
import { AccountPage } from './pages/AccountPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { AdminPage } from './pages/AdminPage'
import { OrdersPage } from './pages/OrdersPage'
import { OrderDetailPage } from './pages/OrderDetailPage'

export default function App() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <nav className="mx-auto flex max-w-3xl gap-6 px-4 py-3 text-sm font-medium">
          <Link className="text-blue-600 hover:underline" to="/">
            Home
          </Link>
          <Link className="text-blue-600 hover:underline" to="/about">
            About
          </Link>
          <Link className="text-blue-600 hover:underline" to="/account">
            Account
          </Link>
          <Link className="text-blue-600 hover:underline" to="/orders">
            Orders
          </Link>
          <Link className="text-blue-600 hover:underline" to="/admin">
            Admin
          </Link>
          <Link className="text-blue-600 hover:underline" to="/login">
            Sign in
          </Link>
          <Link className="text-blue-600 hover:underline" to="/register">
            Register
          </Link>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route
            path="/account"
            element={
              <ProtectedRoute>
                <AccountPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders"
            element={
              <ProtectedRoute>
                <OrdersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders/:orderId"
            element={
              <ProtectedRoute>
                <OrderDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin"
            element={
              <AdminRoute>
                <AdminPage />
              </AdminRoute>
            }
          />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Routes>
      </main>
    </div>
  )
}
