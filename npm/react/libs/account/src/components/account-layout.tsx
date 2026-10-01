import { Outlet, useLocation, useNavigate } from 'react-router';
import { useTranslation } from '@axiomframework/react-core';
import {
  Card,
  CardContent,
  Tabs,
  TabsList,
  TabsTrigger,
  TabsContent,
} from '@axiomframework/react-theme/components';

import logo from '../assets/axiom-logo.svg';
import { AccountSlider } from './account-slider';

export function ErrorBoundary() {
  return <p>Failed to load account page!</p>;
}

export function Component() {
  const t = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const location = useLocation();
  const basePath = useResolvedPath('.').pathname;

  const activeTab: Tab = TABS.find((tab) => location.pathname.split('/').pop() === tab) ?? 'login';

  return (
    <main className="flex min-h-svh items-center justify-center bg-background p-4 md:p-8">
      <div className="grid w-full max-w-6xl gap-4 rounded-2xl border p-3 md:p-4 lg:grid-cols-2">
        <Card className="min-w-0 py-0">
          <CardContent className="flex h-full flex-col p-6 sm:p-8">
            <div className="my-auto py-3">
              <header className="mb-4 flex flex-col items-center gap-2 text-center">
                <img
                  src={logo}
                  alt="Axiom"
                  className="size-10 object-contain"
                />
                <h1 className="text-xl font-semibold tracking-tight">
                  {t('AxiomAccount:GetStartedWithAxiom')}
                </h1>
                <p className="text-sm text-muted-foreground">
                  {t('AxiomAccount:SignInOrCreateAccount')}
                </p>
              </header>
              <Tabs
                value={activeTab}
                onValueChange={(value) =>
                  navigate(
                    `${location.pathname.replace(/\/(login|sign-up)$/, '')}/${value}${location.search}`,
                  )
                }
              >
                <TabsList className="mx-auto mb-4 grid w-full max-w-sm grid-cols-2">
                  <TabsTrigger value="login">
                    {t('AxiomAccount:SignIn')}
                  </TabsTrigger>
                  <TabsTrigger value="sign-up">
                    {t('AxiomAccount:SignUp')}
                  </TabsTrigger>
                </TabsList>
                <TabsContent value={activeTab}>
                  <Outlet />
                </TabsContent>
              </Tabs>
            </div>
            <footer className="pt-5 text-center text-xs text-muted-foreground">
              Copyright © 2026 Allegorysoft. All rights reserved.
            </footer>
          </CardContent>
        </Card>
        <AccountSlider />
      </div>
    </main>
  );
}
