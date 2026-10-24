import { Suspense, useLayoutEffect, useRef, useState } from 'react';
import { ChevronRightIcon, XIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../ui/button';
import {
  Dialog,
  DialogContent,
  DialogClose,
  DialogDescription,
  DialogTitle,
} from '../ui/dialog';
import { SidebarProvider, SidebarTrigger, useSidebar } from '../ui/sidebar';

import type { SettingsDialogProps } from './models';
import { SettingsSidebar } from './settings-dialog-sidebar';
import { useSettingsDialog } from './settings-dialog-provider';

export function SettingsDialog({
  title,
  description,
  open,
  onOpenChange,
  groups,
}: SettingsDialogProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  const t = useTranslation();

  const { setActiveGroup, setActiveTab } = useSettingsDialog();
  const [query, setQuery] = useState('');

  let visibleGroups = groups;
  const search = query.trim().toLowerCase();
  if (search) {
    visibleGroups = groups.filter((g) =>
      g.children.some((item) =>
        [item.title, g.title].some((label) =>
          label.toLowerCase().includes(search),
        ),
      ),
    );
  }

  useLayoutEffect(() => {
    if (open) {
      setQuery('');
      const firstGroup = groups.at(0);
      if (firstGroup) {
        setActiveGroup(firstGroup);
        setActiveTab(firstGroup.children.at(0));
      }
    }
  }, [open]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        ref={dialogRef}
        initialFocus={dialogRef}
        tabIndex={-1}
        showCloseButton={false}
        className="h-[min(760px,90dvh)] max-w-[calc(100%-1rem)] gap-0 overflow-hidden bg-background p-0 sm:max-w-5xl"
      >
        <DialogTitle className="sr-only">{t(title)}</DialogTitle>
        <DialogDescription className="sr-only">
          {t(description)}
        </DialogDescription>

        <SidebarProvider className="h-full min-h-0 flex-col items-stretch md:flex-row">
          <SettingsSidebar
            query={query}
            onQueryChange={setQuery}
            groups={visibleGroups}
          />

          <SettingsContent title={title} />
        </SidebarProvider>
      </DialogContent>
    </Dialog>
  );
}

function SettingsContent({ title }: Pick<SettingsDialogProps, 'title'>) {
  const { activeTab } = useSettingsDialog();
  const t = useTranslation();

  const tab = activeTab;
  if (!tab) {
    return;
  }

  const Content = tab.component;

  return (
    <div className="flex min-h-0 min-w-0 flex-1 flex-col">
      <DialogContentHeader title={title} />

      <section
        key={tab.title}
        aria-label={tab.title}
        className="min-h-0 flex-1 overflow-y-auto overscroll-contain [scrollbar-color:var(--muted-foreground)_transparent] [scrollbar-width:thin]"
      >
        <div className="mx-auto max-w-3xl px-6 py-8 md:px-10 md:py-10">
          <div className="mb-8">
            <h2 className="text-2xl font-semibold tracking-tight">
              {t(tab.title || '')}
            </h2>
            <p className="text-sm text-muted-foreground">
              {t(tab.title || '')}
            </p>
          </div>

          {Content && (
            <Suspense fallback={<TabFallback />}>
              <Content />
            </Suspense>
          )}
        </div>
      </section>
    </div>
  );
}

function DialogContentHeader({ title }: { title: string }) {
  const t = useTranslation();
  const { isMobile } = useSidebar();
  const { activeGroup, activeTab } = useSettingsDialog();

  return (
    <header className="flex shrink-0 items-center border-b px-3 py-3">
      {isMobile && <SidebarTrigger />}

      <span className="ml-2 text-sm text-muted-foreground">{t(title)}</span>
      <HugeiconsIcon
        icon={ChevronRightIcon}
        strokeWidth={2}
        aria-hidden="true"
        className="mx-3 size-4 shrink-0 text-muted-foreground/50"
      />

      <span className="text-sm text-muted-foreground">
        {t(activeGroup?.title || '')}
      </span>
      <HugeiconsIcon
        icon={ChevronRightIcon}
        strokeWidth={2}
        aria-hidden="true"
        className="mx-3 size-4 shrink-0 text-muted-foreground/50"
      />

      <span className="text-sm font-medium">{t(activeTab?.title || '')}</span>

      <DialogClose
        aria-label="Close settings"
        render={
          <Button variant="ghost" size="icon" className="ml-auto shrink-0" />
        }
      >
        <HugeiconsIcon icon={XIcon} strokeWidth={2} />
      </DialogClose>
    </header>
  );
}

function TabFallback() {
  return (
    <div role="status" aria-live="polite" className="space-y-6 animate-pulse">
      <span className="sr-only">Loading…</span>

      <div className="space-y-3">
        <div className="h-4 w-1/3 rounded-md bg-muted" />
        <div className="h-3 w-2/3 rounded-md bg-muted/70" />
      </div>

      <div className="h-10 w-full rounded-lg border bg-muted/40" />

      <div className="space-y-2">
        <div className="h-3 w-full rounded-md bg-muted/70" />
        <div className="h-3 w-11/12 rounded-md bg-muted/70" />
        <div className="h-3 w-4/5 rounded-md bg-muted/70" />
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <div className="h-20 rounded-lg border bg-muted/40" />
        <div className="h-20 rounded-lg border bg-muted/40" />
      </div>
    </div>
  );
}
