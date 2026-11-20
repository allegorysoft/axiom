import type { ReactTable, RowData, TableFeatures } from '@tanstack/react-table';

export interface DataTablePaginationProps<TData extends RowData> {
  table: ReactTable<TableFeatures, TData>;
  paginationOptions?: PaginationOptions;
}

export type PaginationOptions = {
  showNavigationButtons?: boolean;
};
