import { ReactNode } from 'react';
import type {
  CellContext,
  RowData,
  TableFeatures,
} from '@tanstack/react-table';
import { HugeiconsIcon } from '@hugeicons/react';
import { MoreHorizontalIcon } from '@hugeicons/core-free-icons';
import { Button } from '../../ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../../ui/dropdown-menu';
import { useTranslation } from '@axiomframework/react-core';

interface DataTableAction<TData> {
  icon?: ReactNode;
  label: string;
  onClick: (data: TData) => void;
  permission?: string;
}

export function createDataTableActionsColumn<
  T extends TableFeatures,
  D extends RowData,
>(actions: DataTableAction<D>[]) {
  return {
    id: 'actions',
    header: 'AxiomTheme:Actions',
    cell: ({ row }: CellContext<T, D>) => (
      <DataTableActionsCell actions={actions} data={row.original} />
    ),
  };
}

function DataTableActionsCell<TData extends RowData>({
  actions,
  data,
}: {
  actions: DataTableAction<TData>[];
  data: TData;
}) {
  const t = useTranslation();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="ghost" size="icon-sm" aria-label="Row actions">
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
  );
}
