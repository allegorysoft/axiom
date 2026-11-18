import { createColumnHelper } from '@tanstack/react-table';
import type { TData, TFeat } from './models';

export function createDataTableColumnHelper<D extends TData>() {
  return createColumnHelper<TFeat, D>();
}
