import { HugeiconsIcon } from '@hugeicons/react';
import { UserIcon, MailIcon } from '@hugeicons/core-free-icons';

import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import { useTranslation } from '@axiomframework/react-core';
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

import { SocialLogins } from '../social-logins';
import { type SignUpParams, schema } from './schema';

export function Component() {
  const t = useTranslation();

  const form = useForm<SignUpParams>({
    resolver: zodResolver(schema),
    mode: 'onSubmit',
  });

  const { register, handleSubmit } = form;

  const onSubmit = async (input: SignUpParams) => {};

  return (
    <FormProvider {...form}>
      <form noValidate onSubmit={handleSubmit(onSubmit)}>
        <FieldGroup className="gap-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField<SignUpParams> name="name" Container={Field}>
              <FieldLabel htmlFor="name">{t('AxiomAccount:Name')}</FieldLabel>
              <InputGroup>
                <InputGroupAddon>
                  <HugeiconsIcon icon={UserIcon} strokeWidth={2} />
                </InputGroupAddon>
                <InputGroupInput
                  {...register('name')}
                  id="name"
                  type="text"
                  autoComplete="given-name"
                  placeholder={t('AxiomAccount:Name')}
                />
              </InputGroup>
            </FormField>

            <FormField<SignUpParams> name="surname" Container={Field}>
              <FieldLabel htmlFor="surname">
                {t('AxiomAccount:Surname')}
              </FieldLabel>
              <InputGroup>
                <InputGroupAddon>
                  <HugeiconsIcon icon={UserIcon} strokeWidth={2} />
                </InputGroupAddon>
                <InputGroupInput
                  {...register('surname')}
                  id="surname"
                  type="text"
                  autoComplete="family-name"
                  placeholder={t('AxiomAccount:Surname')}
                />
              </InputGroup>
            </FormField>
          </div>

          <FormField<SignUpParams> name="username" Container={Field}>
            <FieldLabel htmlFor="username">
              {t('AxiomAccount:Username')}
            </FieldLabel>
            <InputGroup>
              <InputGroupAddon>
                <HugeiconsIcon icon={UserIcon} strokeWidth={2} />
              </InputGroupAddon>
              <InputGroupInput
                {...register('username')}
                id="username"
                type="text"
                autoComplete="username"
                placeholder={t('AxiomAccount:Username')}
              />
            </InputGroup>
          </FormField>

          <FormField<SignUpParams> name="email" Container={Field}>
            <FieldLabel htmlFor="email">
              {t('AxiomAccount:EmailAddress')}
            </FieldLabel>
            <InputGroup>
              <InputGroupAddon>
                <HugeiconsIcon icon={MailIcon} strokeWidth={2} />
              </InputGroupAddon>
              <InputGroupInput
                {...register('email')}
                id="email"
                type="email"
                autoComplete="email"
                placeholder={t('AxiomAccount:EmailAddress')}
              />
            </InputGroup>
          </FormField>

          <FormField<SignUpParams> name="password" Container={Field}>
            <FieldLabel htmlFor="new-password">
              {t('AxiomAccount:Password')} *
            </FieldLabel>
            <PasswordInput
              {...register('password')}
              id="new-password"
              autoComplete="new-password"
              placeholder={t('AxiomAccount:Password')}
            />
          </FormField>

          <FormField<SignUpParams> name="confirmPassword" Container={Field}>
            <FieldLabel htmlFor="confirm-password">
              {t('AxiomAccount:ConfirmPassword')} *
            </FieldLabel>
            <PasswordInput
              {...register('confirmPassword')}
              id="confirm-password"
              autoComplete="new-password"
              placeholder={t('AxiomAccount:ConfirmPassword')}
            />
          </FormField>

          <Field>
            <Button className="h-10" type="submit">
              {t('AxiomAccount:SignUp')}
            </Button>
          </Field>
          <SocialLogins />
        </FieldGroup>
      </form>
    </FormProvider>
  );
}
