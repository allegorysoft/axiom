import { form } from '../shared/form-wrapper';
import { number, toggle } from '../shared/utils';

export default function PasswordPolicySettings() {
  return form('password-policy', [
    number('length', 'Minimum Password Length', 12, 1, 128),
    toggle('uppercase', 'Require Uppercase Letters', true),
    toggle('lowercase', 'Require Lowercase Letters', true),
    toggle('digit', 'Require Numbers', true),
    toggle('symbol', 'Require Special Characters'),
  ]);
}