import { type RowData, createColumnHelper } from '@tanstack/react-table';
import type { TFeat } from './models';

export function createDataTableColumnHelper<D extends RowData>() {
  return createColumnHelper<TFeat, D>();
}
