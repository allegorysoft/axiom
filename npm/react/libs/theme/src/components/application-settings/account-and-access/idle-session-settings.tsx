import { form } from '../form-wrapper';
import { number, toggle } from '../utils';

export default function IdleSessionsSettings() {
  return form('idle-session', [
    toggle('enabled', 'Enable Idle Session Timeout'),
    number('timeout', 'Idle Timeout (Minutes)', 30),
    toggle('warning', 'Show Warning Before Expiration', true),
  ]);
}
