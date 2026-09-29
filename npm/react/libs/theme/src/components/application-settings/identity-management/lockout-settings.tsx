import { form } from '../form-wrapper';
import { number, toggle } from '../utils';

export default function LockoutSettings() {
  return form('lockout', [
    toggle('enabled', 'Enable Account Lockout', true),
    number('attempts', 'Maximum Failed Attempts', 5, 1, 100),
    number('duration', 'Lockout Duration (Minutes)', 15),
  ]);
}
