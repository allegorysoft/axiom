import { preferencesStore } from '../../preferences/preferences-store';
import { usePreferences } from '../../preferences/use-preferences';
import { SEGMENTED_OPTIONS } from '../../preferences/options';

import { AppearanceContent } from './appearance-content';
import { SettingRow } from '../setting-row';
import { ChoiceGroup } from './choice-group';

export default function LayoutSection() {
  const preferences = usePreferences((state) => state.preferences);
  return (
    <AppearanceContent>
      <SettingRow
        title="Navigation Bar"
        description="Keep the navigation bar visible or let it scroll with the page."
      >
        <ChoiceGroup
          label="Navigation Bar"
          options={SEGMENTED_OPTIONS.navbar}
          value={preferences.navbarBehavior}
          onChange={(navbarBehavior) =>
            preferencesStore.patchPreferences({ navbarBehavior })
          }
        />
      </SettingRow>
      <SettingRow
        title="Sidebar Layout"
        description="Choose how the application sidebar sits alongside your content."
      >
        <ChoiceGroup
          label="Sidebar Layout"
          options={SEGMENTED_OPTIONS.sidebar}
          value={preferences.sidebarStyle}
          onChange={(sidebarStyle) =>
            preferencesStore.patchPreferences({ sidebarStyle })
          }
        />
      </SettingRow>
      <SettingRow
        title="Corner Radius"
        description="Choose how rounded interface elements appear."
      >
        <ChoiceGroup
          label="Corner Radius"
          options={SEGMENTED_OPTIONS.radius}
          value={preferences.radius}
          onChange={(radius) => preferencesStore.patchPreferences({ radius })}
        />
      </SettingRow>
      <SettingRow
        title="User Menu Position"
        description="Choose where your user menu appears."
      >
        <ChoiceGroup
          label="User Menu Position"
          options={
            [
              { value: 'sidebar', label: 'Sidebar' },
              { value: 'navbar', label: 'Navbar' },
            ] as const
          }
          value={preferences.userMenuPosition ?? 'navbar'}
          onChange={(userMenuPosition) =>
            preferencesStore.patchPreferences({ userMenuPosition })
          }
        />
      </SettingRow>
    </AppearanceContent>
  );
}
