import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';

import { MoonIcon, SunIcon } from 'lucide-react';

import {
  localizerStore,
  useLocalizer,
  useTranslation,
} from '@axiomframework/react-core';
import { oAuthProvider, useOAuth } from '@axiomframework/react-oauth';
import {
  Button,
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@axiomframework/react-theme/components';

export function Header() {
  const navigate = useNavigate();
  const t = useTranslation();
  const auth = useOAuth((s) => s.token);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    setIsAuthenticated(!!auth?.accessToken.length);
  }, [auth?.accessToken]);

  function logIn() {
    oAuthProvider.get().redirectToLogin(() => navigate('/account/login'));
  }

  function logOut() {
    oAuthProvider.get().logout();
  }

  return (
    <header className="flex h-10 px-3 py-6 shrink-0 items-center gap-2 border-b overflow-hidden">
      <div className="flex items-center gap-2">
        {!isAuthenticated && (
          <Button onClick={logIn}>{t('AxiomAccount:SignIn')} </Button>
        )}
        {isAuthenticated && (
          <>
            <span>{t('AxiomIdentity:Welcome', { name: 'Masum' })}</span>
            <Button onClick={logOut}>{t('AxiomAccount:Logout')} </Button>
          </>
        )}
      </div>
      <div className="flex-1"></div>
      <ThemeToggle />
      <LanguageSelect />
    </header>
  );
}

const items = [
  { label: 'Türkçe', value: 'tr' },
  { label: 'English', value: 'en' },
] as const;

function LanguageSelect() {
  const culture = useLocalizer((s) => s.culture);

  function setLanguage(lang: string) {
    const item = items.find((f) => f.value === lang);
    if (!item) {
      return;
    }

    localizerStore.setCulture({ name: item.value, displayName: item.label });
  }

  return (
    <Select
      items={items}
      value={culture.name}
      onValueChange={(val) => val && setLanguage(val)}
    >
      <SelectTrigger className="w-full max-w-44">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          {items.map((item) => (
            <SelectItem key={item.value} value={item.value}>
              {item.label}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}

function ThemeToggle() {
  const [isDark, setIsDark] = useState(() =>
    document.documentElement.classList.contains('dark'),
  );

  useEffect(() => {
    const observer = new MutationObserver(() => {
      setIsDark(document.documentElement.classList.contains('dark'));
    });

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['class'],
    });

    return () => observer.disconnect();
  }, []);

  function toggleTheme() {
    document.documentElement.classList.toggle('dark');
  }

  return (
    <Button
      variant="outline"
      size="icon"
      onClick={toggleTheme}
      aria-label="Toggle theme"
    >
      {isDark ? <SunIcon /> : <MoonIcon />}
    </Button>
  );
}
