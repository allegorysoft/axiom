import { useState } from 'react';
import {
  Outlet,
  useLocation,
  useNavigate,
  useResolvedPath,
} from 'react-router';

import { useTranslation } from '@axiomframework/react-core';
import {
  Card,
  CardContent,
  FieldDescription,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@axiomframework/react-theme/components';

const TABS = ['login', 'sign-up'] as const;
type Tab = (typeof TABS)[number];

export function loader() {
  return null;
}

export function ErrorBoundary() {
  return <div>Failed to load account page!</div>;
}

export function Component() {
  const t = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const basePath = useResolvedPath('.').pathname;

  const activeTab: Tab =
    TABS.find((tab) => location.pathname.split('/').pop() === tab) ?? 'login';

  return (
    <div className="flex min-h-svh flex-col items-center justify-center bg-muted p-2 rounded">
      <div className="flex w-full max-w-sm md:max-w-6xl flex-col">
        <Card className="overflow-hidden border transition-colors">
          <CardContent className="grid lg:grid-cols-2 min-h-[700px] gap-2">
            <Tabs
              value={activeTab}
              onValueChange={(value) => navigate(`${basePath}/${value}`)}
              className="w-full bg-muted rounded-xl py-2 px-2"
            >
              <TabsList className="!h-auto grid grid-cols-2 rounded-md gap-1 border w-full lg:w-sm m-auto">
                {TABS.map((tabValue) => (
                  <TabsTrigger key={tabValue} value={tabValue} className="h-9">
                    {t(
                      `AxiomAccount:${tabValue === 'login' ? 'SignIn' : 'SignUp'}`,
                    )}
                  </TabsTrigger>
                ))}
              </TabsList>

              <TabsContent value={activeTab}>
                <Outlet />
              </TabsContent>

              <FieldDescription className="!mt-auto mb-2 pt-8 !text-xs text-center mx-2">
                Copyright © 2026 Allegorysoft. All rights reserved. &nbsp;
                <a href="#" className="text-blue-400 !no-underline">
                  Terms &amp; Conditions
                </a>
                <span className="mx-1">|</span>
                <a href="#" className="text-blue-400 !no-underline">
                  Privacy Policy
                </a>
              </FieldDescription>
            </Tabs>

            <Hero />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

const slides = [
  {
    title: (
      <>
        Build Modular .NET Applications
        <br />
        Without Rebuilding the Basics
      </>
    ),
    description:
      'Axiom handles dependency injection, modularity, interception, unit of work, file providers, and localization so developers can focus on application logic instead of repetitive infrastructure.',
  },
  {
    title: (
      <>
        Compose Features
        <br />
        Into Modular Applications
      </>
    ),
    description:
      'Build applications from independent modules while keeping infrastructure concerns isolated, reusable, and easy to maintain.',
  },
  {
    title: (
      <>
        Keep Infrastructure
        <br />
        Out of Your Application Logic
      </>
    ),
    description:
      'Use Axiom for the repetitive infrastructure so your application code stays focused on business logic and features.',
  },
];

function Hero() {
  const [active, setActive] = useState(0);
  const slide = slides[active];

  return (
    <div className="relative hidden overflow-hidden rounded-xl bg-[#171717] lg:flex">
      <div className="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,.045)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,.045)_1px,transparent_1px)] bg-[64px_64px]" />

      <div className="relative z-10 flex w-full flex-col items-center justify-center px-12 pt-8 pb-12 text-center">
        <div className="relative mb-16">
          <div className="absolute -inset-20 rounded-full bg-blue-500/10 blur-3xl" />

          <div className="relative flex items-end gap-3">
            <div className="flex size-28 items-center justify-center rounded-2xl bg-gradient-to-br from-pink-300 to-pink-500 shadow-[0_20px_50px_rgba(236,72,153,.25)]">
              <span className="text-5xl">✚</span>
            </div>

            <div className="flex size-36 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-300 to-violet-500 shadow-[0_25px_60px_rgba(139,92,246,.3)]">
              <span className="text-5xl font-bold text-white">&lt;/&gt;</span>
            </div>

            <div className="flex size-28 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-400 to-blue-600 shadow-[0_20px_50px_rgba(59,130,246,.3)]">
              <span className="text-5xl">▱</span>
            </div>
          </div>
        </div>

        <div className="flex h-[150px] w-full max-w-xl items-center justify-center">
          <div
            key={active}
            className="animate-in fade-in slide-in-from-right-2 duration-300"
          >
            <h2 className="text-3xl font-bold tracking-tight text-white">
              {slide.title}
            </h2>

            <p className="mt-5 text-sm leading-6 text-zinc-400">
              {slide.description}
            </p>
          </div>
        </div>
      </div>

      <div className="absolute inset-x-12 bottom-6 z-20 flex gap-3">
        {slides.map((_, index) => (
          <button
            key={index}
            type="button"
            aria-label={`Show slide ${index + 1}`}
            aria-current={active === index}
            onClick={() => setActive(index)}
            className={[
              'h-1.5 flex-1 rounded-full transition-all duration-300',
              active === index
                ? 'bg-blue-500'
                : 'bg-zinc-700 hover:bg-zinc-500',
            ].join(' ')}
          />
        ))}
      </div>
    </div>
  );
}
