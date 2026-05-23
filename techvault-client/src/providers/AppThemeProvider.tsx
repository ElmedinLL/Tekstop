import { ThemeProvider } from 'next-themes'

/**
 * SPA equivalent of Next.js + next-themes: drives `class="dark"` on &lt;html&gt; for Tailwind darkMode.
 */
export function AppThemeProvider({ children }: { children: React.ReactNode }) {
  return (
    <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange={false}>
      {children}
    </ThemeProvider>
  )
}
