'use client';

import type { Column } from '@tanstack/react-table';

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

import type { TData, TFeat } from '../models';

type Props = {
  columns: Column<TFeat, TData, unknown>[];
  resetColumnVisibility: (defaultState?: boolean | undefined) => void;
};
export function ColumnVisibilitySettings({
  columns,
  resetColumnVisibility,
}: Props) {
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
