import type { SiteAnnouncement } from '../components/AnnouncementBanner'

/** Site-wide storefront announcements — swap for CMS-backed data when available. */
export const defaultSiteAnnouncements: readonly SiteAnnouncement[] = [
  {
    id: 'storefront-tracking-info',
    type: 'info',
    title: 'Order tracking',
    message:
      'You can track order status anytime from Account → Orders. Full shipping history appears after checkout.',
    scope: 'site',
  },
]
