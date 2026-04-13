import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function AboutPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="About"
        description="Learn about TechVault, our marketplace, and how we help you discover quality tech products."
      />
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">About TechVault</h1>
      <p className="mt-2 text-sm text-slate-500">Last updated March 2026</p>

      <div className="mt-8 space-y-6 text-slate-700">
        <section>
          <h2 className="text-lg font-semibold text-slate-900">Our story</h2>
          <p className="mt-3 text-sm leading-relaxed">
            TechVault started in 2019 as a small side project in a garage in{' '}
            <span className="font-medium text-slate-800">Austin, Texas</span>. Three friends who met at a hackathon
            wanted a simpler way to compare laptops, accessories, and smart-home gear without wading through five
            different tabs. What began as a weekend spreadsheet grew into the storefront you see today.
          </p>
        </section>

        <section>
          <h2 className="text-lg font-semibold text-slate-900">What we do</h2>
          <p className="mt-3 text-sm leading-relaxed">
            We curate electronics and accessories from trusted brands and independent sellers. Our catalog team
            reviews specs, checks compatibility notes, and keeps product pages up to date so you can shop with
            fewer surprises. Whether you are upgrading a workstation or grabbing a last-minute gift, we aim to make
            checkout straightforward and delivery estimates honest.
          </p>
        </section>

        <section>
          <h2 className="text-lg font-semibold text-slate-900">Values</h2>
          <ul className="mt-3 list-inside list-disc space-y-2 text-sm leading-relaxed">
            <li>
              <span className="font-medium text-slate-800">Clarity first</span> — Plain-language descriptions and
              visible return windows on every product page.
            </li>
            <li>
              <span className="font-medium text-slate-800">Support that replies</span> — Our help desk targets a
              first response within one business day (sample SLA for demo purposes).
            </li>
            <li>
              <span className="font-medium text-slate-800">Responsible packaging</span> — We encourage partners to
              use recyclable materials where feasible.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-lg font-semibold text-slate-900">Contact (sample)</h2>
          <p className="mt-3 text-sm leading-relaxed">
            TechVault Demo LLC
            <br />
            742 Evergreen Terrace, Suite 100
            <br />
            Springfield, ST 62704
            <br />
            United States
          </p>
          <p className="mt-2 text-sm leading-relaxed">
            General inquiries:{' '}
            <span className="text-slate-900">hello@techvault.example</span>
            <br />
            Phone (fake): <span className="text-slate-900">+1 (555) 019-2847</span>
          </p>
        </section>
      </div>

      <p className="mt-10 text-sm text-slate-600">
        <Link to="/" className="font-medium text-blue-600 hover:underline">
          ← Back to home
        </Link>
      </p>
    </div>
  )
}
