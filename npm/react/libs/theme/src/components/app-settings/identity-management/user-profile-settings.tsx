import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function UserProfileSettings() {
  return form('user-profile', [
    toggle('name', 'Allow Name Changes', true),
    toggle('email', 'Allow Email Changes', true),
    toggle('phone', 'Allow Phone Number Changes', true),
  ]);
}
