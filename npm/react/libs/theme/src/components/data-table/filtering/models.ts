import type { ReactNode, SubmitEventHandler } from 'react';

export type FilterOptions = {
  title?: string;
  description?: string;
  content: ReactNode;
  onSubmit: SubmitEventHandler<HTMLFormElement>;
  onClear: () => void;
};
