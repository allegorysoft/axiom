import type {
  ReactTable,
  RowData,
  Table_RowPagination,
  TableFeatures,
} from '@tanstack/react-table';

export interface DataTablePaginationProps<
  T extends TableFeatures,
  D extends RowData,
> {
  table: ReactTable<T, D>;
  showPageButtons?: boolean;
}

export type Pagination<T extends TableFeatures, D extends RowData> = Partial<
  Table_RowPagination<T, D>
>;
