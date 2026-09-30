import {
  type ReactNode,
  type RefObject,
  createContext,
  useContext,
  useRef,
  useState,
} from 'react';

export type SettingsDialogContextValue = {
  openDialog: (activeTab: string) => void;
  closeDialog: () => void;
  dialogOpen: boolean;
  activeTab: string | null;
  triggerRef: RefObject<HTMLButtonElement | null>;
};

const SettingsDialogContext = createContext<SettingsDialogContextValue | null>(
  null,
);

export function useSettingsDialog(): SettingsDialogContextValue {
  const ctx = useContext(SettingsDialogContext);
  if (!ctx) {
    throw new Error(
      'useSettingsDialog must be used inside <SettingsDialogProvider>',
    );
  }
  return ctx;
}

export function SettingsDialogProvider({ children }: { children: ReactNode }) {
  const triggerRef = useRef<HTMLButtonElement>(null);
  const [open, setOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<string | null>(null);

  const value: SettingsDialogContextValue = {
    openDialog: (next) => {
      setActiveTab(next);
      setOpen(true);
    },
    closeDialog: () => setOpen(false),
    dialogOpen: open,
    activeTab,
    triggerRef,
  };

  return (
    <SettingsDialogContext.Provider value={value}>
      {children}
    </SettingsDialogContext.Provider>
  );
}
