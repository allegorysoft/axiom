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

interface DataTableAction<TData> {
  label: string;
  onClick: (data: TData) => void;
  permission?: string;
}

export function createDataTableActionsColumn<
  TFeatures extends TableFeatures,
  TData extends RowData,
>(actions: DataTableAction<TData>[]) {
  return {
    id: 'actions',
    header: 'AxiomTheme:Actions',
    cell: ({ row }: CellContext<TFeatures, TData>) => (
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
              onClick={() => action.onClick(row.original)}
            >
              {action.label}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    ),
  };
}
