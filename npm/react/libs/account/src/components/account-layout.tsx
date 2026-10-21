import { NavLink, Outlet, useLocation, useMatch } from 'react-router';

import { useTranslation } from '@axiomframework/react-core';
import {
  Card,
  CardContent,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
  useColorTheme,
  useFont,
  usePreferences,
  useRadius,
  useScale,
} from '@axiomframework/react-theme/components';

import { AccountSlider } from './slider/account-slider';

const logo = new URL('../../assets/axiom-logo.svg', import.meta.url).href;

const TABS = ['login', 'sign-up'] as const;
type Tab = (typeof TABS)[number];

export function ErrorBoundary() {
  return <p>Failed to load account page!</p>;
}

export function Component() {
  const t = useTranslation();
  const { pathname } = useLocation();
  const preferences = usePreferences((state) => state.preferences);

  useColorTheme(preferences);
  useFont(preferences);
  useScale(preferences);
  useRadius(preferences);

  const segment = pathname.split('/').pop();
  const active: Tab = TABS.find((tab) => tab === segment) ?? 'login';

  return (
    <main className="flex min-h-svh items-center justify-center bg-background p-4 md:p-8">
      <div className="grid w-full max-w-6xl gap-4 rounded-2xl border p-3 md:p-4 lg:grid-cols-2">
        <Card className="overflow-hidden border transition-colors">
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

              <Tabs value={active}>
                <TabsList className="mx-auto mb-4 grid w-full max-w-sm grid-cols-2">
                  <TabsTrigger
                    value="login"
                    nativeButton={false}
                    render={<NavLink to="/account/login" />}
                  >
                    {t('AxiomAccount:SignIn')}
                  </TabsTrigger>

                  <TabsTrigger
                    value="sign-up"
                    nativeButton={false}
                    render={<NavLink to="/account/sign-up" />}
                  >
                    {t('AxiomAccount:SignUp')}
                  </TabsTrigger>
                </TabsList>
                <TabsContent value={active}>
                  <Outlet />
                </TabsContent>
              </Tabs>
            </div>

            <footer className="pt-5 text-center text-xs text-muted-foreground">
              {t('AxiomAccount:Copyright')}.
            </footer>
          </CardContent>
        </Card>
        <AccountSlider />
      </div>
    </main>
  );
}
