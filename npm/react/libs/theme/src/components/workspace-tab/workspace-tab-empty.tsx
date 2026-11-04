import { useId } from 'react';

import { HugeiconsIcon } from '@hugeicons/react';
import { Plus } from '@hugeicons/core-free-icons';

import { Kbd, KbdGroup } from '../ui/kbd';
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from '../ui/empty';

export function WorkspaceTabEmpty() {
  const titleId = useId();
  const modifier =
    typeof navigator !== 'undefined' &&
    /Mac|iPhone|iPad/.test(navigator.userAgent)
      ? '⌘'
      : 'Ctrl';

  return (
    <Empty
      className="min-h-[min(36rem,calc(100svh-10rem))] gap-0 px-4 py-12 sm:px-6"
      role="region"
      aria-labelledby={titleId}
    >
      <EmptyHeader className="max-w-sm gap-0">
        <EmptyMedia className="mb-6 size-14 rounded-2xl border bg-muted/40 text-muted-foreground shadow-xs">
          <HugeiconsIcon
            icon={Plus}
            strokeWidth={1.5}
            className="size-6"
            aria-hidden="true"
          />
        </EmptyMedia>
        <EmptyTitle
          id={titleId}
          role="heading"
          aria-level={1}
          className="text-xl font-semibold tracking-tight sm:text-2xl"
        >
          This tab is empty
        </EmptyTitle>
        <EmptyDescription className="mt-3 text-sm leading-6">
          Choose a page from the sidebar to get started, or use search to find
          what you need.
        </EmptyDescription>
      </EmptyHeader>
      <EmptyContent className="mt-10 max-w-sm">
        <div className="w-full border-t pt-6 text-left">
          <h2 className="mb-4 text-xs font-medium uppercase tracking-wider text-muted-foreground">
            Tab shortcuts
          </h2>
          <dl className="space-y-3 text-sm">
            {[
              ['Next tab', '→'],
              ['Previous tab', '←'],
            ].map(([label, key]) => (
              <div
                key={label}
                className="flex items-center justify-between gap-6"
              >
                <dt className="text-muted-foreground">{label}</dt>
                <dd>
                  <KbdGroup aria-label={`Alt + ${key}`}>
                    <Kbd>Alt</Kbd>
                    <Kbd>{key}</Kbd>
                  </KbdGroup>
                </dd>
              </div>
            ))}
            <div className="flex items-center justify-between gap-6">
              <dt className="text-muted-foreground">Search pages</dt>
              <dd>
                <KbdGroup aria-label={`${modifier} + K`}>
                  <Kbd>{modifier}</Kbd>
                  <Kbd>K</Kbd>
                </KbdGroup>
              </dd>
            </div>
            <div className="flex items-center justify-between gap-6">
              <dt className="text-muted-foreground">Close tab</dt>
              <dd className="text-xs text-muted-foreground">Middle-click</dd>
            </div>
          </dl>
        </div>
      </EmptyContent>
    </Empty>
  );
}
