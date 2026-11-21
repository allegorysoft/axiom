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
  TData extends RowData,
  TFeat extends TableFeatures = TableFeatures,
> = TableOptions<TFeat, TData> & {
  columnResizing?: boolean;
  filterOptions?: FilterOptions;
  paginationOptions?: PaginationOptions;
  load?: (model: DataTableLoad) => void;
};

export type DataTableLoad = {
  sorting: SortingState;
  pagination: PaginationState;
};
