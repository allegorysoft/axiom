import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function ChatSettings() {
  return form('feature-chat', [
    toggle('enabled', 'Enable Chat'),
    toggle('attachments', 'Allow Attachments'),
  ]);
}