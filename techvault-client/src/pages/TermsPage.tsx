import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function TermsPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="Terms of service"
        description="Terms and conditions for using the TechVault marketplace."
      />
      <h1 className="text-2xl font-semibold text-slate-900">Terms of service</h1>
      <p className="mt-4 text-sm leading-relaxed text-slate-600">
        This is a placeholder terms of service page. Replace with your legal terms covering orders, payments,
        returns, liability, and acceptable use before going live.
      </p>
      <p className="mt-4 text-sm text-slate-600">
        <Link to="/" className="font-medium text-blue-600 hover:underline">
          ← Back to home
        </Link>
      </p>
    </div>
  )
}
