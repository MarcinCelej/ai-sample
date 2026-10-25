import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { useGetCourses } from '@/api/generated/courses/courses'
import { Pagination } from '@/components/common/Pagination'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatMoney, formatNumber } from '@/lib/format'

const PAGE_SIZE = 20

/** The public course catalog. The page number lives in the URL (`?page=2`). */
export function CourseListPage() {
  const { t } = useTranslation(['courses', 'common'])
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)

  const { data, isPending, isError, refetch } = useGetCourses({ page, pageSize: PAGE_SIZE })

  const goToPage = (next: number) => {
    setSearchParams(next === 1 ? {} : { page: String(next) })
  }

  return (
    <section aria-labelledby="courses-title" className="space-y-4">
      <div className="flex items-baseline justify-between">
        <h1 id="courses-title" className="text-2xl font-semibold">
          {t('list.title')}
        </h1>
        {data && <p className="text-sm text-muted-foreground">{t('list.total', { count: data.totalCount })}</p>}
      </div>

      {isPending && (
        <div role="status" aria-live="polite" className="space-y-2">
          <span className="sr-only">{t('list.loading')}</span>
          <Skeleton className="h-8 w-full" />
          <Skeleton className="h-8 w-full" />
          <Skeleton className="h-8 w-full" />
        </div>
      )}

      {isError && (
        <div role="alert" className="space-y-2">
          <p>{t('list.loadError')}</p>
          <Button variant="outline" onClick={() => void refetch()}>
            {t('common:errors.retry')}
          </Button>
        </div>
      )}

      {data?.items.length === 0 && <p>{t('list.emptyState')}</p>}

      {data && data.items.length > 0 && (
        <>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead scope="col">{t('list.columns.name')}</TableHead>
                <TableHead scope="col">{t('list.columns.description')}</TableHead>
                <TableHead scope="col" className="text-right">
                  {t('list.columns.capacity')}
                </TableHead>
                <TableHead scope="col" className="text-right">
                  {t('list.columns.monthlyFee')}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map((course) => (
                <TableRow key={course.id}>
                  <TableCell className="font-medium">{course.name}</TableCell>
                  <TableCell className="text-muted-foreground">{course.description}</TableCell>
                  <TableCell className="text-right">{formatNumber(course.capacity)}</TableCell>
                  <TableCell className="text-right">{formatMoney(course.monthlyFee)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Pagination page={page} pageSize={PAGE_SIZE} totalCount={data.totalCount} onPageChange={goToPage} />
        </>
      )}
    </section>
  )
}
