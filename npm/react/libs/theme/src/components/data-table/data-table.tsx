'use client';

import { useTable } from '@tanstack/react-table';

import {
  horizontalListSortingStrategy,
  SortableContext,
} from '@dnd-kit/sortable';

import { HugeiconsIcon } from '@hugeicons/react';
import { FilterIcon } from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHeader,
  TableRow,
} from '../ui/table';

import {
  DataTableDndContext,
  DraggableCell,
  DraggableTableHeader,
  useColumnOrder,
} from './draggable/index';

import { Button } from '../ui/button';
import { Input } from '../ui/input';

import { type Pagination, AxiomDataTablePagination } from './pagination/index';
import {
  ColumnVisibilitySettings,
  useColumnVisibility,
} from './column-visiblity/index';
import type { DataTableProps } from './models';

export function AxiomDataTable(props: DataTableProps) {
  const t = useTranslation();
  const { columnOrder, setColumnOrder, sensors, handleDragEnd } =
    useColumnOrder(props.columns);

  const { columnVisibility, setColumnVisibility } = useColumnVisibility(
    props.columns,
  );

  const table = useTable({
    ...props,
    state: {
      columnOrder,
      columnVisibility,
    },
    onColumnOrderChange: setColumnOrder,
    columnResizeMode: 'onChange',
    onColumnVisibilityChange: setColumnVisibility,
  });

  const pagination = table as Pagination;
  const dataLength = props.data.length;

  const headers = table.getHeaderGroups().at(-1)?.headers;
  const colSpan = headers?.length ?? props.columns.length;

  const NoRecordFound = () => (
    <TableRow>
      <TableCell
        colSpan={
          table.getHeaderGroups().at(-1)?.headers.length ?? props.columns.length
        }
        className="h-24 text-center"
      >
        {t('AxiomTheme:NoRecordFound')}
      </TableCell>
    </TableRow>
  );

  const columns = table.getAllColumns().filter((column) => column.getCanHide());

  return (
    <>
      <div
        className="flex items-center justify-end gap-2"
        aria-label="Data table options"
      >
        <ColumnVisibilitySettings columns={columns} table={table} />
        <Filters t={t} />
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
              {!table.getRowModel().rows?.length && <NoRecordFound />}

              {table?.getRowModel()?.rows?.map((row) => {
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
                    <span className="text-sm text-muted-foreground">
                      {t('AxiomTheme:TableCountInfo', [
                        table.getRowModel().rows.length,
                        pagination.getRowCount?.() ?? dataLength,
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
        </DataTableDndContext>
      </div>
    </>
  );
}

function Filters({ t }: { t: ReturnType<typeof useTranslation> }) {
  return (
    <>
      <Input className="max-w-3xs" placeholder="Search" />
      <Button
        variant="outline"
        className="max-w-3xs justify-between cursor-pointer"
      >
        <span className="flex items-center gap-2">
          {t('AxiomTheme:Filters')}
          <HugeiconsIcon icon={FilterIcon} />
        </span>
      </Button>
    </>
  );
}
