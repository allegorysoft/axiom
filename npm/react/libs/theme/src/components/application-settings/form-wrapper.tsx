import { SettingField } from './models';
import { SettingsForm } from './settings-form';

export function form(id: string, fields: SettingField[]) {
  const definition = { id, fields };
  return <SettingsForm definition={definition} />;
}
