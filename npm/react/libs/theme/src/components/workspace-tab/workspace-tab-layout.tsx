import { HugeiconsIcon } from '@hugeicons/react';
import {
  Chart,
  Home,
  SchoolReportCardIcon,
  X,
} from '@hugeicons/core-free-icons';

import {
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { Button } from '../ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../ui/card';
import { Outlet } from 'react-router';

const TABS_LIST_CLASS_NAME =
  'h-10! p-0.5! pb-0! rounded-b-none! bg-card! border-0!';
const TABS_TRIGGER_CLASS_NAME =
  'h-full! rounded-b-none! border-0! hover:bg-card/50 data-active:bg-background!';

export function WorkspaceTabLayout() {
  const tabs = useWorkspaceTabs();

  return (
    <Tabs defaultValue={tabs.at(0)!.url as string} className="w-[400px]">
      <TabsList className={TABS_LIST_CLASS_NAME}>
        {tabs.map((tab) => (
          <TabsTrigger
            key={tab.url}
            value={tab.url}
            nativeButton={false}
            className={TABS_TRIGGER_CLASS_NAME}
            render={<div className="inline-flex items-center gap-3 " />}
          >
            {tab.icon}
            {tab.title}
            {tab.active === true && (
              <Button
                variant="ghost"
                size="icon-xs"
                aria-label={`Close ${tab.title} tab`}
                onClick={() => workspaceTabStore.remove(tab.title)}
              >
                <HugeiconsIcon icon={X} />
              </Button>
            )}
          </TabsTrigger>
        ))}
      </TabsList>

      <Outlet />
    </Tabs>
  );
}

function Overview() {
  return (
    <TabsContent value="overview">
      <Card>
        <CardHeader>
          <CardTitle>Overview</CardTitle>
          <CardDescription>
            View your key metrics and recent project activity. Track progress
            across all your active projects.
          </CardDescription>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground">
          You have 12 active projects and 3 pending tasks.
        </CardContent>
      </Card>
    </TabsContent>
  );
}

function Analytics() {
  return (
    <TabsContent value="analytics">
      <Card>
        <CardHeader>
          <CardTitle>Analytics</CardTitle>
          <CardDescription>
            Track performance and user engagement metrics. Monitor trends and
            identify growth opportunities.
          </CardDescription>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground border-0">
          Page views are up 25% compared to last month.
        </CardContent>
      </Card>
    </TabsContent>
  );
}

function Reports() {
  return (
    <TabsContent value="reports">
      <Card>
        <CardHeader>
          <CardTitle>Reports</CardTitle>
          <CardDescription>
            Generate and download your detailed reports. Export data in multiple
            formats for analysis.
          </CardDescription>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground">
          You have 5 reports ready and available to export.
        </CardContent>
      </Card>
    </TabsContent>
  );
}

export function createTabs() {
  workspaceTabStore.add({
    icon: <HugeiconsIcon icon={Home}></HugeiconsIcon>,
    title: 'Overview',
    active: false,
    url: 'overview',
  });
  workspaceTabStore.add({
    icon: <HugeiconsIcon icon={Chart}></HugeiconsIcon>,
    title: 'Analytics',
    active: false,
    url: 'analytics',
  });
  workspaceTabStore.add({
    icon: <HugeiconsIcon icon={SchoolReportCardIcon}></HugeiconsIcon>,
    title: 'Reports',
    active: false,
    url: 'reports',
  });
}

createTabs();
