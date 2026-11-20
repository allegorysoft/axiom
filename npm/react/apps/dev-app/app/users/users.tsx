'use client';

import { useState } from 'react';
import {
  AxiomDataTable,
  axiomDataTableFeatures,
  Field,
  FieldGroup,
  FieldLabel,
  Input,
} from '@axiomframework/react-theme/components';

import { userColumns } from './user-columns';
import { type AppUser, DATA } from './data';

type FilterType = Partial<Record<keyof AppUser, string>>;
const filterFields: (keyof AppUser)[] = [
  'name',
  'surname',
  'email',
  'birthDate',
  'address',
];

export function Component() {
  const [filters, setFilters] = useState<FilterType>({});
  const data = DATA.filter((user) =>
    filterFields.every((field) =>
      String(user[field] ?? '')
        .toLowerCase()
        .includes((filters[field] ?? '').toLowerCase()),
    ),
  );

  const onFilterSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    setFilters(
      Object.fromEntries(
        filterFields.map((field) => [field, String(formData.get(field) ?? '')]),
      ),
    );
  };

  return (
    <div className="mx-auto py-2 space-y-4">
      <AxiomDataTable
        features={axiomDataTableFeatures}
        columns={userColumns}
        data={data}
        filterOptions={{
          title: 'User list',
          description: 'Filter user list entity',
          content: <Filter filters={filters} />,
          onSubmit: onFilterSubmit,
          onClear: () => setFilters({}),
        }}
        paginationOptions={{ showNavigationButtons: true }}
        initialState={{ pagination: { pageIndex: 0, pageSize: 5 } }}
      />
    </div>
  );
}

function Filter({ filters }: { filters: FilterType }) {
  return (
    <>
      {filterFields.map((field) => (
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
      ))}
    </>
  );
}
