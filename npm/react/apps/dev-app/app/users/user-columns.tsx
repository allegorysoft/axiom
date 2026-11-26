import { HugeiconsIcon } from '@hugeicons/react';
import { Delete02Icon, PencilEdit01Icon } from '@hugeicons/core-free-icons';

import {
  createDataTableActionsColumn,
  createDataTableColumnHelper,
  createDataTableSelectColumn,
  DataTableSortingHeader,
} from '@axiomframework/react-theme/components';

import type { AppUser } from './data';

const columnHelper = createDataTableColumnHelper<AppUser>();

export const userColumns = columnHelper.columns([
  createDataTableSelectColumn<AppUser>(),

  columnHelper.accessor('name', {
    id: 'name',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="AxiomIdentity:Name" />
    ),
  }),
  columnHelper.accessor('surname', {
    id: 'surname',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="AxiomIdentity:Surname" />
    ),
  }),
  columnHelper.accessor('email', {
    id: 'email',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="AxiomIdentity:Email" />
    ),
  }),
  columnHelper.accessor('birthDate', {
    id: 'birthDate',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="AxiomIdentity:BirthDate" />
    ),
    cell: ({ getValue }) => new Date(getValue()).toLocaleDateString(),
  }),
  columnHelper.accessor('address', {
    id: 'address',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="AxiomIdentity:Address" />
    ),
  }),

  createDataTableActionsColumn<AppUser>([
    {
      icon: <HugeiconsIcon icon={PencilEdit01Icon} />,
      label: 'AxiomTheme:Edit',
      permission: 'AxiomIdentity.Users.Update',
      onClick: (user) => {},
    },
    {
      icon: <HugeiconsIcon icon={Delete02Icon} />,
      label: 'AxiomTheme:Delete',
      permission: 'AxiomIdentity.Users.Delete',
      onClick: (user) => {},
    },
  ]),
]);
