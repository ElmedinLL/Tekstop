import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function AboutPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="About"
        description="Learn about TechVault, our marketplace, and how we help you discover quality tech products."
      />
      <h1 className="text-2xl font-semibold">About</h1>
      <p className="mt-2 text-slate-600">Placeholder route for React Router.</p>
      <Link className="mt-6 inline-block text-blue-600 hover:underline" to="/">
        ← Back home
      </Link>
    </div>
  )
}
