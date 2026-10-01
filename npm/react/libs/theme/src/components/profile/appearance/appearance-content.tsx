import type { ReactNode } from 'react';

import { Button } from '../../ui/button';
import { Separator } from '../../ui/separator';

import { preferencesStore } from '../../preferences/preferences-store';

export function AppearanceContent({ children }: { children: ReactNode }) {
  return (
    <>
      <div>{children}</div>
      <Separator className="mb-6" />
      <div className="space-y-5">
        <p className="text-sm text-muted-foreground">
          Restore the default color palette, font, layout, interface size and
          corner radius.
        </p>
        <Button onClick={() => preferencesStore.resetPreferences()}>
          Restore Defaults
        </Button>
      </div>
    </>
  );
}
