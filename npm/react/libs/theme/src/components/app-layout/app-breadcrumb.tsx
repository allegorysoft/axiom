import { Fragment } from 'react';
import { useLocation } from 'react-router';

import { HugeiconsIcon } from '@hugeicons/react';

import {
  useNavGroups,
  useTranslation,
  useWorkspaceTabs,
  withGroupName,
} from '@axiomframework/react-core';

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '../ui/breadcrumb';
import { findGroupPath } from '../workspace-tab/workspace-tab-utils';
import { House } from '@hugeicons/core-free-icons';

export function AppBreadcrumb() {
  const t = useTranslation();
  const tabs = useWorkspaceTabs();
  const tab = tabs.find((item) => item.active) ?? tabs[0];
  const groups = useNavGroups();
  const location = useLocation();
  const path = tab?.empty ? [] : findGroupPath(groups, location.pathname);

  return (
    <Breadcrumb className="p-1 mx-1 border rounded-lg">
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbLink href="/">
            <HugeiconsIcon icon={House} strokeWidth={2} className="size-5" />
          </BreadcrumbLink>
        </BreadcrumbItem>
        <BreadcrumbSeparator />
        {path.length ? (
          path.map((item, index) => (
            <Fragment key={index}>
              {path.length > 3 && index === 2 && (
                <>
                  <BreadcrumbSeparator className="md:hidden" />
                  <BreadcrumbItem className="md:hidden">
                    <span aria-label="More breadcrumb items">...</span>
                  </BreadcrumbItem>
                </>
              )}
              {index > 0 && (
                <BreadcrumbSeparator
                  className={
                    path.length > 3 && index > 1 && index < path.length - 1
                      ? 'hidden md:block'
                      : undefined
                  }
                />
              )}
              <BreadcrumbItem
                className={
                  path.length > 3 && index > 1 && index < path.length - 1
                    ? 'hidden md:inline-flex'
                    : undefined
                }
              >
                {index < path.length - 1 && item.url ? (
                  <BreadcrumbLink href={withGroupName(item.url)}>
                    {t(item.title)}
                  </BreadcrumbLink>
                ) : index === path.length - 1 ? (
                  <BreadcrumbPage>{t(item.title)}</BreadcrumbPage>
                ) : (
                  <span>{t(item.title)}</span>
                )}
              </BreadcrumbItem>
            </Fragment>
          ))
        ) : (
          <BreadcrumbItem>
            <BreadcrumbPage>{tab?.title ?? 'Home'}</BreadcrumbPage>
          </BreadcrumbItem>
        )}
      </BreadcrumbList>
    </Breadcrumb>
  );
}
