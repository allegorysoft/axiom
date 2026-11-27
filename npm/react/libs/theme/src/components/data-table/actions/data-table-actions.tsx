import { ReactNode } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { MoreHorizontalIcon } from '@hugeicons/core-free-icons';

import type { RowData } from '@tanstack/react-table';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../../ui/dropdown-menu';

import { createDataTableColumnHelper } from '../data-table-utils';

type DataTableAction<D extends RowData> = {
  icon?: ReactNode;
  label: string;
  onClick: (data: D) => void;
  permission?: string;
};

export function createDataTableActionsColumn<D extends RowData>(
  actions: DataTableAction<D>[],
) {
  const columnHelper = createDataTableColumnHelper<D>();

  return columnHelper.display({
    id: 'actions',
    maxSize: 50,
    header: DataTableActionsHeader,
    cell: ({ row }) => (
      <DataTableActionsCell actions={actions} data={row.original} />
    ),
  });
}

function DataTableActionsHeader() {
  const t = useTranslation();
  return t('AxiomTheme:Actions');
}

type Props<D extends RowData> = {
  actions: DataTableAction<D>[];
  data: D;
};
function DataTableActionsCell<D extends RowData>({ actions, data }: Props<D>) {
  const t = useTranslation();

  return (
    <div className="flex">
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button
              variant="ghost"
              size="icon-sm"
              className="cursor-pointer"
              aria-label="Row actions"
            >
              <HugeiconsIcon icon={MoreHorizontalIcon} />
            </Button>
          }
        />
        <DropdownMenuContent align="end">
          {actions.map((action) => (
            <DropdownMenuItem
              key={action.label}
              onClick={() => action.onClick(data)}
            >
              {action.icon}
              {t(action.label)}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
