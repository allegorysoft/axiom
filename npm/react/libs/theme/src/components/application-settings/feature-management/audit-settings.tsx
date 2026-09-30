import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function AuditSettings() {
  return form('feature-audit', [
    toggle('enabled', 'Enable Audit Logging', true),
    toggle('anonymous', 'Log Anonymous Requests'),
  ]);
}