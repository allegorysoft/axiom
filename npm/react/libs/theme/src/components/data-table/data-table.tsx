'use client';

import { useEffect } from 'react';
import { type RowData, useTable } from '@tanstack/react-table';

import {
  horizontalListSortingStrategy,
  SortableContext,
} from '@dnd-kit/sortable';

import { useTranslation } from '@axiomframework/react-core';

import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHeader,
  TableRow,
} from '../ui/table';

import { DataTableFilter } from './filtering/index';
import {
  DataTableDndContext,
  DraggableCell,
  DraggableTableHeader,
  useColumnOrder,
} from './draggable/index';
import {
  ColumnVisibilitySettings,
  useColumnVisibility,
} from './column-visiblity/index';
import { AxiomDataTablePagination } from './pagination/index';

import type { DataTableProps } from './models';
import { isFixedOrderColumn } from './data-table-utils';
import { AppPageTitle } from '../app-layout/app-page-title';

export function AxiomDataTable<D extends RowData>({
  pageTitle,
  ...props
}: DataTableProps<D>) {
  const t = useTranslation();
  const { columnVisibility, setColumnVisibility } = useColumnVisibility();
  const { columnOrder, setColumnOrder, sensors, handleDragEnd } =
    useColumnOrder<D>(props.columns);

  const table = useTable({
    ...props,
    state: {
      ...props.state,
      columnOrder,
      columnVisibility,
    },
    initialState: {
      pagination: { pageIndex: 0, pageSize: 5 },
    },
    onColumnOrderChange: setColumnOrder,
    columnResizeMode: 'onChange',
    onColumnVisibilityChange: setColumnVisibility,
  });

  const headers = table.getHeaderGroups().at(-1)?.headers;
  const colSpan = headers?.length ?? props.columns.length;
  const columns = table.getAllColumns().filter((column) => column.getCanHide());
  const rows = table?.getRowModel()?.rows;
  const visibleColumns = table.getVisibleLeafColumns();
  const sortableOrder = visibleColumns
    .filter((column) => !isFixedOrderColumn(column.columnDef))
    .map((column) => column.id);
  const fixedWidths = new Map(
    visibleColumns
      .filter((column) => isFixedOrderColumn(column.columnDef))
      .map((column) => [column.id, column.getSize()]),
  );
  const fixedWidth = Array.from(fixedWidths.values()).reduce(
    (total, width) => total + width,
    0,
  );
  const dataWidth = visibleColumns.reduce(
    (total, column) =>
      total + (fixedWidths.has(column.id) ? 0 : column.getSize()),
    0,
  );

  useEffect(() => {
    const notify = () => {
      props.load?.({
        sorting: table.atoms.sorting.get() ?? [],
        pagination: table.atoms.pagination.get(),
        filters: {},
      });
    };

    const _sorting = table.atoms.sorting.subscribe(notify);
    const _pagination = table.atoms.pagination.subscribe(notify);
    notify();

    return () => {
      _sorting.unsubscribe();
      _pagination.unsubscribe();
    };
  }, [props.load, table]);

  return (
    <div
      data-slot="data-table"
      className="min-w-0 overflow-hidden rounded-xl border border-border/70 bg-card text-card-foreground in-data-[radius=lg]:rounded-[26px]"
    >
      <div
        aria-label="Data table options section"
        className="flex flex-wrap items-center justify-end gap-2 border-b border-border/70 px-4 py-3"
      >
        {pageTitle && (
          <div className="mr-auto">
            <AppPageTitle title={pageTitle} />
          </div>
        )}
        <DataTableFilter filterOptions={props.filterOptions!} />

        <ColumnVisibilitySettings
          columns={columns}
          resetColumnVisibility={table.resetColumnVisibility}
        />
      </div>

      <div className="@container">
        <DataTableDndContext onDragEnd={handleDragEnd} sensors={sensors}>
          <Table
            className="table-fixed [&_td]:px-4 [&_th]:px-4"
            style={{
              width:
                dataWidth > 0
                  ? `max(100cqw, ${dataWidth + fixedWidth}px)`
                  : fixedWidth || undefined,
            }}
          >
            <colgroup>
              {visibleColumns.map((column) => {
                const fraction = column.getSize() / dataWidth;
                const width =
                  fixedWidths.get(column.id) ??
                  `max(${column.getSize()}px, calc(${fraction * 100}cqw - ${fraction * fixedWidth}px))`;

                return <col key={column.id} style={{ width }} />;
              })}
            </colgroup>
            <TableHeader>
              {table.getHeaderGroups().map((headerGroup) => (
                <TableRow
                  key={headerGroup.id}
                  className="border-border/70 hover:bg-transparent [&>th]:border-t-0"
                >
                  <SortableContext
                    items={sortableOrder}
                    strategy={horizontalListSortingStrategy}
                  >
                    {headerGroup.headers.map((header) => (
                      <DraggableTableHeader
                        key={header.id}
                        header={header}
                        table={table}
                        columnResizing={props.columnResizing}
                      />
                    ))}
                  </SortableContext>
                </TableRow>
              ))}
            </TableHeader>

            <TableBody>
              {!rows?.length && (
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

              {rows?.map((row) => {
                const visibleCells = row.getVisibleCells?.();
                const cells = visibleCells ?? row.getAllCells();

                return (
                  <TableRow
                    key={row.id}
                    className="border-border/50"
                    data-state={row.getIsSelected?.() && 'selected'}
                  >
                    <SortableContext
                      items={sortableOrder}
                      strategy={horizontalListSortingStrategy}
                    >
                      {cells.map((cell) => (
                        <DraggableCell key={cell.id} cell={cell} />
                      ))}
                    </SortableContext>
                  </TableRow>
                );
              })}
            </TableBody>

            <TableFooter className="border-border/70 bg-transparent">
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={colSpan} className="py-3">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <AxiomDataTablePagination
                      table={table}
                      paginationOptions={props.paginationOptions}
                    />
                  </div>
                </TableCell>
              </TableRow>
            </TableFooter>
          </Table>
        </DataTableDndContext>
      </div>
    </div>
  );
}
