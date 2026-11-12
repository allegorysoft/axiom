import type {
  Column_RowSorting,
  RowData,
  TableFeatures,
} from '@tanstack/react-table';

import { cn } from 'cn';

import { HugeiconsIcon } from '@hugeicons/react';
import { ArrowDown, ArrowUp, ArrowUpDown } from '@hugeicons/core-free-icons';

import { Button } from '../../ui/button';

import type { DataTableColumnProps } from './data-table-column-props';
import { useTranslation } from '@axiomframework/react-core';

export interface DataTableColumnHeaderProps<
  TFeatures extends TableFeatures,
  TData extends RowData,
  TValue = unknown,
> extends DataTableColumnProps<TFeatures, TData, TValue> {
  column: DataTableColumnProps<TFeatures, TData, TValue>['column'] &
    Pick<
      Column_RowSorting<TFeatures, TData>,
      'getCanSort' | 'getIsSorted' | 'toggleSorting'
    >;
  title: string;
}

export function DataTableColumnHeader<
  TFeatures extends TableFeatures,
  TData extends RowData,
  TValue = unknown,
>({
  column,
  title,
  className,
}: DataTableColumnHeaderProps<TFeatures, TData, TValue>) {
  const t = useTranslation();

  if (!column.getCanSort()) {
    return <div className={cn(className)}>{t(title)}</div>;
  }

  const sorting = column.getIsSorted();

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Button
        variant="ghost"
        size="sm"
        className="-ml-1 h-8"
        onClick={() => column.toggleSorting()}
      >
        <span>{t(title)}</span>
        <HugeiconsIcon
          icon={
            sorting === 'asc'
              ? ArrowUp
              : sorting === 'desc'
                ? ArrowDown
                : ArrowUpDown
          }
        />
      </Button>
    </div>
  );
}
