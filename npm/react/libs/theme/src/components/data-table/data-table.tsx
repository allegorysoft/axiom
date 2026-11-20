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
      ...props.state,
      columnOrder,
      columnVisibility,
    },
    onColumnOrderChange: setColumnOrder,
    columnResizeMode: 'onChange',
    onColumnVisibilityChange: setColumnVisibility,
  });

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
        aria-label="Data table options section"
        className="flex items-center justify-end gap-2 mx-2 sm:mx-0.5"
      >
        <DataTableFilter filter={props.filter} />

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
                    <AxiomDataTablePagination
                      table={table}
                      pageOptions={props.pageOptions}
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
