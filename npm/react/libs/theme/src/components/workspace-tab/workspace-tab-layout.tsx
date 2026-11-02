import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate, useOutlet } from 'react-router';

import {
  type Nav,
  type WorkspaceTab,
  useNavGroups,
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { TabsContent } from '../ui/tabs';
import { findNavPath } from './workspace-tab-utils';

export function WorkspaceTabLayout() {
  const tabs = useWorkspaceTabs();
  const activeTab = tabs.find((tab) => tab.active) ?? tabs[0];

  const groups = useNavGroups();
  const items = groups.flatMap((group) => group.children);

  const outlet = useOutlet();

  if (!tabs.length) {
    return outlet;
  }

  return tabs.map((tab) => (
    <TabsContent key={tab.url} value={tab.url} keepMounted>
      <WorkspaceTabPage tab={tab} active={tab === activeTab} items={items} />
    </TabsContent>
  ));
}

type Props = {
  tab: WorkspaceTab;
  active: boolean;
  items: Nav[];
};
function WorkspaceTabPage({ tab, active, items }: Props) {
  const outlet = useOutlet();
  const location = useLocation();
  const navigate = useNavigate();

  const url = location.pathname + location.search + location.hash;
  const previousLocation = useRef(location);
  const savedUrl = useRef<string | undefined>(undefined);

  const [content, setContent] = useState(active && !tab.empty ? outlet : null);

  useEffect(() => {
    const changed = previousLocation.current !== location;
    previousLocation.current = location;

    if (!active) {
      return;
    }

    const targetUrl = savedUrl.current ?? findNavPath(items, tab.url).at(-1)?.url;
    if (!changed && targetUrl && targetUrl !== url) {
      void navigate(targetUrl);
      return;
    }

    if (tab.empty && !changed) {
      return;
    }

    if (changed || !content) {
      setContent(outlet);
    }

    if (savedUrl.current !== url || tab.empty) {
      savedUrl.current = url;
      const item = findNavPath(items, location.pathname).at(-1);

      workspaceTabStore.patch(tab.url, {
        empty: false,
        title: item?.title ?? location.pathname,
        ...(item?.icon ? { icon: item.icon } : {}),
      });
    }
  }, [active, content, items, location, navigate, outlet, tab, url]);

  return content;
}
