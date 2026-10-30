import { useEffect, useRef } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { FileEmptyIcon, Plus, X } from '@hugeicons/core-free-icons';

import {
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { TabsList, TabsTrigger } from '../ui/tabs';
import { Button } from '../ui/button';

const TABS_LIST_CLASS_NAME =
  'h-full! rounded-b-none! border-0! bg-card! p-0.5! pb-0! gap-1';
const TABS_TRIGGER_CLASS_NAME =
  'h-full! rounded-b-none! border-0! hover:bg-background/10 data-active:bg-background!';

export function AppTabsList() {
  const tabs = useWorkspaceTabs();
  const initialMount = useRef(true);

  useEffect(() => {
    if (tabs.length === 0 && workspaceTabStore.get().tabs.length === 0) {
      addEmptyTab(!initialMount.current);
    }

    initialMount.current = false;
  }, [tabs.length]);

  return (
    <TabsList
      className={`${TABS_LIST_CLASS_NAME} max-w-60 md:max-w-full justify-center-safe overflow-hidden overflow-x-auto`}
    >
      {tabs.map((tab) => (
        <TabsTrigger
          key={tab.url}
          value={tab.url}
          nativeButton={false}
          className={`${TABS_TRIGGER_CLASS_NAME} group`}
          render={<div className="inline-flex items-center" />}
        >
          {tab.icon}
          {tab.title}

          <Button
            variant="ghost"
            size="icon-xs"
            aria-label={`Close ${tab.title} tab`}
            className="opacity-0 group-hover:opacity-100"
            onClick={(event) => {
              event.stopPropagation();
              workspaceTabStore.remove(tab.url);
            }}
          >
            <HugeiconsIcon icon={X} />
          </Button>
        </TabsTrigger>
      ))}
      <Button
        variant="ghost"
        size="icon"
        aria-label={`Add tab`}
        className={TABS_TRIGGER_CLASS_NAME}
        onClick={() => addEmptyTab()}
      >
        <HugeiconsIcon icon={Plus} />
      </Button>
    </TabsList>
  );
}

function addEmptyTab(empty = true) {
  workspaceTabStore.add({
    icon: <HugeiconsIcon icon={FileEmptyIcon} strokeWidth={2} />,
    url: `/empty/${crypto.randomUUID()}`,
    title: 'Empty Tab',
    empty,
    active: true,
  });
}
