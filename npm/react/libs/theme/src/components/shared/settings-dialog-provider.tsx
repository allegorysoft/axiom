import {
  type ReactNode,
  type RefObject,
  createContext,
  useContext,
  useRef,
  useState,
} from 'react';

export type SettingsDialogContextValue = {
  openDialog: (dialogName: string) => void;
  closeDialog: () => void;
  dialogOpen: boolean;
  activeDialog: string | null;
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
  const [activeDialog, setActiveDialog] = useState<string | null>(null);

  const value: SettingsDialogContextValue = {
    openDialog: (next) => {
      setActiveDialog(next);
      setOpen(true);
    },
    closeDialog: () => setOpen(false),
    dialogOpen: open,
    activeDialog,
    triggerRef,
  };

  return (
    <SettingsDialogContext.Provider value={value}>
      {children}
    </SettingsDialogContext.Provider>
  );
}
