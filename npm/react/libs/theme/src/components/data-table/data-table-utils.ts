import { type RowData, createColumnHelper } from '@tanstack/react-table';

import type { AxiomDataTableFeatures } from './data-table-features';

export function createDataTableColumnHelper<TData extends RowData>() {
  return createColumnHelper<AxiomDataTableFeatures, TData>();
}
