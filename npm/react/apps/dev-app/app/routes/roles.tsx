import { DATA } from '../users/data';
import { roleOptions } from '../users/user-columns';
import { UsersTable } from '../users/users';

const usersWithRoles = DATA.map((user, index) => ({
  ...user,
  role: roleOptions[index % roleOptions.length].name,
}));

export function Component() {
  return <UsersTable data={usersWithRoles} showRole />;
}
