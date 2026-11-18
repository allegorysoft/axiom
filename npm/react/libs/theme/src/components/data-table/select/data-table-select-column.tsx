import type { RowData } from '@tanstack/react-table';

import { Checkbox } from '../../ui/checkbox';
import { createDataTableColumnHelper } from '../data-table-utils';

type Options = {
  id?: string;
  selectAllLabel?: string;
  selectRowLabel?: string;
};

export function createDataTableSelectColumn<D extends RowData>(
  options: Options = {},
) {
  const columnHelper = createDataTableColumnHelper<D>();

  return columnHelper.display({
    id: options.id ?? 'select',
    maxSize: 40,
    header: ({ table }) => (
      <Checkbox
        checked={table.getIsAllPageRowsSelected()}
        indeterminate={
          table.getIsSomePageRowsSelected() && !table.getIsAllPageRowsSelected()
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label={options.selectAllLabel ?? 'Select all'}
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label={options.selectRowLabel ?? 'Select row'}
      />
    ),
    enableSorting: false,
    enableHiding: false,
  });
}
