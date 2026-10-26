import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function VerificationSettings() {
  return form('verification', [
    toggle('email', 'Require Verified Email'),
    toggle('phone', 'Require Verified Phone Number'),
  ]);
}
