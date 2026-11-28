import { HugeiconsIcon } from '@hugeicons/react';
import {
  Clock01Icon,
  CrownIcon,
  Delete02Icon,
  File01Icon,
  Key01Icon,
  PaintBrush01Icon,
  PencilEdit01Icon,
  PencilEdit02Icon,
  UserMultipleIcon,
  UserIcon,
  ToolsIcon,
} from '@hugeicons/core-free-icons';

import {
  createDataTableActionsColumn,
  createDataTableColumnHelper,
  createDataTableSelectColumn,
  DataTableSortingHeader,
} from '@axiomframework/react-theme/components';

import type { AppUser } from './data';

const columnHelper = createDataTableColumnHelper<AppUser>();

const columns = columnHelper.columns([
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
]);

const actionsColumn = createDataTableActionsColumn<AppUser>([
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
]);

export const userColumns = columnHelper.columns([...columns, actionsColumn]);

export const roleOptions = [
  {
    name: 'Maintainer',
    icon: ToolsIcon,
    color: 'text-amber-600 dark:text-amber-400',
  },
  {
    name: 'Admin',
    icon: UserIcon,
    color: 'text-green-700 dark:text-green-400',
  },
  {
    name: 'Editor',
    icon: PaintBrush01Icon,
    color: 'text-emerald-700 dark:text-emerald-400',
  },
  {
    name: 'Author',
    icon: PencilEdit02Icon,
    color: 'text-indigo-600 dark:text-indigo-400',
  },
  {
    name: 'Subscriber',
    icon: CrownIcon,
    color: 'text-rose-600 dark:text-rose-400',
  },
];

export const roleColumns = columnHelper.columns([
  ...columns.filter(
    (column) => column.id !== 'address' && column.id !== 'birthDate',
  ),
  columnHelper.accessor('role', {
    id: 'role',
    header: ({ column }) => (
      <DataTableSortingHeader column={column} title="Role" />
    ),
    cell: ({ getValue }) => {
      const role = getValue();
      const option = roleOptions.find(({ name }) => name === role);

      return (
        <span className="inline-flex items-center gap-2">
          {option && (
            <HugeiconsIcon
              icon={option.icon}
              size={16}
              className={option.color}
              aria-hidden="true"
            />
          )}
          {role}
        </span>
      );
    },
  }),
  createDataTableActionsColumn<AppUser>(
    [
      {
        icon: <HugeiconsIcon icon={File01Icon} aria-hidden="true" />,
        label: 'Claims',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Key01Icon} aria-hidden="true" />,
        label: 'Permissions',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Clock01Icon} aria-hidden="true" />,
        label: 'Change history',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={UserMultipleIcon} aria-hidden="true" />,
        label: 'Move all users',
        onClick: () => {},
      },
    ],
    [
      {
        icon: <HugeiconsIcon icon={PencilEdit02Icon} aria-hidden="true" />,
        label: 'AxiomTheme:Edit',
        onClick: () => {},
      },
      {
        icon: <HugeiconsIcon icon={Delete02Icon} aria-hidden="true" />,
        label: 'AxiomTheme:Delete',
        variant: 'destructive',
        onClick: () => {},
      },
    ],
  ),
]);
