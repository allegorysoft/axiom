import type {
  RowData,
  TableFeatures,
  TableOptions,
  SortingState,
  PaginationState,
} from '@tanstack/react-table';
import type { PaginationOptions } from './pagination/models';
import { FilterOptions } from './filtering/models';

export type DataTableProps<
  D extends RowData,
  F extends TableFeatures = TableFeatures,
> = TableOptions<F, D> & {
  pageTitle?: string;
  columnResizing?: boolean;
  showColumnDividers?: boolean;
  filterOptions?: FilterOptions;
  paginationOptions?: PaginationOptions;
  load?: (model: DataTableLoad) => void;
};

export type DataTableLoad = {
  sorting: SortingState;
  pagination: PaginationState;
  filters: object;
};
