import type { ReactTable, Table_RowPagination } from '@tanstack/react-table';
import type { TData, TFeat } from '../models';

export interface DataTablePaginationProps {
  table: ReactTable<TFeat, TData>;
  showPageButtons?: boolean;
}

export interface DataTablePaginationFooterProps extends DataTablePaginationProps {
  dataLength: number;
  colSpan: number;
}

export type Pagination = Partial<Table_RowPagination<TFeat, TData>>;
