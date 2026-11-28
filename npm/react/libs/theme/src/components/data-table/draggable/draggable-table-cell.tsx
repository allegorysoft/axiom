import type { CSSProperties } from 'react';
import {
  type Cell,
  type Column_ColumnSizing,
  type RowData,
  type TableFeatures,
  flexRender,
} from '@tanstack/react-table';

import { CSS } from '@dnd-kit/utilities';
import { useSortable } from '@dnd-kit/sortable';

import { TableCell } from '../../ui/table';
import { isFixedOrderColumn, isSelectionColumn } from '../data-table-utils';

type Props<D extends RowData> = { cell: Cell<TableFeatures, D, unknown> };

export function DraggableCell<D extends RowData>({ cell }: Props<D>) {
  const fixed = isFixedOrderColumn(cell.column.columnDef);
  const selection = isSelectionColumn(cell.column.columnDef);
  const { isDragging, setNodeRef, transform, transition } = useSortable({
    id: cell.column.id,
    disabled: { draggable: fixed, droppable: fixed },
  });

  const width = (
    cell.column as typeof cell.column & Column_ColumnSizing
  ).getSize();
  const style: CSSProperties = {
    height: selection ? width : undefined,
    paddingInline: selection ? 0 : undefined,
    opacity: isDragging ? 0.8 : 1,
    position: 'relative',
    transform: CSS.Translate.toString(transform),
    transition,
    zIndex: isDragging ? 1 : 0,
  };

  return (
    <TableCell
      ref={setNodeRef}
      className="truncate before:pointer-events-none before:absolute before:inset-y-0 before:left-0 before:w-px before:bg-border first:before:hidden"
      style={style}
    >
      {flexRender(cell.column.columnDef.cell, cell.getContext())}
    </TableCell>
  );
}
