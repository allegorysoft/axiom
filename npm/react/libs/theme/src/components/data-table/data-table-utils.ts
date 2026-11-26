import {
  type RowData,
  type TableFeatures,
  createColumnHelper,
} from '@tanstack/react-table';

export function createDataTableColumnHelper<D extends RowData>() {
  return createColumnHelper<TableFeatures, D>();
}
