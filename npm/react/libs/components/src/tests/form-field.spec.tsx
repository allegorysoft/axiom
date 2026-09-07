/**
 * @vitest-environment jsdom
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { useEffect, type ReactNode } from 'react';
import { FormProvider, useController, useForm } from 'react-hook-form';

import { FormField } from '../components/form-field';

type FormValues = { username: string };

function TestForm({
  children,
  defaultValues,
}: {
  children: ReactNode;
  defaultValues?: Partial<FormValues>;
}) {
  const form = useForm<FormValues>({ defaultValues });
  return <FormProvider {...form}>{children}</FormProvider>;
}

function TestFormWithError({ children }: { children: ReactNode }) {
  const form = useForm<FormValues>();

  useEffect(() => {
    form.setError('username', {
      type: 'manual',
      message: 'Username is required',
    });
  }, [form]);

  return <FormProvider {...form}>{children}</FormProvider>;
}

describe('FormField', () => {
  it('should render children', () => {
    render(
      <TestForm>
        <FormField<FormValues> name="username">
          <input aria-label="Username" />
        </FormField>
      </TestForm>,
    );

    expect(
      screen.getByRole('textbox', { name: 'Username' }),
    ).toBeInTheDocument();
  });

  it('should render children without a container element when Container is not provided', () => {
    const { container } = render(
      <TestForm>
        <FormField<FormValues> name="username">
          <input aria-label="Username" />
        </FormField>
      </TestForm>,
    );

    expect(container.firstElementChild).toBe(
      screen.getByRole('textbox', { name: 'Username' }),
    );
    expect(container.firstElementChild!.ariaLabel).toBe('Username');
  });

  it('should render validation error', () => {
    render(
      <TestFormWithError>
        <FormField<FormValues> name="username">
          <input aria-label="Username" />
        </FormField>
      </TestFormWithError>,
    );

    expect(screen.getByText('Username is required')).toBeInTheDocument();
  });

  it('should not render an error message when the field is valid', () => {
    const { container } = render(
      <TestForm>
        <FormField<FormValues> name="username">
          <input aria-label="Username" />
        </FormField>
      </TestForm>,
    );

    expect(container.querySelector('p')).not.toBeInTheDocument();
  });

  it('should render content inside a custom container element', () => {
    render(
      <TestForm>
        <FormField<FormValues> name="username" Container="section">
          <input aria-label="Username" />
        </FormField>
      </TestForm>,
    );

    const input = screen.getByRole('textbox', { name: 'Username' });
    expect(input.closest('section')).toBeInTheDocument();
  });

  it('should render the error message inside the custom container', () => {
    const TestFormWithErrorAndContainer = () => {
      const form = useForm<FormValues>();

      useEffect(() => {
        form.setError('username', {
          type: 'manual',
          message: 'Username is required',
        });
      }, [form]);

      return (
        <FormProvider {...form}>
          <FormField<FormValues> name="username" Container="section">
            <input aria-label="Username" />
          </FormField>
        </FormProvider>
      );
    };

    render(<TestFormWithErrorAndContainer />);

    const error = screen.getByText('Username is required');
    expect(error.closest('section')).toBeInTheDocument();
  });

  it('should render children with the field value', () => {
    function TestField() {
      const { field } = useController<FormValues>({
        name: 'username',
      });

      return (
        <FormField<FormValues> name="username">
          <input aria-label="Username" {...field} />
        </FormField>
      );
    }

    render(
      <TestForm defaultValues={{ username: 'john' }}>
        <TestField />
      </TestForm>,
    );

    expect(screen.getByRole('textbox', { name: 'Username' })).toHaveValue(
      'john',
    );
  });
});
