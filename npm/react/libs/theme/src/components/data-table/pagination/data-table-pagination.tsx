import type { TableState_RowPagination } from '@tanstack/react-table';

import { HugeiconsIcon } from '@hugeicons/react';
import {
  ArrowLeft01Icon,
  ArrowRight01Icon,
  ArrowLeftDoubleIcon,
  ArrowRightDoubleIcon,
} from '@hugeicons/core-free-icons';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../ui/select';
import { Button } from '../../ui/button';
import { Separator } from '../../ui/separator';
import type { DataTablePaginationProps, Pagination } from './models';

export function AxiomDataTablePagination({
  table,
  showPageButtons = true,
}: DataTablePaginationProps) {
  const pagination = table as Pagination;
  const tableState = table.state as Partial<TableState_RowPagination>;
  const paginationState = tableState.pagination;

  if (!paginationState || !pagination.previousPage) {
    return null;
  }

  const pageCount = pagination.getPageCount?.() ?? 0;

  return (
    <div className="flex flex-wrap items-center justify-end gap-2">
      <Select
        value={paginationState.pageSize}
        onValueChange={(value) => {
          if (value !== null) pagination.setPageSize?.(value);
        }}
      >
        <SelectTrigger size="sm" aria-label="Rows per page">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {[5, 10, 20, 50, 100].map((size) => (
            <SelectItem key={size} value={size}>
              {size}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Separator orientation="vertical" className="h-5" />

      <Select
        value={pageCount === 0 ? null : paginationState.pageIndex}
        disabled={pageCount <= 0}
        onValueChange={(value) => {
          if (value !== null) pagination.setPageIndex?.(value);
        }}
      >
        <SelectTrigger size="sm" aria-label="Page">
          <SelectValue>
            {pageCount === 0 ? 0 : paginationState.pageIndex + 1}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          {pagination.getPageOptions?.().map((index) => (
            <SelectItem key={index} value={index}>
              {index + 1}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {showPageButtons && (
        <>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label="First page"
            onClick={() => pagination.firstPage?.()}
            disabled={!pagination.getCanPreviousPage?.()}
          >
            <HugeiconsIcon icon={ArrowLeftDoubleIcon} />
          </Button>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label="Previous page"
            onClick={() => pagination.previousPage?.()}
            disabled={!pagination.getCanPreviousPage?.()}
          >
            <HugeiconsIcon icon={ArrowLeft01Icon} />
          </Button>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label="Next page"
            onClick={() => pagination.nextPage?.()}
            disabled={!pagination.getCanNextPage?.()}
          >
            <HugeiconsIcon icon={ArrowRight01Icon} />
          </Button>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label="Last page"
            onClick={() => pagination.lastPage?.()}
            disabled={!pagination.getCanLastPage?.()}
          >
            <HugeiconsIcon icon={ArrowRightDoubleIcon} />
          </Button>
        </>
      )}
    </div>
  );
}
