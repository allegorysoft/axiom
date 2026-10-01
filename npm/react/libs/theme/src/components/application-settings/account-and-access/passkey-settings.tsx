import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function PasskeySettings() {
  return form('passkey', [
    toggle('enabled', 'Enable Passkey Sign In'),
    toggle('verification', 'Require User Verification', true),
  ]);
}
