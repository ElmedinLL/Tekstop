import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function PrivacyPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="Privacy policy"
        description="How TechVault collects, uses, and protects your information."
      />
      <h1 className="text-2xl font-semibold text-slate-900">Privacy policy</h1>
      <p className="mt-4 text-sm leading-relaxed text-slate-600">
        This is a placeholder privacy policy for the TechVault storefront. Replace this copy with your real policy
        before production, including details on data collection, cookies, third parties, and user rights.
      </p>
      <p className="mt-4 text-sm text-slate-600">
        <Link to="/" className="font-medium text-blue-600 hover:underline">
          ← Back to home
        </Link>
      </p>
    </div>
  )
}
