'use client';
import { useTranslation } from '@axiomframework/react-core';
import {
  useTable,
  type ReactTable,
  type HeaderGroup,
  type RowData,
  type Table_RowPagination,
  type TableFeatures,
} from '@tanstack/react-table';

import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';

import type { DataTableProps, OptionalRow } from './models';
import { AxiomDataTablePagination } from './pagination/data-table-pagination';

export function AxiomDataTable<T extends TableFeatures, D extends RowData>(
  props: DataTableProps<T, D>,
) {
  const t = useTranslation();
  const table = useTable(props);
  const pagination = table as Partial<Table_RowPagination<T, D>>;

  return (
    <div className="overflow-hidden rounded border">
      <Table>
        <TableHeader>
          {table
            .getHeaderGroups()
            .map((headerGroup) => renderHeaderRow(headerGroup, table, t))}
        </TableHeader>

        <TableBody>
          {table.getRowModel().rows?.length ? (
            table.getRowModel().rows.map((row) => {
              const optionalRow = row as OptionalRow<T, D>;
              const cells =
                optionalRow.getVisibleCells?.() ?? row.getAllCells();

              return (
                <TableRow
                  key={row.id}
                  data-state={optionalRow.getIsSelected?.() && 'selected'}
                >
                  {cells.map((cell) => (
                    <TableCell key={cell.id}>
                      <table.FlexRender cell={cell} />
                    </TableCell>
                  ))}
                </TableRow>
              );
            })
          ) : (
            <TableRow>
              <TableCell
                colSpan={
                  table.getHeaderGroups().at(-1)?.headers.length ??
                  props.columns.length
                }
                className="h-24 text-center"
              >
                {t('AxiomTheme:NoRecordFound')}
              </TableCell>
            </TableRow>
          )}
        </TableBody>

        <TableFooter>
          <TableRow>
            <TableCell
              colSpan={
                table.getHeaderGroups().at(-1)?.headers.length ??
                props.columns.length
              }
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="text-sm text-muted-foreground">
                  {t('AxiomTheme:TableCountInfo', [
                    table.getRowModel().rows.length,
                    pagination.getRowCount?.() ?? props.data.length,
                  ])}
                </span>
                <AxiomDataTablePagination
                  table={table}
                  showPageButtons={props.showPageButtons}
                />
              </div>
            </TableCell>
          </TableRow>
        </TableFooter>
      </Table>
    </div>
  );
}

function renderHeaderRow<T extends TableFeatures, D extends RowData>(
  headerGroup: HeaderGroup<T, D>,
  table: ReactTable<T, D>,
  t: ReturnType<typeof useTranslation>,
) {
  const key = headerGroup.id;
  const headers = headerGroup.headers;

  return (
    <TableRow key={key}>
      {headers.map((header) => {
        const { column } = header;
        let content = table.FlexRender({ header });
        const isString = typeof column.columnDef.header === 'string';

        if (header.isPlaceholder !== null && isString) {
          content = t(column.columnDef.header as string);
        }

        return <TableHead key={header.id}>{content}</TableHead>;
      })}
    </TableRow>
  );
}
