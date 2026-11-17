'use client';

import { useTable } from '@tanstack/react-table';

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

import {
  DataTableDndContext,
  DraggableCell,
  DraggableTableHeader,
  useDataTableColumnOrder,
} from './draggable';

import { type Pagination, AxiomDataTablePagination } from './pagination/index';

import type { DataTableProps, OptionalRow } from './models';

export function AxiomDataTable(props: DataTableProps) {
  const t = useTranslation();
  const { columnOrder, setColumnOrder, sensors, handleDragEnd } =
    useDataTableColumnOrder(props.columns);

  const table = useTable({
    ...props,
    state: {
      columnOrder,
    },
    onColumnOrderChange: setColumnOrder,
    columnResizeMode: 'onChange',
  });

  const pagination = table as Pagination;
  const dataLength = props.data.length;
  const colSpan = table.getHeaderGroups().at(-1)?.headers.length ?? props.columns.length;

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

  return (
    <div className="overflow-hidden rounded border">
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
              const optionalRow = row as OptionalRow;
              const cells =
                optionalRow.getVisibleCells?.() ?? row.getAllCells();

              return (
                <TableRow
                  key={row.id}
                  data-state={optionalRow.getIsSelected?.() && 'selected'}
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
  );
}
