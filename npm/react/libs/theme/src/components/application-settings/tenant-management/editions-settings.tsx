import { form } from '../form-wrapper';
import { number, toggle } from '../utils';

export default function EditionsSettings() {
  return form('editions', [
    toggle('trials', 'Enable Trial Editions'),
    number('trialDays', 'Trial Duration (Days)', 14, 1, 365),
  ]);
}
