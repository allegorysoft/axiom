import type { RowData, TableFeatures } from '@tanstack/react-table';
import { cn } from 'cn';
import { HugeiconsIcon } from '@hugeicons/react';
import { ArrowDown, ArrowUp, ArrowUpDown } from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import type { DataTableSortingHeaderProps } from './models';

export function DataTableSortingHeader<
  T extends TableFeatures,
  D extends RowData,
  V = unknown,
>({ column, title, className }: DataTableSortingHeaderProps<T, D, V>) {
  const t = useTranslation();

  if (!column.getCanSort()) {
    return <div className={cn(className)}>{t(title)}</div>;
  }

  const getSortingIcon = () => {
    const sorting = column.getIsSorted();
    if (sorting === 'asc') {
      return ArrowUp;
    }

    if (sorting === 'desc') {
      return ArrowDown;
    }

    return ArrowUpDown;
  };

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Button
        variant="ghost"
        size="sm"
        className="-ml-1 h-8"
        onClick={() => column.toggleSorting()}
      >
        <span>{t(title)}</span>
        <HugeiconsIcon icon={getSortingIcon()} />
      </Button>
    </div>
  );
}
