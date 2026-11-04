import { useEffect } from 'react';

import {
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { Tabs } from '../ui/tabs';

import { WorkspaceTabLayout } from '../workspace-tab/workspace-tab-layout';

export function AppTabLayout({ children }: { children?: React.ReactNode }) {
  const tabs = useWorkspaceTabs();
  const activeTab = tabs.find((tab) => tab.active);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (
        event.defaultPrevented ||
        !event.altKey ||
        event.ctrlKey ||
        event.metaKey ||
        event.shiftKey ||
        (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft')
      ) {
        return;
      }

      const tabs = workspaceTabStore.get().tabs;
      if (tabs.length < 2) return;

      event.preventDefault();
      const index = Math.max(
        0,
        tabs.findIndex((tab) => tab.active),
      );
      const offset = event.key === 'ArrowRight' ? 1 : -1;
      workspaceTabStore.setActive(
        tabs[(index + offset + tabs.length) % tabs.length].url,
      );
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);

  function handleTabChange(url: string) {
    workspaceTabStore.setActive(url);
  }

  return (
    <Tabs
      value={activeTab?.url ?? ''}
      className="gap-0 flex-1"
      onValueChange={handleTabChange}
    >
      {children}

      <div className="flex-1 gap-3 p-2 m-1 border rounded-lg">
        <WorkspaceTabLayout />
      </div>
    </Tabs>
  );
}
