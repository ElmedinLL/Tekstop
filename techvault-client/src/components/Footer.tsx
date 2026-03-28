import { type FormEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { toast } from '../lib/notifications'

/** Replace with your real profiles when ready. */
const SOCIAL_LINKS = [
  {
    label: 'X (Twitter)',
    href: 'https://twitter.com',
    icon: SocialX,
  },
  {
    label: 'Facebook',
    href: 'https://www.facebook.com',
    icon: SocialFacebook,
  },
  {
    label: 'Instagram',
    href: 'https://www.instagram.com',
    icon: SocialInstagram,
  },
  {
    label: 'GitHub',
    href: 'https://github.com',
    icon: SocialGitHub,
  },
] as const

type LinkCol = { title: string; links: { to: string; label: string }[] }

const LINK_COLUMNS: LinkCol[] = [
  {
    title: 'Shop',
    links: [
      { to: '/', label: 'Home' },
      { to: '/search', label: 'Search' },
      { to: '/cart', label: 'Cart' },
      { to: '/compare', label: 'Compare' },
      { to: '/checkout', label: 'Checkout' },
    ],
  },
  {
    title: 'Account',
    links: [
      { to: '/login', label: 'Sign in' },
      { to: '/register', label: 'Create account' },
      { to: '/account', label: 'Profile' },
      { to: '/orders', label: 'My orders' },
      { to: '/wishlist', label: 'Wishlist' },
    ],
  },
  {
    title: 'Company',
    links: [
      { to: '/about', label: 'About' },
      { to: '/search', label: 'Browse catalog' },
    ],
  },
  {
    title: 'Legal',
    links: [
      { to: '/privacy', label: 'Privacy policy' },
      { to: '/terms', label: 'Terms of service' },
    ],
  },
]

function SocialX({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
    </svg>
  )
}

function SocialFacebook({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
    </svg>
  )
}

function SocialInstagram({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z" />
    </svg>
  )
}

function SocialGitHub({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z"
      />
    </svg>
  )
}

export function Footer() {
  const [email, setEmail] = useState('')
  const year = new Date().getFullYear()

  const onNewsletter = (e: FormEvent) => {
    e.preventDefault()
    const trimmed = email.trim()
    if (!trimmed) {
      toast.error('Enter your email address.')
      return
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
      toast.error('Enter a valid email address.')
      return
    }
    toast.success("You're on the list!", {
      description: 'We will share product drops and offers at this address.',
    })
    setEmail('')
  }

  return (
    <footer className="border-t border-slate-800 bg-slate-900 text-slate-300">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:py-14">
        <div className="flex flex-col gap-12 lg:flex-row lg:gap-10">
          <div className="grid flex-1 grid-cols-1 gap-10 sm:grid-cols-2 lg:grid-cols-4 lg:gap-8">
            {LINK_COLUMNS.map((col) => (
              <div key={col.title}>
                <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-400">{col.title}</h2>
                <ul className="mt-4 space-y-3">
                  {col.links.map((item) => (
                    <li key={item.to + item.label}>
                      <Link
                        to={item.to}
                        className="text-sm text-slate-300 transition hover:text-white hover:underline"
                      >
                        {item.label}
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>

          <div className="shrink-0 lg:max-w-sm lg:border-l lg:border-slate-800 lg:pl-10">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-400">Newsletter</h2>
            <p className="mt-3 text-sm leading-relaxed text-slate-400">
              Get launch news, seasonal deals, and curated picks. No spam — unsubscribe anytime.
            </p>
            <form onSubmit={onNewsletter} className="mt-4 flex flex-col gap-2 sm:flex-row sm:items-stretch">
              <label htmlFor="footer-newsletter-email" className="sr-only">
                Email for newsletter
              </label>
              <input
                id="footer-newsletter-email"
                type="email"
                name="email"
                autoComplete="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="min-w-0 flex-1 rounded-lg border border-slate-700 bg-slate-800/50 px-3 py-2.5 text-sm text-white placeholder:text-slate-500 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30"
              />
              <button
                type="submit"
                className="rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-400 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-900 sm:shrink-0"
              >
                Subscribe
              </button>
            </form>
          </div>
        </div>

        <div className="mt-12 flex flex-col items-center gap-6 border-t border-slate-800 pt-10 sm:flex-row sm:justify-between sm:gap-8">
          <div className="flex flex-wrap items-center justify-center gap-3 sm:justify-start">
            {SOCIAL_LINKS.map(({ label, href, icon: Icon }) => (
              <a
                key={label}
                href={href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={label}
                className="flex h-10 w-10 items-center justify-center rounded-lg text-slate-400 transition hover:bg-slate-800 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
              >
                <Icon className="h-5 w-5" />
              </a>
            ))}
          </div>
          <p className="text-center text-xs text-slate-500 sm:text-right">
            © {year} TechVault. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  )
}
