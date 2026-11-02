import {
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { Tabs } from '../ui/tabs';

import { WorkspaceTabLayout } from '../workspace-tab/workspace-tab-layout';

export function AppTabLayout({ children }: { children?: React.ReactNode }) {
  const tabs = useWorkspaceTabs();
  const activeTab = tabs.find((tab) => tab.active);

  function handleTabChange(url: string) {
    workspaceTabStore.setActive(url);
  }

  return (
    <Tabs
      value={activeTab?.url ?? ''}
      className="gap-0"
      onValueChange={handleTabChange}
    >
      {children}

      <div className="gap-3 p-2 m-1 border rounded-lg">
        <WorkspaceTabLayout />
      </div>
    </Tabs>
  );
}
