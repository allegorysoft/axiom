import type { CSSProperties } from 'react';

import { HugeiconsIcon } from '@hugeicons/react';
import { GripVerticalIcon } from '@hugeicons/core-free-icons';

import type {
  Column_ColumnSizing,
  Header,
  ReactTable,
} from '@tanstack/react-table';

import { CSS } from '@dnd-kit/utilities';
import { useSortable } from '@dnd-kit/sortable';

import { TableHead } from '../../ui/table';
import { Button } from '../../ui/button';

import type { TData, TFeat } from '../models';

type DraggableTableHeaderProps = {
  header: Header<TFeat, TData, unknown>;
  table: ReactTable<TFeat, TData>;
};
export function DraggableTableHeader({
  header,
  table,
}: DraggableTableHeaderProps) {
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
    width: (
      header.column as typeof header.column & Column_ColumnSizing
    ).getSize(),
    zIndex: isDragging ? 1 : 0,
  };

  return (
    <TableHead
      ref={setNodeRef}
      className="before:bg-border relative h-10 border-t before:absolute before:inset-y-0 before:left-0 before:w-px first:before:bg-transparent"
      style={style}
    >
      <div className="flex items-center justify-start gap-0.5">
        <Button
          size="icon"
          variant="ghost"
          className="-ml-2 size-7"
          {...attributes}
          {...listeners}
          aria-label="Drag to reorder"
        >
          <HugeiconsIcon
            icon={GripVerticalIcon}
            className="opacity-60"
            aria-hidden="true"
          />
        </Button>
        <span className="grow truncate">
          {header.isPlaceholder ? null : table.FlexRender({ header })}
        </span>
      </div>
    </TableHead>
  );
}
