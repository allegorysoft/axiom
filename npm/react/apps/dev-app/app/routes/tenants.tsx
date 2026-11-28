import { HugeiconsIcon } from '@hugeicons/react';
import {
  Clock01Icon,
  Delete02Icon,
  Key01Icon,
  PencilEdit02Icon,
} from '@hugeicons/core-free-icons';
import {
  Badge,
  createDataTableActionsColumn,
  createDataTableColumnHelper,
  createDataTableSelectColumn,
  DataTableSortingHeader,
} from '@axiomframework/react-theme/components';

import { FilteredDataTable } from '../filtered-data-table';

type Tenant = {
  id: string;
  name: string;
  edition: string;
  status: 'Active' | 'Inactive';
};

const tenants: Tenant[] = [
  {
    id: 'allegorysoft',
    name: 'Allegorysoft',
    edition: 'Enterprise',
    status: 'Active',
  },
  {
    id: 'microsoft',
    name: 'Microsoft',
    edition: 'Enterprise',
    status: 'Active',
  },
  { id: 'github', name: 'GitHub', edition: 'Free', status: 'Active' },
  { id: 'apple', name: 'Apple', edition: 'Standard', status: 'Active' },
  {
    id: 'harbor-systems',
    name: 'Harbor Systems',
    edition: 'Professional',
    status: 'Inactive',
  },
];

const columnHelper = createDataTableColumnHelper<Tenant>();
const columns = columnHelper.columns([
  createDataTableSelectColumn<Tenant>(),
  columnHelper.accessor('name', {
    id: 'name',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="Tenant name" />
    ),
  }),
  columnHelper.accessor('edition', {
    id: 'edition',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="Edition" />
    ),
  }),
  columnHelper.accessor('status', {
    id: 'status',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="Status" />
    ),
    cell: ({ getValue }) => {
      const status = getValue();
      return (
        <Badge
          variant={status === 'Active' ? 'success' : 'destructive'}
          className={
            status === 'Active'
              ? 'rounded-[6px] bg-success/8 dark:bg-success/12'
              : 'rounded-[6px] bg-destructive/8 dark:bg-destructive/12'
          }
        >
          {status}
        </Badge>
      );
    },
  }),
  createDataTableActionsColumn<Tenant>(
    [
      {
        icon: <HugeiconsIcon icon={PencilEdit02Icon} aria-hidden="true" />,
        label: 'AxiomTheme:Edit',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Key01Icon} aria-hidden="true" />,
        label: 'Manage features',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Clock01Icon} aria-hidden="true" />,
        label: 'Change history',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Delete02Icon} aria-hidden="true" />,
        label: 'AxiomTheme:Delete',
        variant: 'destructive',
        separatorBefore: true,
        onClick: () => {},
      },
    ],
    [],
    { align: 'end' },
  ),
]);

export function Component() {
  return (
    <FilteredDataTable<Tenant>
      data={tenants}
      pageTitle="Tenants"
      className="[&_[data-slot=table]_tr>:first-child:has([role=checkbox])+*]:pl-8"
      showColumnDividers={false}
      columns={columns}
      fields={['name', 'edition', 'status']}
      filterTitle="Tenant list"
      filterDescription="Filter tenants by name, edition, or status"
    />
  );
}
