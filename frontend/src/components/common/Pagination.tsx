import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

interface PaginationProps {
  page: number
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
}

/** Previous/next paging for `{ items, totalCount }` lists. Hidden when everything fits on one page. */
export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps) {
  const { t } = useTranslation()
  const pageCount = Math.max(1, Math.ceil(totalCount / pageSize))

  if (pageCount === 1) {
    return null
  }

  return (
    <nav aria-label={t('pagination.label')} className="flex items-center justify-end gap-2">
      <Button
        variant="outline"
        size="icon"
        aria-label={t('pagination.previous')}
        disabled={page <= 1}
        onClick={() => {
          onPageChange(page - 1)
        }}
      >
        <ChevronLeft aria-hidden />
      </Button>
      <span className="text-sm">{t('pagination.pageOf', { page, pageCount })}</span>
      <Button
        variant="outline"
        size="icon"
        aria-label={t('pagination.next')}
        disabled={page >= pageCount}
        onClick={() => {
          onPageChange(page + 1)
        }}
      >
        <ChevronRight aria-hidden />
      </Button>
    </nav>
  )
}
