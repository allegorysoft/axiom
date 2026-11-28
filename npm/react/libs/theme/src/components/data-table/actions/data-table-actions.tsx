import { Fragment, type ReactNode } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  MoreHorizontalIcon,
  MoreVerticalIcon,
} from '@hugeicons/core-free-icons';

import type { RowData } from '@tanstack/react-table';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '../../ui/dropdown-menu';

import { createDataTableColumnHelper } from '../data-table-utils';

type DataTableAction<D extends RowData> = {
  icon?: ReactNode;
  label: string;
  onClick: (data: D) => void;
  permission?: string;
  variant?: 'destructive';
  separatorBefore?: boolean;
};

export function createDataTableActionsColumn<D extends RowData>(
  actions: DataTableAction<D>[],
  inlineActions: DataTableAction<D>[] = [],
  { align = 'start' }: { align?: 'start' | 'end' } = {},
) {
  const columnHelper = createDataTableColumnHelper<D>();

  return columnHelper.display({
    id: 'actions',
    size: inlineActions.length ? 112 : 80,
    enableResizing: false,
    header: () => <DataTableActionsHeader align={align} />,
    cell: ({ row }) => (
      <DataTableActionsCell
        actions={actions}
        inlineActions={inlineActions}
        align={align}
        data={row.original}
      />
    ),
  });
}

function DataTableActionsHeader({ align }: { align: 'start' | 'end' }) {
  const t = useTranslation();
  return (
    <div className={align === 'end' ? 'text-right' : undefined}>
      {t('AxiomTheme:Actions')}
    </div>
  );
}

type Props<D extends RowData> = {
  actions: DataTableAction<D>[];
  inlineActions: DataTableAction<D>[];
  align: 'start' | 'end';
  data: D;
};
function DataTableActionsCell<D extends RowData>({
  actions,
  inlineActions,
  align,
  data,
}: Props<D>) {
  const t = useTranslation();

  return (
    <div
      className={
        align === 'end'
          ? '-mr-1.5 flex justify-end gap-1'
          : inlineActions.length
            ? '-ml-1.5 flex gap-1'
            : 'flex justify-center gap-1'
      }
    >
      {inlineActions.map((action) => (
        <Button
          key={action.label}
          variant="ghost"
          size="icon-sm"
          className={
            action.variant === 'destructive'
              ? 'shrink-0 cursor-pointer hover:text-destructive focus-visible:text-destructive'
              : 'shrink-0 cursor-pointer'
          }
          aria-label={t(action.label)}
          onClick={() => action.onClick(data)}
        >
          {action.icon}
        </Button>
      ))}
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button
              variant="ghost"
              size="icon-sm"
              className="shrink-0 cursor-pointer"
              aria-label="Row actions"
            >
              <HugeiconsIcon
                icon={
                  inlineActions.length ? MoreVerticalIcon : MoreHorizontalIcon
                }
              />
            </Button>
          }
        />
        <DropdownMenuContent
          align="end"
          className={
            inlineActions.length || align === 'end'
              ? 'flex w-max flex-col gap-1 p-2 whitespace-nowrap'
              : undefined
          }
        >
          {actions.map((action) => (
            <Fragment key={action.label}>
              {action.separatorBefore && (
                <DropdownMenuSeparator className="my-0 shrink-0" />
              )}
              <DropdownMenuItem
                variant={action.variant}
                onClick={() => action.onClick(data)}
              >
                {action.icon}
                {t(action.label)}
              </DropdownMenuItem>
            </Fragment>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
