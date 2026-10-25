import { screen, within } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import type { GetCoursesItem, PagedResponseOfGetCoursesItem } from '@/api/generated/model'
import { renderRoute } from '@/test/render'
import { server } from '@/test/server'

const chess: GetCoursesItem = {
  id: '6f1c2a3b-0000-4000-8000-000000000001',
  name: 'Szachy',
  description: 'Dla początkujących',
  capacity: 12,
  monthlyFee: 150.5,
}

function respondWith(page: PagedResponseOfGetCoursesItem, onRequest?: (url: URL) => void) {
  server.use(
    http.get('*/api/courses', ({ request }) => {
      onRequest?.(new URL(request.url))
      return HttpResponse.json(page)
    }),
  )
}

describe('CourseListPage', () => {
  it('shows the courses with formatted values', async () => {
    respondWith({ items: [chess], totalCount: 1 })

    renderRoute('/courses')

    const row = await screen.findByRole('row', { name: /Szachy/ })
    expect(within(row).getByText('Dla początkujących')).toBeInTheDocument()
    expect(within(row).getByText('12')).toBeInTheDocument()
    expect(within(row).getByText(/150,50\s*zł/)).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Kursy' })).toBeInTheDocument()
    expect(screen.getByText('1 kurs')).toBeInTheDocument()
  })

  it('redirects the root to the course list', async () => {
    respondWith({ items: [], totalCount: 0 })

    const { router } = renderRoute('/')

    expect(await screen.findByRole('heading', { name: 'Kursy' })).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/courses')
  })

  it('shows an empty state when there are no courses', async () => {
    respondWith({ items: [], totalCount: 0 })

    renderRoute('/courses')

    expect(await screen.findByText('Nie ma jeszcze żadnych kursów.')).toBeInTheDocument()
    expect(screen.getByText('0 kursów')).toBeInTheDocument()
  })

  it('shows an error with a retry button when loading fails', async () => {
    server.use(http.get('*/api/courses', () => HttpResponse.json({ status: 500 }, { status: 500 })))

    const { user } = renderRoute('/courses')

    const alert = await screen.findByRole('alert')
    expect(within(alert).getByText('Nie udało się wczytać kursów.')).toBeInTheDocument()

    respondWith({ items: [chess], totalCount: 1 })
    await user.click(within(alert).getByRole('button', { name: 'Spróbuj ponownie' }))
    expect(await screen.findByRole('row', { name: /Szachy/ })).toBeInTheDocument()
  })

  it('reads the page from the URL and pages through the results', async () => {
    const requestedPages: string[] = []
    respondWith({ items: [chess], totalCount: 45 }, (url) => requestedPages.push(url.searchParams.get('page') ?? ''))

    const { user, router } = renderRoute('/courses?page=2')

    expect(await screen.findByText('Strona 2 z 3')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Następna strona' }))
    expect(await screen.findByText('Strona 3 z 3')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Następna strona' })).toBeDisabled()

    await user.click(screen.getByRole('button', { name: 'Poprzednia strona' }))
    await user.click(await screen.findByRole('button', { name: 'Poprzednia strona' }))
    expect(await screen.findByText('Strona 1 z 3')).toBeInTheDocument()
    expect(router.state.location.search).toBe('')
    expect(requestedPages).toEqual(['2', '3', '2', '1'])
  })
})
