import { form } from '../shared/form-wrapper';
import { number, toggle } from '../shared/utils';

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
