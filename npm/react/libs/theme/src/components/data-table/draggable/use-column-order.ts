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

export function useColumnOrder<D extends RowData>(
  columns: DataTableProps<D>['columns'],
) {
  const [columnOrder, setColumnOrder] = useState(
    columns.map((column) => column.id as string),
  );

  const sensors = useSensors(
    useSensor(MouseSensor, {}),
    useSensor(TouchSensor, {}),
    useSensor(KeyboardSensor, {}),
  );

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;

    if (active && over && active.id !== over.id) {
      setColumnOrder((currentOrder) => {
        const oldIndex = currentOrder.indexOf(active.id as string);
        const newIndex = currentOrder.indexOf(over.id as string);

        return arrayMove(currentOrder, oldIndex, newIndex);
      });
    }
  }

  return { columnOrder, setColumnOrder, sensors, handleDragEnd };
}
