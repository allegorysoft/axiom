'use client';

import type { OnChangeFn } from '@tanstack/react-table';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  FilterIcon,
  Search,
} from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from '../../ui/popover';
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from '../../ui/input-group';

import { Separator } from '../../ui/separator';
import { FilterOptions } from './models';

type Props = {
  globalFilter?: string;
  setGlobalFilter?: OnChangeFn<string>;
  filterOptions: FilterOptions;
};

export function DataTableFilter({
  globalFilter,
  setGlobalFilter,
  filterOptions,
}: Props) {
  const t = useTranslation();

  return (
    <section className="flex items-center gap-2">
      <InputGroup className="max-w-3xs">
        <InputGroupInput
          placeholder={t('AxiomTheme:Search')}
          value={globalFilter ?? ''}
          onChange={(event) => {
            if (setGlobalFilter) {
              setGlobalFilter(() => event.target.value);
            }
          }}
        />
        <InputGroupAddon>
          <HugeiconsIcon icon={Search} />
        </InputGroupAddon>
      </InputGroup>

      <Popover>
        <PopoverTrigger
          render={
            <Button variant="outline" className="cursor-pointer">
              <HugeiconsIcon icon={FilterIcon} />
              {t('AxiomTheme:Filters')}
            </Button>
          }
        />

        <PopoverContent
          align="end"
          className="max-h-[min(32rem,80vh)] w-[min(35rem,calc(100vw-2rem))] overflow-y-auto"
        >
          {(filterOptions?.title || filterOptions?.description) && (
            <PopoverHeader>
              <PopoverTitle>{filterOptions?.title}</PopoverTitle>
              <PopoverDescription>
                {filterOptions?.description}
              </PopoverDescription>
            </PopoverHeader>
          )}

          <form
            className="grid gap-2 p-1"
            autoComplete="off"
            onSubmit={filterOptions.onSubmit}
          >
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {filterOptions.content}
            </div>

            <Separator />

            <div className="col-span-full flex justify-end gap-1">
              <Button
                type="button"
                variant="outline"
                onClick={(event) => {
                  filterOptions?.onClear();
                  event.currentTarget.form?.reset();
                }}
              >
                Clear
              </Button>
              <Button type="submit">Save</Button>
            </div>
          </form>
        </PopoverContent>
      </Popover>
    </section>
  );
}
