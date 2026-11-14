import type { CSSProperties } from 'react';
import {
  flexRender,
  type Cell,
  type Column_ColumnSizing,
  type RowData,
  type TableFeatures,
} from '@tanstack/react-table';

import { CSS } from '@dnd-kit/utilities';
import { useSortable } from '@dnd-kit/sortable';
import { TableCell } from '../../ui/table';

export function DraggableCell<T extends TableFeatures, D extends RowData>({
  cell,
}: {
  cell: Cell<T, D, unknown>;
}) {
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
