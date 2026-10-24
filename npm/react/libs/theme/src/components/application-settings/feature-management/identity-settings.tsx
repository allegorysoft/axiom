import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function IdentitySettings() {
  return form('feature-identity', [
    toggle('enabled', 'Enable Identity Management', true),
    toggle('roles', 'Enable Role Management', true),
  ]);
}