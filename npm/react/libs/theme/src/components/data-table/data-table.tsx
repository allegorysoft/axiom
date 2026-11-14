'use client';

import { useId, useState } from 'react';
import type { HeaderGroup, ReactTable } from '@tanstack/react-table';
import { useTable } from '@tanstack/react-table';

import {
  closestCenter,
  DndContext,
  KeyboardSensor,
  MouseSensor,
  TouchSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { restrictToHorizontalAxis } from '@dnd-kit/modifiers';
import {
  arrayMove,
  horizontalListSortingStrategy,
  SortableContext,
} from '@dnd-kit/sortable';

import { useTranslation } from '@axiomframework/react-core';

import {
  Table,
  TableBody,
  TableCell,
  TableHeader,
  TableRow,
} from '../ui/table';

import { DraggableTableHeader } from './draggable/draggable-table-header';
import { DraggableCell } from './draggable/draggable-table-cell';
import { AxiomDataTablePaginationFooter } from './pagination/data-table-pagination-footer';

import type { TData, TFeat, DataTableProps, OptionalRow } from './models';

export function AxiomDataTable(props: DataTableProps) {
  const t = useTranslation();
  const [columnOrder, setColumnOrder] = useState<string[]>(
    props.columns.map((column) => column.id as string),
  );

  const table = useTable({
    ...props,
    state: {
      columnOrder,
    },
    onColumnOrderChange: setColumnOrder,
  });

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;

    if (active && over && active.id !== over.id) {
      setColumnOrder((columnOrder) => {
        const oldIndex = columnOrder.indexOf(active.id as string);
        const newIndex = columnOrder.indexOf(over.id as string);

        return arrayMove(columnOrder, oldIndex, newIndex);
      });
    }
  }

  const sensors = useSensors(
    useSensor(MouseSensor, {}),
    useSensor(TouchSensor, {}),
    useSensor(KeyboardSensor, {}),
  );

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
      <DndContext
        id={useId()}
        collisionDetection={closestCenter}
        modifiers={[restrictToHorizontalAxis]}
        onDragEnd={handleDragEnd}
        sensors={sensors}
      >
        <Table>
          <TableHeader>
            {table
              .getHeaderGroups()
              .map((headerGroup) =>
                renderHeaderRow(headerGroup, table, columnOrder),
              )}
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

          <AxiomDataTablePaginationFooter
            table={table}
            dataLength={props.data.length}
            colSpan={
              table.getHeaderGroups().at(-1)?.headers.length ??
              props.columns.length
            }
            showPageButtons={props.showPageButtons}
          />
        </Table>
      </DndContext>
    </div>
  );
}

function renderHeaderRow(
  headerGroup: HeaderGroup<TFeat, TData>,
  table: ReactTable<TFeat, TData>,
  columnOrder: string[],
) {
  const key = headerGroup.id;
  return (
    <TableRow key={key} className="bg-muted/50 [&>th]:border-t-0">
      <SortableContext
        items={columnOrder}
        strategy={horizontalListSortingStrategy}
      >
        {headerGroup.headers.map((header) => (
          <DraggableTableHeader key={header.id} header={header} table={table} />
        ))}
      </SortableContext>
    </TableRow>
  );
}
