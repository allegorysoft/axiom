'use client';

import type { Column, RowData, TableFeatures } from '@tanstack/react-table';

import { HugeiconsIcon } from '@hugeicons/react';
import { RefreshCcwIcon, SlidersHorizontal } from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '../../ui/dropdown-menu';
import { Button } from '../../ui/button';

type Props<D extends RowData> = {
  columns: Column<TableFeatures, D, unknown>[];
  resetColumnVisibility: (defaultState?: boolean) => void;
};

export function ColumnVisibilitySettings<D extends RowData>({
  columns,
  resetColumnVisibility,
}: Props<D>) {
  const t = useTranslation();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="outline" className="cursor-pointer items-center">
            <HugeiconsIcon icon={SlidersHorizontal} />
          </Button>
        }
      />
      <DropdownMenuContent align="end" className="w-full">
        {columns.map((column) => (
          <DropdownMenuCheckboxItem
            key={column.id}
            className="capitalize h-8"
            checked={column.getIsVisible()}
            onCheckedChange={(value) => column.toggleVisibility(!!value)}
            onSelect={(e) => e.preventDefault()}
          >
            {column.id}
          </DropdownMenuCheckboxItem>
        ))}

        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => resetColumnVisibility()}>
          <HugeiconsIcon icon={RefreshCcwIcon} /> {t('AxiomTheme:Reset')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
