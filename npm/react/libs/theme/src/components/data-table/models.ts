import type {
  RowData,
  TableFeatures,
  TableOptions,
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
};
