import { useState, useEffect } from 'react';
import { Search } from 'lucide-react';

import { useNavGroups } from '@axiomframework/react-core';

import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from '../ui/input-group';
import { Kbd } from '../ui/kbd';
import {
  Command,
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '../ui/command';

const shortcutModifier = /Mac|iPhone|iPad/.test(navigator.userAgent)
  ? '⌘'
  : 'Ctrl';

export function SidebarHeaderSearch() {
  const [searchOpen, setSearchOpen] = useState(false);
  const groups = useNavGroups();

  const pages = deepFlatMap(
    groups.flatMap((group) => group.items),
    (item) => item.children,
    (item) => (item.url ? item : undefined),
  ).filter((item): item is { title: string; url: string } =>
    Boolean(item?.url?.length),
  );

  function navigate(url: string) {
    window.location.href = url;
  }

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (event.key.toLowerCase() !== 'k') {
        return;
      }

      if (!(event.metaKey || event.ctrlKey)) {
        return;
      }

      event.preventDefault();
      setSearchOpen((open) => !open);
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);

  return (
    <>
      <InputGroup
        className="group-data-[collapsible=icon]:hidden"
        onClick={() => setSearchOpen(true)}
      >
        <InputGroupAddon>
          <Search />
        </InputGroupAddon>
        <InputGroupInput
          aria-label="Search pages"
          aria-haspopup="dialog"
          aria-expanded={searchOpen}
          placeholder="Search…"
          readOnly
          onClick={() => setSearchOpen(true)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault();
              setSearchOpen(true);
            }
          }}
        />
        <InputGroupAddon align="inline-end">
          <Kbd>{shortcutModifier} K</Kbd>
        </InputGroupAddon>
      </InputGroup>

      <CommandDialog
        open={searchOpen}
        onOpenChange={setSearchOpen}
        className="w-[calc(100vw-1.5rem)] max-w-[calc(100vw-1.5rem)] sm:max-w-lg"
      >
        <Command>
          <div className="relative">
            <CommandInput placeholder="Search pages…" className="pr-14" />
            <Kbd className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2">
              ESC
            </Kbd>
          </div>
          <CommandList className="max-h-[min(60vh,20rem)]">
            <CommandEmpty>No results found.</CommandEmpty>
            <CommandGroup heading="Pages">
              {pages.map((page) => (
                <CommandItem
                  key={page.title}
                  onSelect={() => navigate(page.url)}
                >
                  <Search />
                  {page.title}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </CommandDialog>
    </>
  );
}

function deepFlatMap<T, R>(
  items: T[],
  getChildren: (item: T) => T[] | undefined,
  map: (item: T) => R,
): R[] {
  return items.flatMap((item) => [
    map(item),
    ...deepFlatMap(getChildren(item) ?? [], getChildren, map),
  ]);
}
