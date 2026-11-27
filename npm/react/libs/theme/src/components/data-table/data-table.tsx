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
    <>
      <div
        aria-label="Data table options section"
        className="flex flex-wrap items-center justify-end gap-2"
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

      <div className="overflow-hidden rounded-lg border">
        <DataTableDndContext onDragEnd={handleDragEnd} sensors={sensors}>
          <Table>
            <TableHeader>
              {table.getHeaderGroups().map((headerGroup) => (
                <TableRow
                  key={headerGroup.id}
                  className="bg-muted/50 [&>th]:border-t-0"
                >
                  <SortableContext
                    items={columnOrder}
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
                    data-state={row.getIsSelected?.() && 'selected'}
                  >
                    {cells.map((cell) => (
                      <SortableContext
                        key={cell.id}
                        items={columnOrder}
                        strategy={horizontalListSortingStrategy}
                      >
                        <DraggableCell key={cell.id} cell={cell} />
                      </SortableContext>
                    ))}
                  </TableRow>
                );
              })}
            </TableBody>

            <TableFooter>
              <TableRow>
                <TableCell colSpan={colSpan}>
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
    </>
  );
}
