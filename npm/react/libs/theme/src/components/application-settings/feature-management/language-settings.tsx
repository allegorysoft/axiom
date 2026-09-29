import { form } from '../form-wrapper';
import { toggle } from '../utils';

export default function LanguageSettings() {
  return form('feature-language', [
    toggle('enabled', 'Enable Language Management', true),
    toggle('translations', 'Allow Custom Translations'),
  ]);
}