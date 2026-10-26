import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function AuditSettings() {
  return form('feature-audit', [
    toggle('enabled', 'Enable Audit Logging', true),
    toggle('anonymous', 'Log Anonymous Requests'),
  ]);
}
