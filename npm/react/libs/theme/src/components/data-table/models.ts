import type { ReactNode, SubmitEventHandler } from 'react';
import type {
  RowData,
  TableFeatures,
  TableOptions,
} from '@tanstack/react-table';
import type { PageOptions } from './pagination/models';

export type TFeat = TableFeatures;
export type TData = RowData;

export type DataTableProps = TableOptions<TFeat, TData> & {
  columnResizing?: boolean;
  filter?: {
    title?: string;
    description?: string;
    content: ReactNode;
    onSubmit: SubmitEventHandler<HTMLFormElement>;
    onClear: () => void;
  };

  pageOptions?: PageOptions;
};
