import { useEffect, useState } from 'react';
import { House } from 'lucide-react';

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from './ui/breadcrumb';
import { Separator } from './ui/separator';
import { SidebarTrigger } from './ui/sidebar';

import { CurrentUserDropdown } from './current-user-dropdown';
import { Languages } from './languages';

export function Header() {
  const label = useDecodedHash();

  return (
    <header className="flex h-14 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 px-3 md:px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60 border-b">
      <SidebarTrigger className="-ml-1" />
      <Separator
        orientation="vertical"
        className="mr-2 data-[orientation=vertical]:h-4"
      />
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem className="hidden md:block">
            <BreadcrumbLink href="/">
              <House className="size-4" />
            </BreadcrumbLink>
          </BreadcrumbItem>

          {label && (
            <>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem>
                <BreadcrumbPage>{label}</BreadcrumbPage>
              </BreadcrumbItem>
            </>
          )}
        </BreadcrumbList>
      </Breadcrumb>

      <div className="ml-auto flex min-w-0 items-center gap-2">
        <Languages />
        <CurrentUserDropdown />
      </div>
    </header>
  );
}

function decodeHash(hash: string): string {
  const raw = hash.startsWith('#') ? hash.slice(1) : hash;
  if (!raw) return '';

  try {
    return decodeURIComponent(raw);
  } catch {
    return raw;
  }
}

function useDecodedHash(): string {
  const [value, setValue] = useState(() =>
    typeof window === 'undefined' ? '' : decodeHash(window.location.hash),
  );

  useEffect(() => {
    const update = () => setValue(decodeHash(window.location.hash));

    update();
    window.addEventListener('hashchange', update);
    return () => window.removeEventListener('hashchange', update);
  }, []);

  return value;
}
