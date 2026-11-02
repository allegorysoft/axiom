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
      className="gap-0 flex flex-1"
      onValueChange={handleTabChange}
    >
      {children}

      <div className="flex flex-1 flex-col m-1 mb-2 gap-3 p-2 border rounded-lg">
        <WorkspaceTabLayout />
      </div>
    </Tabs>
  );
}
