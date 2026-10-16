import { HugeiconsIcon } from '@hugeicons/react';
import { Login } from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import { SidebarMenuButton, SidebarMenuItem, useSidebar } from './ui/sidebar';

type Props = { placement?: 'navbar' | 'sidebar' };
export function LoginButton({
  placement,
  children,
  ...props
}: Props & React.ComponentProps<'button'>) {
  const t = useTranslation();
  const { isMobile, state } = useSidebar();

  const isInSidebar = placement === 'sidebar';
  const collapsedSidebar = isInSidebar && !isMobile && state === 'collapsed';

  if (isInSidebar) {
    const item = (
      <SidebarMenuButton aria-label="Login button" {...props}>
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
      {...props}
    >
      <HugeiconsIcon icon={Login} strokeWidth={2} className="size-5" />
    </button>
  );
}
