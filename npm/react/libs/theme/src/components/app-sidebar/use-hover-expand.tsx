import { useEffect, useRef } from 'react';
import { useSidebar } from '../ui/sidebar';

const SIDEBAR_COOKIE_NAME = 'sidebar_state';
const HOVER_CLOSE_DELAY_MS = 100;

export function useHoverExpand() {
  const { open, setOpen, isMobile } = useSidebar();
  const hoverOpened = useRef(false);
  const closeTimer = useRef<number | null>(null);

  const cancelClose = () => {
    if (closeTimer.current !== null) {
      window.clearTimeout(closeTimer.current);
      closeTimer.current = null;
    }
  };

  const setOpenWithoutPersisting = (next: boolean) => {
    const snapshot = readSidebarCookie();
    setOpen(next);
    writeSidebarCookie(snapshot);
  };

  const onMouseEnter = () => {
    if (isMobile) {
      return;
    }
    if (open) {
      return;
    }

    cancelClose();
    hoverOpened.current = true;
    setOpenWithoutPersisting(true);
  };

  const onMouseLeave = () => {
    if (isMobile) {
      return;
    }
    if (!hoverOpened.current) {
      return;
    }

    cancelClose();
    closeTimer.current = window.setTimeout(() => {
      closeTimer.current = null;
      hoverOpened.current = false;
      setOpenWithoutPersisting(false);
    }, HOVER_CLOSE_DELAY_MS);
  };

  useEffect(() => cancelClose, []);

  return { onMouseEnter, onMouseLeave };
}

function readSidebarCookie(): string | null {
  const match = document.cookie.match(
    new RegExp(`(?:^|;\\s*)${SIDEBAR_COOKIE_NAME}=([^;]*)`),
  );
  return match ? match[1] : null;
}

function writeSidebarCookie(value: string | null) {
  if (value === null) {
    document.cookie = `${SIDEBAR_COOKIE_NAME}=; path=/; max-age=0`;
    return;
  }
  document.cookie = `${SIDEBAR_COOKIE_NAME}=${value}; path=/`;
}
