import { useLocation, useMatches } from 'react-router';
import { useNavGroups, useTranslation } from '@axiomframework/react-core';

import { findGroupPath } from '../workspace-tab/workspace-tab-utils';

export function AppPageTitle({ title }: { title: string }) {
  const t = useTranslation();
  return <h1 className="text-xl font-semibold tracking-tight">{t(title)}</h1>;
}

export function AppRouteTitle() {
  const { pathname } = useLocation();
  const matches = useMatches();
  const groups = useNavGroups();
  const title = findGroupPath(groups, pathname).at(-1)?.title;
  const segment = pathname.split('/').filter(Boolean).at(-1);
  const handle = matches.at(-1)?.handle as { pageTitleInToolbar?: boolean } | undefined;

  if (handle?.pageTitleInToolbar) return null;

  const fallbackTitle = segment
    ? segment.replace(/[-_]+/g, ' ').replace(/^./, (letter) => letter.toUpperCase())
    : 'AxiomBase:Home';

  return (
    <div className="mb-3">
      <AppPageTitle title={title ?? fallbackTitle} />
    </div>
  );
}
