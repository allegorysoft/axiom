import { useEffect, useRef } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  ChevronDownIcon,
  FileEmptyIcon,
  PinIcon,
  PinOffIcon,
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
import { Kbd, KbdGroup } from '../ui/kbd';
import { Tooltip, TooltipContent, TooltipTrigger } from '../ui/tooltip';
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
  ContextMenuShortcut,
  ContextMenuTrigger,
} from '../ui/context-menu';
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
        <TabsList className="min-w-0 justify-start gap-1 border-none!">
          {tabs.map((tab, index) => (
            <ContextMenu key={tab.url}>
              <TabsTrigger
                value={tab.url}
                onDoubleClick={() =>
                  workspaceTabStore.patch(tab.url, { ephemeral: false })
                }
                onAuxClick={(event) => {
                  if (event.button === 1 && !tab.pinned) {
                    event.preventDefault();
                    workspaceTabStore.remove(tab.url);
                  }
                }}
                nativeButton={false}
                className="group h-auto! max-w-48 flex-none border-none! py-1! pr-1! before:pointer-events-none before:absolute before:top-1/2 before:-left-0.5 before:hidden before:h-4 before:w-px before:-translate-y-1/2 before:bg-border [[data-slot=tabs-trigger]:not([data-active])+&:not([data-active])]:before:block"
                render={<ContextMenuTrigger />}
              >
                {tab.icon}
                <span
                  className={
                    tab.ephemeral
                      ? 'truncate select-none italic'
                      : 'truncate select-none'
                  }
                  title={t(tab.title)}
                >
                  {t(tab.title)}
                </span>

                <Button
                  variant="ghost"
                  size="icon-xs"
                  className={`cursor-pointer rounded-sm hover:bg-current/10 hover:text-inherit dark:hover:bg-current/10 ${!tab.active && !tab.pinned ? 'opacity-0 group-hover:opacity-100 focus-visible:opacity-100' : ''}`}
                  aria-label={`${tab.pinned ? 'Unpin' : 'Close'} ${t(tab.title)} tab`}
                  onClick={(event) => {
                    event.stopPropagation();
                    if (tab.pinned) {
                      workspaceTabStore.patch(tab.url, { pinned: false });
                    } else {
                      workspaceTabStore.remove(tab.url);
                    }
                  }}
                >
                  <HugeiconsIcon icon={tab.pinned ? PinIcon : X} />
                </Button>
              </TabsTrigger>
              <ContextMenuContent>
                <ContextMenuItem
                  onClick={() =>
                    workspaceTabStore.patch(tab.url, { pinned: !tab.pinned })
                  }
                >
                  {tab.pinned ? 'Unpin tab' : 'Pin tab'}
                </ContextMenuItem>
                <ContextMenuSeparator />
                <ContextMenuItem
                  disabled={index === 0}
                  onClick={() => workspaceTabStore.setActive(tabs[index - 1].url)}
                >
                  Previous tab
                  <ContextMenuShortcut>
                    <KbdGroup aria-label="Alt + Left Arrow">
                      <Kbd>Alt</Kbd>
                      <Kbd>←</Kbd>
                    </KbdGroup>
                  </ContextMenuShortcut>
                </ContextMenuItem>
                <ContextMenuItem
                  disabled={index === tabs.length - 1}
                  onClick={() => workspaceTabStore.setActive(tabs[index + 1].url)}
                >
                  Next tab
                  <ContextMenuShortcut>
                    <KbdGroup aria-label="Alt + Right Arrow">
                      <Kbd>Alt</Kbd>
                      <Kbd>→</Kbd>
                    </KbdGroup>
                  </ContextMenuShortcut>
                </ContextMenuItem>
                <ContextMenuSeparator />
                <ContextMenuItem onClick={() => workspaceTabStore.remove(tab.url)}>
                  Close tab
                  {!tab.pinned && <ContextMenuShortcut>Middle-click</ContextMenuShortcut>}
                </ContextMenuItem>
              </ContextMenuContent>
            </ContextMenu>
          ))}
        </TabsList>
      )}
      <Tooltip>
        <TooltipTrigger
          render={
            <Button
              variant="ghost"
              size="icon"
              className="cursor-pointer bg-muted text-foreground hover:bg-[var(--segmented-selected,var(--background))] hover:text-[color:var(--segmented-selected-foreground,var(--foreground))] dark:bg-muted/50 dark:hover:bg-[var(--segmented-selected,color-mix(in_oklab,var(--input)_30%,transparent))]"
              aria-label="New tab"
              onClick={() => addEmptyTab()}
            />
          }
        >
          <HugeiconsIcon icon={Plus} />
        </TooltipTrigger>
        <TooltipContent side="bottom">New tab</TooltipContent>
      </Tooltip>
    </div>
  );
}

function TabPinMenuItem({ tab }: { tab: WorkspaceTab }) {
  return (
    <DropdownMenuItem
      onClick={() => workspaceTabStore.patch(tab.url, { pinned: !tab.pinned })}
    >
      <HugeiconsIcon icon={tab.pinned ? PinOffIcon : PinIcon} />
      {tab.pinned ? 'Unpin tab' : 'Pin tab'}
    </DropdownMenuItem>
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
          onDoubleClick={() => {
            if (activeTab)
              workspaceTabStore.patch(activeTab.url, { ephemeral: false });
          }}
        >
          {activeTab?.icon}
          <span
            className={
              activeTab?.ephemeral
                ? 'truncate select-none italic'
                : 'truncate select-none'
            }
          >
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
              <span
                className={
                  tab.ephemeral
                    ? 'truncate select-none italic'
                    : 'truncate select-none'
                }
              >
                {t(tab.title)}
              </span>
              {tab.pinned && <HugeiconsIcon icon={PinIcon} />}
            </DropdownMenuItem>
          ))}
          {activeTab && <TabPinMenuItem tab={activeTab} />}
        </DropdownMenuContent>
      </DropdownMenu>
      {activeTab && (
        <Button
          variant="ghost"
          size="icon-xs"
          aria-label={`${activeTab.pinned ? 'Unpin' : 'Close'} ${t(activeTab.title)} tab`}
          onClick={() => {
            if (activeTab.pinned) {
              workspaceTabStore.patch(activeTab.url, { pinned: false });
            } else {
              workspaceTabStore.remove(activeTab.url);
            }
          }}
        >
          <HugeiconsIcon icon={activeTab.pinned ? PinIcon : X} />
        </Button>
      )}
    </div>
  );
}

function addEmptyTab(empty = true) {
  workspaceTabStore.add({
    icon: <HugeiconsIcon icon={FileEmptyIcon} strokeWidth={2} />,
    url: `/empty/${crypto.randomUUID()}`,
    title: 'New Tab',
    empty,
    active: true,
  });
}
