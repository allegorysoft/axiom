import { useNavigate, useSearchParams } from 'react-router';
import { HugeiconsIcon } from '@hugeicons/react';
import { MailIcon } from '@hugeicons/core-free-icons';

import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import {
  getOrCreateAuthProvider,
  useTranslation,
} from '@axiomframework/react-core';
import { FormField } from '@axiomframework/react-components';
import {
  Button,
  Field,
  FieldGroup,
  FieldLabel,
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
  PasswordInput,
} from '@axiomframework/react-theme/components';

import { schema, type LoginParams } from './schema';
import { SocialLogins } from '../social-logins';

export function ErrorBoundary() {
  return <p>Failed to load login!</p>;
}

export function Component() {
  const t = useTranslation();
  const navigate = useNavigate();
  const [params] = useSearchParams();

  const form = useForm<LoginParams>({
    resolver: zodResolver(schema),
    mode: 'onSubmit',
  });

  const { register, handleSubmit } = form;

  const onSubmit = async (input: LoginParams) => {
    const provider = getOrCreateAuthProvider();
    await provider.get().login(input.usernameOrEmail, input.password);
    const returnUrl = params.get('returnUrl') || '/';
    navigate(returnUrl);
  };

  return (
    <FormProvider {...form}>
      <form
        onSubmit={(event) => {
          event.preventDefault();
          handleSubmit(onSubmit);
        }}
      >
        <FieldGroup className="gap-4">
          <FormField<LoginParams> name="usernameOrEmail" Container={Field}>
            <FieldLabel htmlFor="usernameOrEmail">
              {t('AxiomAccount:EmailOrUsername')}
            </FieldLabel>
            <InputGroup>
              <InputGroupAddon>
                <HugeiconsIcon icon={MailIcon} strokeWidth={2} />
              </InputGroupAddon>
              <InputGroupInput
                {...register('usernameOrEmail')}
                id="usernameOrEmail"
                name="usernameOrEmail"
                autoComplete="username"
                placeholder={t('AxiomAccount:EmailOrUsername')}
              />
            </InputGroup>
          </FormField>

          <FormField<LoginParams> name="password" Container={Field} className="gap-1.5">
            <FieldLabel htmlFor="password">
              {t('AxiomAccount:Password')}
            </FieldLabel>
            <PasswordInput
              {...register('password')}
              id="password"
              name="password"
              autoComplete="current-password"
              placeholder={t('AxiomAccount:Password')}
            />
          </FormField>

          <Field>
            <Button type="submit">{t('AxiomAccount:SignIn')}</Button>
          </Field>
          <SocialLogins />
        </FieldGroup>
      </form>
    </FormProvider>
  );
}
