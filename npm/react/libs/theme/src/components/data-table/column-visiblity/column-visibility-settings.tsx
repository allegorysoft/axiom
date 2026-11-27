'use client';

import type { Column, RowData, TableFeatures } from '@tanstack/react-table';

import { HugeiconsIcon } from '@hugeicons/react';
import {
  FilterHorizontalIcon,
  RefreshCcwIcon,
} from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
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
          <Button variant="outline" className="cursor-pointer">
            <HugeiconsIcon icon={FilterHorizontalIcon} />
            View
          </Button>
        }
      />
      <DropdownMenuContent align="end" className="w-auto min-w-36">
        <DropdownMenuGroup>
          <DropdownMenuLabel>Toggle columns</DropdownMenuLabel>
          <DropdownMenuSeparator />
          {columns.map((column) => (
            <DropdownMenuCheckboxItem
              key={column.id}
              className="capitalize"
              checked={column.getIsVisible()}
              onCheckedChange={(value) => column.toggleVisibility(!!value)}
            >
              {column.id}
            </DropdownMenuCheckboxItem>
          ))}
        </DropdownMenuGroup>

        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => resetColumnVisibility()}>
          <HugeiconsIcon icon={RefreshCcwIcon} /> {t('AxiomTheme:Reset')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
