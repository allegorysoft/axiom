import type { RowData, TableFeatures } from '@tanstack/react-table';
import { cn } from 'cn';
import { HugeiconsIcon } from '@hugeicons/react';
import { ArrowDown, ArrowUp, ArrowUpDownIcon } from '@hugeicons/core-free-icons';

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

    return ArrowUpDownIcon;
  };

  return (
    <div className={cn('flex min-w-0 items-center gap-1', className)}>
      <span className="truncate">{t(title)}</span>
      <Button
        variant="ghost"
        size="icon"
        className="size-7 shrink-0 cursor-pointer"
        aria-label={t(title)}
        onClick={() => column.toggleSorting()}
      >
        <HugeiconsIcon icon={getSortingIcon()} aria-hidden="true" />
      </Button>
    </div>
  );
}
