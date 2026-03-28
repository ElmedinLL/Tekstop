import { useLocation } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { Icon404 } from '../components/error/ErrorIllustrations'
import { ErrorPageCard, ErrorPrimaryLink, ErrorSecondaryLink } from '../components/error/ErrorPageShell'

export function NotFoundPage() {
  const location = useLocation()
  const isAdmin = location.pathname.startsWith('/admin')

  return (
    <div className={isAdmin ? 'py-6' : 'mx-auto max-w-lg px-4 py-12 sm:py-16'}>
      <Seo
        title="Page not found"
        description="The page you requested does not exist on TechVault."
        noindex
      />
      <ErrorPageCard
        badge="404"
        icon={<Icon404 className="h-10 w-10" />}
        title="Page not found"
        description="That URL doesn’t match anything in our store. Try search, head home, or use the menu above."
        actions={
          <>
            <ErrorPrimaryLink to="/">Go to home</ErrorPrimaryLink>
            <ErrorSecondaryLink to="/search">Search products</ErrorSecondaryLink>
          </>
        }
      />
    </div>
  )
}
