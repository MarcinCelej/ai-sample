import { Navigate, type RouteObject } from 'react-router'
import { AppLayout } from './AppLayout'

/** Route table. Feature pages are lazy-loaded. */
export const routes: RouteObject[] = [
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/courses" replace /> },
      {
        path: 'courses',
        lazy: async () => ({ Component: (await import('@/features/courses/pages/CourseListPage')).CourseListPage }),
      },
    ],
  },
]
