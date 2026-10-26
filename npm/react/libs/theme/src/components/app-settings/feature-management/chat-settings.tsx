import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function ChatSettings() {
  return form('feature-chat', [
    toggle('enabled', 'Enable Chat'),
    toggle('attachments', 'Allow Attachments'),
  ]);
}