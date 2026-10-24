import { type ReactNode, createContext, useContext, useState } from 'react';
import type { Tab, TabGroup } from '@axiomframework/react-core';

export type SettingsDialogContextValue = {
  activeGroup: TabGroup | undefined;
  setActiveGroup: (group: TabGroup | undefined) => void;

  activeTab: Tab | undefined;
  setActiveTab: (tab: Tab | undefined) => void;
};

const SettingsDialogContext = createContext<SettingsDialogContextValue | null>(
  null,
);

export function useSettingsDialog(): SettingsDialogContextValue {
  const context = useContext(SettingsDialogContext);
  if (!context) {
    throw new Error(
      'useSettingsDialog must be used inside <SettingsDialogProvider>',
    );
  }

  return context;
}

export function SettingsDialogProvider({ children }: { children: ReactNode }) {
  const [activeGroup, setActiveGroup] = useState<TabGroup>();
  const [activeTab, setActiveTab] = useState<Tab>();

  const value: SettingsDialogContextValue = {
    activeGroup,
    setActiveGroup,
    activeTab,
    setActiveTab,
  };

  return (
    <SettingsDialogContext.Provider value={value}>
      {children}
    </SettingsDialogContext.Provider>
  );
}
