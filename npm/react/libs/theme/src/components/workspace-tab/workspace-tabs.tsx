import { useEffect, useRef } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  ChevronDownIcon,
  FileEmptyIcon,
  Plus,
  X,
} from '@hugeicons/core-free-icons';

import {
  type WorkspaceTab,
  useTranslation,
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { useSidebar } from '../ui/sidebar';
import { TabsList, TabsTrigger } from '../ui/tabs';
import { Button } from '../ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';

export function WorkspaceTabs() {
  const t = useTranslation();
  const tabs = useWorkspaceTabs();
  const { isMobile } = useSidebar();
  const initialMount = useRef(true);

  useEffect(() => {
    if (tabs.length === 0 && workspaceTabStore.get().tabs.length === 0) {
      addEmptyTab(!initialMount.current);
    }

    initialMount.current = false;
  }, [tabs.length]);

  return (
    <div className="flex min-w-0 flex-1 items-center gap-1">
      {isMobile ? (
        <MobileWorkspaceTabs tabs={tabs} />
      ) : (
        <TabsList className="min-w-0 justify-start border-none!">
          {tabs.map((tab) => (
            <TabsTrigger
              key={tab.url}
              value={tab.url}
              onAuxClick={(event) => {
                if (event.button === 1) {
                  event.preventDefault();
                  workspaceTabStore.remove(tab.url);
                }
              }}
              nativeButton={false}
              className="group max-w-48 flex-none border-none!"
              render={<div className="inline-flex items-center" />}
            >
              {tab.icon}
              <span className="truncate" title={t(tab.title)}>
                {t(tab.title)}
              </span>

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
        </TabsList>
      )}
      <Button
        variant="ghost"
        size="icon"
        aria-label="Add tab"
        onClick={() => addEmptyTab()}
      >
        <HugeiconsIcon icon={Plus} />
      </Button>
    </div>
  );
}

function MobileWorkspaceTabs({ tabs }: { tabs: WorkspaceTab[] }) {
  const t = useTranslation();
  const activeTab = tabs.find((tab) => tab.active) ?? tabs[0];

  return (
    <div className="flex min-w-0 items-center gap-1">
      <DropdownMenu>
        <DropdownMenuTrigger
          render={<Button variant="ghost" className="min-w-0 max-w-40 gap-2" />}
          aria-label="Select workspace tab"
        >
          {activeTab?.icon}
          <span className="truncate">
            {activeTab ? t(activeTab.title) : 'Tabs'}
          </span>
          <HugeiconsIcon icon={ChevronDownIcon} />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start" className="w-56">
          {tabs.map((tab) => (
            <DropdownMenuItem
              key={tab.url}
              onClick={() => workspaceTabStore.setActive(tab.url)}
              className={tab.active ? 'bg-accent' : undefined}
            >
              {tab.icon}
              <span className="truncate">{t(tab.title)}</span>
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
      {activeTab && (
        <Button
          variant="ghost"
          size="icon-xs"
          aria-label={`Close ${t(activeTab.title)} tab`}
          onClick={() => workspaceTabStore.remove(activeTab.url)}
        >
          <HugeiconsIcon icon={X} />
        </Button>
      )}
    </div>
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
