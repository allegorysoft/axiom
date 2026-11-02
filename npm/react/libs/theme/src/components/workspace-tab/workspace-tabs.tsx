import { useEffect, useRef } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { FileEmptyIcon, Plus, X } from '@hugeicons/core-free-icons';

import {
  useTranslation,
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { TabsList, TabsTrigger } from '../ui/tabs';
import { Button } from '../ui/button';

const COMMON_CLASS_NAME = 'border-none!';
const TAB_LIST_CLASS_NAME = `${COMMON_CLASS_NAME}`;
const TAB_CLASS_NAME = `${COMMON_CLASS_NAME} `;

export function WorkspaceTabs() {
  const t = useTranslation();
  const tabs = useWorkspaceTabs();
  const initialMount = useRef(true);

  useEffect(() => {
    if (tabs.length === 0 && workspaceTabStore.get().tabs.length === 0) {
      addEmptyTab(!initialMount.current);
    }

    initialMount.current = false;
  }, [tabs.length]);

  return (
    <TabsList className={`${TAB_LIST_CLASS_NAME}`}>
      {tabs.map((tab) => (
        <TabsTrigger
          key={tab.url}
          value={tab.url}
          nativeButton={false}
          className={`${TAB_CLASS_NAME} rounded-lg! group`}
          render={<div className="inline-flex items-center" />}
        >
          {tab.icon}
          {t(tab.title)}

          <Button
            variant="ghost"
            size="icon-xs"
            aria-label={`Close ${tab.title} tab`}
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
        aria-label="Add tab"
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
