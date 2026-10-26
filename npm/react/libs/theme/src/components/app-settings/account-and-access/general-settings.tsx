import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function GeneralSettings() {
  return form('general', [
    toggle('registration', 'Allow Self Registration'),
    toggle('localLogin', 'Allow Local Sign In', true),
  ]);
}
