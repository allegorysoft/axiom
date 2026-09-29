import { form } from '../form-wrapper';
import { number, toggle } from '../utils';

export default function SessionsSettings() {
  return form('sessions', [
    number('concurrent', 'Maximum Concurrent Sessions', 5, 1, 100),
    toggle(
      'passwordChange',
      'Revoke Other Sessions After Password Change',
      true,
    ),
  ]);
}
