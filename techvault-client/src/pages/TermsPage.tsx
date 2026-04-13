import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function TermsPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="Terms of service"
        description="Terms and conditions for using the TechVault marketplace."
      />
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Terms of service</h1>
      <p className="mt-2 text-sm text-slate-500">Effective date: March 1, 2026 · Sample text for development only</p>

      <div className="mt-8 space-y-8 text-sm leading-relaxed text-slate-700">
        <section>
          <h2 className="text-base font-semibold text-slate-900">1. Agreement</h2>
          <p className="mt-3">
            By accessing or using the TechVault website and services (“Services”), you agree to these Terms. If you do
            not agree, do not use the Services. We may update these Terms from time to time; the “Effective date” at
            the top will change when we do. Continued use after changes means you accept the updated Terms.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">2. Eligibility &amp; accounts</h2>
          <p className="mt-3">
            You must be at least 18 years old (or the age of majority where you live) to make purchases. You are
            responsible for keeping your account credentials confidential and for all activity under your account.
            Notify us promptly at <span className="text-slate-900">security@techvault.example</span> if you suspect
            unauthorized use.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">3. Orders &amp; pricing</h2>
          <p className="mt-3">
            Product listings, prices, and availability are subject to change without notice until you complete
            checkout. We reserve the right to cancel orders affected by pricing errors, suspected fraud, or stock
            issues. Taxes and shipping are estimated at checkout and may be adjusted on the final invoice where
            required by law.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">4. Payments</h2>
          <p className="mt-3">
            Payments are processed by third-party providers (for example, card networks and payment processors). You
            authorize us and our partners to charge your selected payment method for the total amount shown at
            checkout. Failed payments may result in order cancellation.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">5. Shipping &amp; returns (sample)</h2>
          <p className="mt-3">
            Estimated delivery dates are not guaranteed. For this demo storefront, return windows and restocking fees
            are described on individual product pages and in your order confirmation email. Open-box and clearance
            items may be final sale unless otherwise stated.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">6. Acceptable use</h2>
          <p className="mt-3">
            You may not use the Services to violate law, harass others, scrape or overload our systems, attempt
            unauthorized access, or resell access to your account. We may suspend or terminate access for violations.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">7. Disclaimer &amp; limitation of liability</h2>
          <p className="mt-3">
            The Services are provided “as is” to the maximum extent permitted by law. TechVault Demo LLC and its
            affiliates are not liable for indirect, incidental, or consequential damages arising from your use of the
            Services. Some jurisdictions do not allow certain limitations; in those cases our liability is limited
            to the fullest extent permitted.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">8. Contact</h2>
          <p className="mt-3">
            Questions about these Terms: <span className="text-slate-900">legal@techvault.example</span>
            <br />
            Mailing address (sample): TechVault Demo LLC, 742 Evergreen Terrace, Suite 100, Springfield, ST 62704,
            USA.
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
