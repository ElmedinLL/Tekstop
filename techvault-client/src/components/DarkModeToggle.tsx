import { useTheme } from 'next-themes'
import { useEffect, useState } from 'react'

/**
 * P220: Uses next-themes setTheme — sun/moon with smooth CSS transition via global * selector (index.css).
 */
export function DarkModeToggle({ className = '' }: { className?: string }) {
  const { theme, setTheme, resolvedTheme } = useTheme()
  const [mounted, setMounted] = useState(false)
  useEffect(() => setMounted(true), [])

  const current = resolvedTheme ?? theme
  const isDark = current === 'dark'

  const label = isDark ? 'Switch to light mode' : 'Switch to dark mode'

  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      className={`group relative inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-800 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-600 focus-visible:ring-offset-2 dark:border-slate-700 dark:bg-slate-950 dark:text-amber-200 dark:hover:bg-slate-900 ${className}`.trim()}
      disabled={!mounted}
      onClick={() => setTheme(isDark ? 'light' : 'dark')}
    >
      <SunIcon className={`h-[1.35rem] w-[1.35rem] transition-transform duration-500 ${isDark ? 'scale-75 rotate-90 opacity-0' : 'scale-100 rotate-0 opacity-100'}`} />
      <MoonIcon className={`absolute h-[1.35rem] w-[1.35rem] transition-transform duration-500 ${isDark ? 'scale-100 rotate-0 opacity-100' : 'scale-75 -rotate-90 opacity-0'}`} />
    </button>
  )
}

function SunIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <circle cx="12" cy="12" r="4.5" strokeWidth="1.6" />
      <path strokeWidth="1.6" strokeLinecap="round" d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41" />
    </svg>
  )
}

function MoonIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden>
      <path
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M21 13.25A8.75 8.75 0 0010.75 3a9 9 0 109.25 10.25z"
      />
    </svg>
  )
}
