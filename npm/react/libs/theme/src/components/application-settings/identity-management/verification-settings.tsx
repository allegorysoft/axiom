import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function VerificationSettings() {
  return form('verification', [
    toggle('email', 'Require Verified Email'),
    toggle('phone', 'Require Verified Phone Number'),
  ]);
}
