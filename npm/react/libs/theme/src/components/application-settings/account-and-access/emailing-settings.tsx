import { form } from '../form-wrapper';
import { number, text, toggle } from '../utils';

export default function EmailingSettings() {
  return form('emailing', [
    text('senderName', 'Default From Display Name', 'Axiom', true),
    {
      key: 'senderAddress',
      label: 'Default From Address',
      type: 'email',
      initial: '',
      required: true,
    },
    text('host', 'Host'),
    number('port', 'Port', 587, 1, 65535),
    toggle('ssl', 'Enable SSL', true),
    toggle('defaultCredentials', 'Use Default Credentials', true),
  ]);
}
