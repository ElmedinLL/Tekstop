import { Route, Routes, useLocation } from 'react-router-dom'
import { AdminRoute } from './auth/AdminRoute'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { CartAuthBridge } from './components/CartAuthBridge'
import { CartDrawer } from './components/CartDrawer'
import { Navbar } from './components/Navbar'
import { AdminLayout } from './layouts/AdminLayout'
import { HomePage } from './pages/HomePage'
import { AboutPage } from './pages/AboutPage'
import { ProfilePage } from './pages/ProfilePage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { AdminCategoriesPage } from './pages/admin/AdminCategoriesPage'
import { AdminCouponsPage } from './pages/admin/AdminCouponsPage'
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage'
import { AdminOrdersPage } from './pages/admin/AdminOrdersPage'
import { AdminProductFormPage } from './pages/admin/AdminProductFormPage'
import { AdminProductsPage } from './pages/admin/AdminProductsPage'
import { AdminUsersPage } from './pages/admin/AdminUsersPage'
import { OrdersPage } from './pages/OrdersPage'
import { OrderDetailPage } from './pages/OrderDetailPage'
import { ProductDetailPage } from './pages/ProductDetailPage'
import { CategoryPage } from './pages/CategoryPage'
import { SearchPage } from './pages/SearchPage'
import { CartPage } from './pages/CartPage'
import { WishlistPage } from './pages/WishlistPage'
import { CheckoutPage } from './pages/CheckoutPage'
import { OrderConfirmationPage } from './pages/OrderConfirmationPage'
import { useCartStore } from './store/useCartStore'

export default function App() {
  const location = useLocation()
  const isAdminShell = location.pathname.startsWith('/admin')
  const cartDrawerOpen = useCartStore((s) => s.cartDrawerOpen)
  const setCartDrawerOpen = useCartStore((s) => s.setCartDrawerOpen)

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <CartAuthBridge />
      {!isAdminShell && (
        <>
          <Navbar onOpenCart={() => setCartDrawerOpen(true)} />
          <CartDrawer open={cartDrawerOpen} onClose={() => setCartDrawerOpen(false)} />
        </>
      )}
      <main className={isAdminShell ? 'min-h-screen' : undefined}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/category/:slug" element={<CategoryPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route
            path="/checkout"
            element={
              <ProtectedRoute>
                <CheckoutPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders/confirmation/:orderId"
            element={
              <ProtectedRoute>
                <OrderConfirmationPage />
              </ProtectedRoute>
            }
          />
          <Route path="/about" element={<AboutPage />} />
          <Route
            path="/account"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/wishlist"
            element={
              <ProtectedRoute>
                <WishlistPage />
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
                <AdminLayout />
              </AdminRoute>
            }
          >
            <Route index element={<AdminDashboardPage />} />
            <Route path="products" element={<AdminProductsPage />} />
            <Route path="products/new" element={<AdminProductFormPage />} />
            <Route path="products/:productId/edit" element={<AdminProductFormPage />} />
            <Route path="orders" element={<AdminOrdersPage />} />
            <Route path="users" element={<AdminUsersPage />} />
            <Route path="categories" element={<AdminCategoriesPage />} />
            <Route path="coupons" element={<AdminCouponsPage />} />
          </Route>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Routes>
      </main>
    </div>
  )
}
