import type { ReactTable, Table_RowPagination } from '@tanstack/react-table';
import type { TData, TFeat } from '../models';

export interface DataTablePaginationProps {
  table: ReactTable<TFeat, TData>;
  pageOptions?: PageOptions;
}

export type TanstackPagination = Partial<Table_RowPagination<TFeat, TData>>;
export type PageOptions = {
  showNavigationButtons?: boolean;
};
