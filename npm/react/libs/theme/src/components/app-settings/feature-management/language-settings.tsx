import { form } from '../shared/form-wrapper';
import { toggle } from '../shared/utils';

export default function LanguageSettings() {
  return form('feature-language', [
    toggle('enabled', 'Enable Language Management', true),
    toggle('translations', 'Allow Custom Translations'),
  ]);
}