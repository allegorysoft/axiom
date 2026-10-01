import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function GeneralSettings() {
  return form('general', [
    toggle('registration', 'Allow Self Registration'),
    toggle('localLogin', 'Allow Local Sign In', true),
  ]);
}
