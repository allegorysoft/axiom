import { useEffect, useLayoutEffect, useReducer, useRef } from 'react';
import {
  initialNavPanelState,
  navPanelReducer,
  type Nav,
} from '@axiomframework/react-core';
import { useSidebar } from '../ui/sidebar';

export function useSidebarNavigation() {
  const [state, dispatch] = useReducer(navPanelReducer, initialNavPanelState);
  const { open, isMobile, setOpen } = useSidebar();
  const containerRef = useRef<HTMLDivElement>(null);
  const triggers = useRef<HTMLElement[]>([]);
  const focusAfterBack = useRef<HTMLElement | null>(null);
  const previousDepth = useRef(0);

  useEffect(() => {
    if (!open && !isMobile) dispatch({ type: 'reset' });
  }, [open, isMobile]);

  useLayoutEffect(() => {
    if (state.depth === previousDepth.current) return;
    const target = focusAfterBack.current ?? containerRef.current?.querySelector<HTMLElement>(
      `[data-panel-depth="${state.depth}"] [data-sidebar-back]`,
    );
    target?.focus({ preventScroll: true });
    focusAfterBack.current = null;
    previousDepth.current = state.depth;
  }, [state.depth]);

  function enter(item: Nav, trigger: HTMLButtonElement) {
    if (!isMobile && !open) setOpen(true);
    triggers.current[state.depth] = trigger;
    dispatch({ type: 'enter', item });
  }

  function back() {
    if (!state.depth) return;
    focusAfterBack.current = triggers.current[state.depth - 1] ?? null;
    dispatch({ type: 'back' });
  }

  return { ...state, containerRef, enter, back };
}
