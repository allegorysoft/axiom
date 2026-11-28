import { FilteredDataTable } from '../filtered-data-table';
import { roleColumns, userColumns } from './user-columns';
import { type AppUser, DATA } from './data';

const userFields = [
  'name',
  'surname',
  'email',
  'birthDate',
  'address',
] as const;
const roleFields = ['name', 'surname', 'email', 'role'] as const;

export function Component() {
  return <UsersTable data={DATA} />;
}

export function UsersTable({
  data,
  showRole = false,
}: {
  data: readonly AppUser[];
  showRole?: boolean;
}) {
  return (
    <FilteredDataTable<AppUser>
      data={data}
      pageTitle={showRole ? 'Roles' : 'Users'}
      columns={showRole ? roleColumns : userColumns}
      fields={showRole ? roleFields : userFields}
      filterTitle={showRole ? 'Role list' : 'User list'}
      filterDescription={
        showRole ? 'Filter users by role' : 'Filter user list entity'
      }
    />
  );
}
