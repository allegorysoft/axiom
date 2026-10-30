import { HugeiconsIcon } from '@hugeicons/react';
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '../ui/breadcrumb';
import { House } from '@hugeicons/core-free-icons';

export function AppBreadcrumb() {
  return (
    <Breadcrumb className="py-3 pb-2 px-3 mb-1 border border-t-0 border-card rounded-lg rounded-t-none">
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbLink href="/">
            <HugeiconsIcon icon={House} strokeWidth={2} className="size-5" />
          </BreadcrumbLink>
        </BreadcrumbItem>
        <BreadcrumbSeparator />
        <BreadcrumbItem>
          <BreadcrumbPage>{'Home'}</BreadcrumbPage>
        </BreadcrumbItem>
      </BreadcrumbList>
    </Breadcrumb>
  );
}
