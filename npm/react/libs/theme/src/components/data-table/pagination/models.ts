import type { ReactTable, RowData, TableFeatures } from '@tanstack/react-table';

export interface DataTablePaginationProps<D extends RowData> {
  table: ReactTable<TableFeatures, D>;
  paginationOptions?: PaginationOptions;
}

export type PaginationOptions = {
  showNavigationButtons?: boolean;
};
