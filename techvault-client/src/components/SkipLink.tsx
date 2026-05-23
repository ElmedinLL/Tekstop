/**
 * P226: Hidden until focused skip link — must be among first focusables in DOM order after route shell.
 */
export function SkipLink() {
  return (
    <a
      href="#main-content"
      className="fixed left-3 top-3 z-[100] -translate-y-28 rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white shadow-lg outline-none ring-blue-600 transition focus-visible:translate-y-0 focus-visible:ring-2 focus-visible:ring-offset-2"
    >
      Skip to main content
    </a>
  )
}
