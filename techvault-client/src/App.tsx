import { Link, Route, Routes } from 'react-router-dom'
import { HomePage } from './pages/HomePage'
import { AboutPage } from './pages/AboutPage'

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
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
        </Routes>
      </main>
    </div>
  )
}
