'use client';

import { Column, ReactTable, type TableState } from '@tanstack/react-table';

import { HugeiconsIcon } from '@hugeicons/react';
import { Columns3CogIcon, RefreshCcwIcon } from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '../../ui/dropdown-menu';
import { TData, TFeat } from '../models';
import { Button } from '../../ui/button';

type TableType<
  F extends TFeat = TFeat,
  D extends TData = TData,
  S extends TableState<F> = TableState<F>,
> = ReactTable<F, D, S>;

export function ColumnVisibilitySettings({
  columns,
  table,
}: {
  columns: Column<TFeat, TData, unknown>[];
  table: TableType;
}) {
  const t = useTranslation();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="ghost" className=" justify-between cursor-pointer">
            <span className="flex items-center gap-2">
              <HugeiconsIcon icon={Columns3CogIcon} />
            </span>
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
        <DropdownMenuItem
          onClick={() => {
            table.resetColumnVisibility();
          }}
        >
          <HugeiconsIcon icon={RefreshCcwIcon} /> {t('AxiomTheme:Reset')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
