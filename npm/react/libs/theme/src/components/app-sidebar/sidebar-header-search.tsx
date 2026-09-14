import { useState } from 'react';
import { Search } from 'lucide-react';
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
import { NAV_GROUPS } from './data';

const shortcutModifier = /Mac|iPhone|iPad/.test(navigator.userAgent)
  ? '⌘'
  : 'Ctrl';

const pages = new Set(
  deepFlatMap(
    NAV_GROUPS.flatMap((g) => g.items),
    (item) => item.children,
    (item) => (item.url ? item : undefined),
  ).filter((i) => i?.url?.length),
);

export function SidebarHeaderSearch() {
  const [searchOpen, setSearchOpen] = useState(false);
  function navigate(url: string) {
    window.location.href = url;
  }

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

      <CommandDialog open={searchOpen} onOpenChange={setSearchOpen}>
        <Command>
          <div className="relative">
            <CommandInput placeholder="Search pages…" className="pr-14" />
            <Kbd className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2">
              ESC
            </Kbd>
          </div>
          <CommandList>
            <CommandEmpty>No results found.</CommandEmpty>
            <CommandGroup heading="Pages">
              {[...pages].map((value) => (
                <CommandItem
                  key={value?.title}
                  onSelect={() => navigate(value?.url || '')}
                >
                  <Search />
                  {value?.title}
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
