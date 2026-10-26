import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function TenantsSettings() {
  return form('tenants', [
    toggle('registration', 'Allow Tenant Self Registration'),
    toggle('approval', 'Require Approval For New Tenants', true),
  ]);
}
