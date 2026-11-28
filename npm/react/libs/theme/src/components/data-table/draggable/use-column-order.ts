'use client';

import { useState } from 'react';

import type { RowData } from '@tanstack/react-table';
import {
  KeyboardSensor,
  MouseSensor,
  TouchSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { arrayMove } from '@dnd-kit/sortable';

import type { DataTableProps } from '../models';
import { isFixedOrderColumn } from '../data-table-utils';

export function useColumnOrder<D extends RowData>(
  columns: DataTableProps<D>['columns'],
) {
  const [columnOrder, setColumnOrder] = useState(
    columns.map((column) => column.id as string),
  );
  const fixedIds = new Set(
    columns.filter(isFixedOrderColumn).map((column) => column.id),
  );

  const sensors = useSensors(
    useSensor(MouseSensor, {}),
    useSensor(TouchSensor, {}),
    useSensor(KeyboardSensor, {}),
  );

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      setColumnOrder((currentOrder) => {
        const sortableIds = currentOrder.filter((id) => !fixedIds.has(id));
        const oldIndex = sortableIds.indexOf(active.id as string);
        const newIndex = sortableIds.indexOf(over.id as string);
        if (oldIndex < 0 || newIndex < 0) return currentOrder;

        const reordered = arrayMove(sortableIds, oldIndex, newIndex);
        let index = 0;
        return currentOrder.map((id) =>
          fixedIds.has(id) ? id : reordered[index++],
        );
      });
    }
  }

  return { columnOrder, setColumnOrder, sensors, handleDragEnd };
}
