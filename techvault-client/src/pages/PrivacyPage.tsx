import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function PrivacyPage() {
  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <Seo
        title="Privacy policy"
        description="How TechVault collects, uses, and protects your information."
      />
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Privacy policy</h1>
      <p className="mt-2 text-sm text-slate-500">Effective date: March 1, 2026 · Sample policy for development only</p>

      <div className="mt-8 space-y-8 text-sm leading-relaxed text-slate-700">
        <section>
          <h2 className="text-base font-semibold text-slate-900">Who we are</h2>
          <p className="mt-3">
            TechVault Demo LLC (“TechVault,” “we,” “us”) operates this website and related services. This policy
            describes how we handle personal information in connection with the demo storefront. Replace this text
            with counsel-reviewed language before production.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Information we collect</h2>
          <ul className="mt-3 list-inside list-disc space-y-2">
            <li>
              <span className="font-medium text-slate-800">Account &amp; profile:</span> name, email address, phone
              number if you provide it, and password (stored hashed by our identity provider).
            </li>
            <li>
              <span className="font-medium text-slate-800">Orders &amp; payments:</span> shipping and billing
              addresses, order history, and payment metadata processed by our payment partners (we do not store full
              card numbers on our servers in typical configurations).
            </li>
            <li>
              <span className="font-medium text-slate-800">Device &amp; usage:</span> approximate location from IP
              address, browser type, pages viewed, and timestamps — used to secure accounts and improve the site.
            </li>
            <li>
              <span className="font-medium text-slate-800">Support:</span> messages you send to customer support,
              including attachments you choose to include.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">How we use information</h2>
          <p className="mt-3">
            We use the data above to create and manage accounts, process and ship orders, send transactional emails
            (order confirmations, shipping updates), detect fraud and abuse, analyze aggregate traffic patterns, and
            comply with legal obligations. With your consent where required, we may also send marketing emails; you
            can opt out via the link in those messages.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Cookies &amp; similar technologies</h2>
          <p className="mt-3">
            We use cookies and local storage to keep you signed in, remember cart contents, and measure basic
            analytics. You can control cookies through your browser settings; disabling some cookies may limit
            certain features (for example, staying logged in).
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Sharing</h2>
          <p className="mt-3">
            We share information with service providers who help us run the site — hosting, email delivery, payment
            processing, and shipping carriers. They may only use data as instructed by us. We may disclose information
            if required by law or to protect the rights, safety, and security of TechVault, our users, or the public.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Retention</h2>
          <p className="mt-3">
            We keep account and order records for as long as your account is active and for a reasonable period
            afterward for legal, tax, and dispute-resolution purposes. Marketing preferences are honored until you
            withdraw consent or close your account.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Your choices &amp; rights</h2>
          <p className="mt-3">
            Depending on where you live, you may have rights to access, correct, delete, or export your personal
            information, or to object to certain processing. To exercise these rights, contact{' '}
            <span className="text-slate-900">privacy@techvault.example</span>. We may verify your identity before
            fulfilling requests.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Children</h2>
          <p className="mt-3">
            Our Services are not directed at children under 13 (or 16 where applicable). We do not knowingly collect
            personal information from children.
          </p>
        </section>

        <section>
          <h2 className="text-base font-semibold text-slate-900">Contact</h2>
          <p className="mt-3">
            Data protection inquiries: <span className="text-slate-900">privacy@techvault.example</span>
            <br />
            TechVault Demo LLC, 742 Evergreen Terrace, Suite 100, Springfield, ST 62704, United States.
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
