import { useNavigate } from 'react-router';
import { HugeiconsIcon } from '@hugeicons/react';
import { Login } from '@hugeicons/core-free-icons';

import {
  useTranslation,
  getOrCreateAuthProvider,
} from '@axiomframework/react-core';

import { SidebarMenuButton, SidebarMenuItem, useSidebar } from './ui/sidebar';

type Props = { placement?: 'navbar' | 'sidebar' };
export function LoginButton({
  placement,
  children,
  ...props
}: Props & React.ComponentProps<'button'>) {
  const navigate = useNavigate();
  const t = useTranslation();
  const { isMobile, state } = useSidebar();
  const authProvider = getOrCreateAuthProvider().get();

  const isInSidebar = placement === 'sidebar';
  const collapsedSidebar = isInSidebar && !isMobile && state === 'collapsed';

  function logIn() {
    authProvider.redirectToLogin(() => navigate(`/account/login`));
  }

  if (isInSidebar) {
    const item = (
      <SidebarMenuButton aria-label="Login button" onClick={logIn} {...props}>
        <HugeiconsIcon icon={Login} strokeWidth={2} />
        <span className="truncate cursor-pointer font-medium">
          {t('AxiomAccount:Login')}
        </span>
      </SidebarMenuButton>
    );
    return collapsedSidebar ? <SidebarMenuItem>{item}</SidebarMenuItem> : item;
  }

  return (
    <button
      type="button"
      aria-label="Login button"
      className="cursor-pointer rounded-full outline-none transition-transform active:scale-95"
      onClick={logIn}
      {...props}
    >
      <HugeiconsIcon icon={Login} strokeWidth={2} className="size-5" />
    </button>
  );
}
