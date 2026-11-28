'use client';

import { useState } from 'react';
import {
  AxiomDataTable,
  type DataTableProps,
  axiomDataTableFeatures,
  Field,
  FieldGroup,
  FieldLabel,
  Input,
} from '@axiomframework/react-theme/components';

export function FilteredDataTable<D extends object>({
  pageTitle,
  showColumnDividers,
  className,
  filterTitle,
  filterDescription,
  fields,
  columns,
  data,
}: {
  pageTitle: string;
  showColumnDividers?: boolean;
  className?: string;
  filterTitle: string;
  filterDescription: string;
  fields: readonly (keyof D & string)[];
  columns: DataTableProps<D>['columns'];
  data: readonly D[];
}) {
  const [filters, setFilters] = useState<Record<string, string>>({});
  const filteredData = data.filter((row) =>
    fields.every((field) =>
      String(row[field] ?? '')
        .toLowerCase()
        .includes((filters[field] ?? '').toLowerCase()),
    ),
  );

  const onFilterSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    setFilters(
      Object.fromEntries(
        fields.map((field) => [field, String(formData.get(field) ?? '')]),
      ),
    );
  };

  return (
    <div className={className}>
      <AxiomDataTable<D>
        pageTitle={pageTitle}
        showColumnDividers={showColumnDividers}
        features={axiomDataTableFeatures}
        columns={columns}
        data={filteredData}
        columnResizing={true}
        filterOptions={{
          title: filterTitle,
          description: filterDescription,
          content: fields.map((field) => (
            <FieldGroup key={field}>
              <Field id={field} className="capitalize">
                <FieldLabel htmlFor={field}>{field}</FieldLabel>
                <Input
                  id={field}
                  name={field}
                  placeholder={field}
                  defaultValue={filters[field] ?? ''}
                />
              </Field>
            </FieldGroup>
          )),
          onSubmit: onFilterSubmit,
          onClear: () => setFilters({}),
        }}
        paginationOptions={{ showNavigationButtons: true }}
        initialState={{ pagination: { pageIndex: 0, pageSize: 5 } }}
      />
    </div>
  );
}
