import type { CSSProperties } from 'react';
import {
  type Cell,
  type Column_ColumnSizing,
  flexRender,
} from '@tanstack/react-table';

import { CSS } from '@dnd-kit/utilities';
import { useSortable } from '@dnd-kit/sortable';

import { TableCell } from '../../ui/table';
import type { TData, TFeat } from '../models';

type Props = { cell: Cell<TFeat, TData, unknown> };

export function DraggableCell({ cell }: Props) {
  const { isDragging, setNodeRef, transform, transition } = useSortable({
    id: cell.column.id,
  });

  const style: CSSProperties = {
    opacity: isDragging ? 0.8 : 1,
    position: 'relative',
    transform: CSS.Translate.toString(transform),
    transition,
    width: (cell.column as typeof cell.column & Column_ColumnSizing).getSize(),
    zIndex: isDragging ? 1 : 0,
  };

  return (
    <TableCell ref={setNodeRef} className="truncate" style={style}>
      {flexRender(cell.column.columnDef.cell, cell.getContext())}
    </TableCell>
  );
}
