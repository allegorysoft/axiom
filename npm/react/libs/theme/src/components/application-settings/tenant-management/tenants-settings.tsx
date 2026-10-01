import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function TenantsSettings() {
  return form('tenants', [
    toggle('registration', 'Allow Tenant Self Registration'),
    toggle('approval', 'Require Approval For New Tenants', true),
  ]);
}
