import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router'

export function AppLayout() {
  const { t } = useTranslation()

  return (
    <div className="min-h-svh">
      <header className="border-b">
        <div className="mx-auto flex max-w-5xl items-center gap-6 px-4 py-3">
          <span className="font-semibold">{t('appName')}</span>
          <nav aria-label={t('nav.main')}>
            <NavLink
              to="/courses"
              className="text-sm underline-offset-4 hover:underline aria-[current=page]:font-medium"
            >
              {t('nav.courses')}
            </NavLink>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
