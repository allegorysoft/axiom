import type { CSSProperties } from 'react';

import { HugeiconsIcon } from '@hugeicons/react';
import { ArrowLeftRightIcon } from '@hugeicons/core-free-icons';

import type {
  Header,
  ReactTable,
  RowData,
  TableFeatures,
} from '@tanstack/react-table';

import { CSS } from '@dnd-kit/utilities';
import { useSortable } from '@dnd-kit/sortable';

import { TableHead } from '../../ui/table';
import { Button } from '../../ui/button';

type Props<D extends RowData> = {
  header: Header<TableFeatures, D, unknown>;
  table: ReactTable<TableFeatures, D>;
  columnResizing?: boolean;
};
export function DraggableTableHeader<D extends RowData>({
  header,
  table,
  columnResizing,
}: Props<D>) {
  const {
    attributes,
    isDragging,
    listeners,
    setNodeRef,
    transform,
    transition,
  } = useSortable({
    id: header.column.id,
  });

  const style: CSSProperties = {
    opacity: isDragging ? 0.8 : 1,
    position: 'relative',
    transform: CSS.Translate.toString(transform),
    transition,
    whiteSpace: 'nowrap',
    width: header.column.id === 'actions' ? '1%' : header.getSize(),
    zIndex: isDragging ? 1 : 0,
  };

  const canResize = columnResizing && header.column.getCanResize();

  return (
    <TableHead
      ref={setNodeRef}
      className="group/head relative h-10 select-none before:pointer-events-none before:absolute before:inset-y-0 before:left-0 before:w-px before:bg-border first:before:hidden"
      colSpan={header.colSpan}
      style={style}
    >
      <div className="flex items-center gap-1">
        <div
          className={
            header.column.id === 'actions'
              ? 'shrink-0'
              : 'min-w-0 flex-1 truncate'
          }
        >
          {header.isPlaceholder ? null : table.FlexRender({ header })}
        </div>
        <Button
          size="icon"
          variant="ghost"
          className={`size-7 shrink-0 cursor-grab active:cursor-grabbing ${isDragging ? '' : 'pointer-fine:pointer-events-none pointer-fine:opacity-0 pointer-fine:group-hover/head:pointer-events-auto pointer-fine:group-hover/head:opacity-100 focus-visible:opacity-100'}`}
          {...attributes}
          {...listeners}
          aria-label="Drag to reorder"
        >
          <HugeiconsIcon icon={ArrowLeftRightIcon} aria-hidden="true" />
        </Button>
      </div>

      {canResize && (
        <div
          {...{
            onDoubleClick: () => header.column.resetSize(),
            onMouseDown: header.getResizeHandler(),
            onTouchStart: header.getResizeHandler(),
            className:
              'group-last/head:hidden absolute top-0 h-full w-4 cursor-col-resize user-select-none touch-none -right-2 z-10',
          }}
        />
      )}
    </TableHead>
  );
}
