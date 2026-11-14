'use client';

import { useTranslation } from '@axiomframework/react-core';

import { TableCell, TableFooter, TableRow } from '../../ui/table';

import type { DataTablePaginationFooterProps, Pagination } from './models';
import { AxiomDataTablePagination } from './data-table-pagination';

export function AxiomDataTablePaginationFooter({
  table,
  dataLength,
  colSpan,
  showPageButtons,
}: DataTablePaginationFooterProps) {
  const t = useTranslation();
  const pagination = table as Pagination;

  return (
    <TableFooter>
      <TableRow>
        <TableCell colSpan={colSpan}>
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="text-sm text-muted-foreground">
              {t('AxiomTheme:TableCountInfo', [
                table.getRowModel().rows.length,
                pagination.getRowCount?.() ?? dataLength,
              ])}
            </span>
            <AxiomDataTablePagination
              table={table}
              showPageButtons={showPageButtons}
            />
          </div>
        </TableCell>
      </TableRow>
    </TableFooter>
  );
}
