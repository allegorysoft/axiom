import { useState } from 'react';
import { createStore, createStoreHook } from '@axiomframework/react-core';
import {
  SettingsIcon,
  Layers01Icon,
  Key01Icon,
  TimeHalfPassIcon,
  SecurityIcon,
  Chat01Icon,
  FileTextIcon,
  LockPasswordIcon,
  LockKeyholeIcon,
  FaceIdIcon,
  UsersRoundIcon,
  ClockIcon,
  UserRoundIcon,
  MailIcon,
  LanguagesIcon,
} from '@hugeicons/core-free-icons';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Checkbox } from '../ui/checkbox';
import { Field, FieldGroup, FieldLabel, FieldDescription } from '../ui/field';
import { Separator } from '../ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import type { SettingsSection } from './user-settings-dialog';

type Value = string | number | boolean;
type SettingField = {
  key: string;
  label: string;
  type: 'text' | 'email' | 'number' | 'checkbox' | 'select';
  options?: { value: string; label: string }[];
  initial: Value;
  min?: number;
  max?: number;
  required?: boolean;
};
type FormDefinition = { id: string; fields: SettingField[] };

const draftStore = createStore<{
  saved: Record<string, Record<string, Value>>;
}>({ saved: {} });
const useDrafts = createStoreHook(draftStore);

// These values are UI drafts only. Server settings are not read or changed yet.
function SettingsForm({ definition }: { definition: FormDefinition }) {
  const saved = useDrafts((state) => state.saved[definition.id]);
  const defaults = Object.fromEntries(
    definition.fields.map((field) => [field.key, field.initial]),
  );
  const [values, setValues] = useState(saved ?? defaults);
  const [message, setMessage] = useState('');
  const baseline = saved ?? defaults;
  const dirty = definition.fields.some(
    (field) => values[field.key] !== baseline[field.key],
  );
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        draftStore.set((state) => ({
          saved: { ...state.saved, [definition.id]: { ...values } },
        }));
        setMessage(
          'Draft saved for this session. Application settings have not been changed.',
        );
      }}
    >
      <FieldGroup>
        {definition.fields.map((field) =>
          field.type === 'checkbox' ? (
            <Field key={field.key} orientation="horizontal">
              <Checkbox
                id={`${definition.id}-${field.key}`}
                checked={Boolean(values[field.key])}
                onCheckedChange={(checked) => {
                  setValues({ ...values, [field.key]: checked });
                  setMessage('');
                }}
              />
              <FieldLabel htmlFor={`${definition.id}-${field.key}`}>
                {field.label}
              </FieldLabel>
            </Field>
          ) : field.type === 'select' ? (
            <Field key={field.key}>
              <FieldLabel htmlFor={`${definition.id}-${field.key}`}>
                {field.label}
              </FieldLabel>
              <Select
                items={field.options}
                value={String(values[field.key])}
                onValueChange={(value) => {
                  if (value) {
                    setValues({ ...values, [field.key]: value });
                    setMessage('');
                  }
                }}
              >
                <SelectTrigger
                  id={`${definition.id}-${field.key}`}
                  className="w-full"
                >
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {field.options?.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
          ) : (
            <Field key={field.key}>
              <FieldLabel htmlFor={`${definition.id}-${field.key}`}>
                {field.label}
                {field.required ? ' *' : ''}
              </FieldLabel>
              <Input
                id={`${definition.id}-${field.key}`}
                type={field.type}
                className={
                  field.type === 'number'
                    ? '[appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none'
                    : undefined
                }
                value={String(values[field.key] ?? '')}
                min={field.min}
                max={field.max}
                required={field.required}
                onChange={(event) => {
                  setValues({ ...values, [field.key]: event.target.value });
                  setMessage('');
                }}
              />
            </Field>
          ),
        )}
        {definition.id === 'captcha' && (
          <Field>
            <FieldLabel htmlFor="captcha-secret">Secret Key</FieldLabel>
            <Input
              id="captcha-secret"
              type="password"
              autoComplete="new-password"
              disabled
            />
            <FieldDescription>
              The secret key can be configured when account services are
              connected. CAPTCHA verification is not active yet.
            </FieldDescription>
          </Field>
        )}
        {definition.id === 'emailing' && !values.defaultCredentials && (
          <>
            <Field>
              <FieldLabel htmlFor="smtp-username">Username</FieldLabel>
              <Input id="smtp-username" autoComplete="off" disabled />
            </Field>
            <Field>
              <FieldLabel htmlFor="smtp-password">Password</FieldLabel>
              <Input
                id="smtp-password"
                type="password"
                autoComplete="new-password"
                disabled
              />
            </Field>
            <FieldDescription>
              SMTP credentials can be configured once account services are
              connected.
            </FieldDescription>
          </>
        )}
        <Separator />
        <FieldDescription>
          These are local drafts. Saving does not apply settings to the server.
        </FieldDescription>
        <div className="flex flex-wrap gap-2">
          <Button type="submit" disabled={!dirty}>
            Save
          </Button>
          <Button
            type="button"
            variant="outline"
            disabled={!dirty}
            onClick={() => {
              setValues({ ...baseline });
              setMessage('');
            }}
          >
            Cancel
          </Button>
          {definition.id === 'emailing' && (
            <Button type="button" variant="outline" disabled>
              Send Test Email
            </Button>
          )}
        </div>
        {definition.id === 'emailing' && (
          <FieldDescription>
            Test emails will be available when the email service is connected.
          </FieldDescription>
        )}
        {message && (
          <p role="status" className="text-sm text-muted-foreground">
            {message}
          </p>
        )}
      </FieldGroup>
    </form>
  );
}

const toggle = (key: string, label: string, initial = false): SettingField => ({
  key,
  label,
  type: 'checkbox',
  initial,
});

const number = (
  key: string,
  label: string,
  initial: number,
  min = 1,
  max = 10080,
): SettingField => ({
  key,
  label,
  type: 'number',
  initial,
  min,
  max,
  required: true,
});

const text = (
  key: string,
  label: string,
  initial = '',
  required = false,
): SettingField => ({ key, label, type: 'text', initial, required });

function form(id: string, fields: SettingField[]) {
  const definition = { id, fields };
  return function SettingsPage() {
    return <SettingsForm definition={definition} />;
  };
}

export const APPLICATION_SETTINGS_SECTIONS: readonly SettingsSection[] = [
  {
    id: 'general',
    label: 'General',
    group: 'Account & Access',
    description: 'Configure account registration and sign-in preferences.',
    icon: SettingsIcon,
    component: form('general', [
      toggle('registration', 'Allow Self Registration'),
      toggle('localLogin', 'Allow Local Sign In', true),
    ]),
  },
  {
    id: 'captcha',
    label: 'Captcha',
    group: 'Account & Access',
    description: 'Configure CAPTCHA protection for account forms.',
    icon: SecurityIcon,
    component: form('captcha', [
      toggle('enabled', 'Enable CAPTCHA'),
      {
        key: 'provider',
        label: 'Provider',
        type: 'select',
        initial: 'turnstile',
        options: [
          { value: 'turnstile', label: 'Cloudflare Turnstile' },
          { value: 'recaptcha', label: 'Google reCAPTCHA' },
          { value: 'hcaptcha', label: 'hCaptcha' },
        ],
      },
      text('siteKey', 'Site Key'),
      toggle('login', 'Use On Sign In', true),
      toggle('registration', 'Use On Registration', true),
      toggle('passwordReset', 'Use On Password Reset', true),
    ]),
  },
  {
    id: 'idle-session',
    label: 'Idle Session Timeout',
    group: 'Account & Access',
    description: 'Configure when inactive sessions should expire.',
    icon: TimeHalfPassIcon,
    component: form('idle-session', [
      toggle('enabled', 'Enable Idle Session Timeout'),
      number('timeout', 'Idle Timeout (Minutes)', 30),
      toggle('warning', 'Show Warning Before Expiration', true),
    ]),
  },
  {
    id: 'passkey',
    label: 'Passkey',
    group: 'Account & Access',
    description: 'Configure passwordless sign-in with passkeys.',
    icon: Key01Icon,
    component: form('passkey', [
      toggle('enabled', 'Enable Passkey Sign In'),
      toggle('verification', 'Require User Verification', true),
    ]),
  },
  {
    id: 'emailing',
    label: 'Emailing',
    group: 'Account & Access',
    description: 'Configure sender information and outgoing email delivery.',
    icon: MailIcon,
    component: form('emailing', [
      text('senderName', 'Default From Display Name', 'Axiom', true),
      {
        key: 'senderAddress',
        label: 'Default From Address',
        type: 'email',
        initial: '',
        required: true,
      },
      text('host', 'Host'),
      number('port', 'Port', 587, 1, 65535),
      toggle('ssl', 'Enable SSL', true),
      toggle('defaultCredentials', 'Use Default Credentials', true),
    ]),
  },
  {
    id: 'feature-identity',
    label: 'Identity',
    group: 'Feature Management',
    description: 'Control availability of identity features.',
    icon: UserRoundIcon,
    component: form('feature-identity', [
      toggle('enabled', 'Enable Identity Management', true),
      toggle('roles', 'Enable Role Management', true),
    ]),
  },
  {
    id: 'feature-chat',
    label: 'Chat',
    group: 'Feature Management',
    description: 'Control availability of messaging features.',
    icon: Chat01Icon,
    component: form('feature-chat', [
      toggle('enabled', 'Enable Chat'),
      toggle('attachments', 'Allow Attachments'),
    ]),
  },
  {
    id: 'feature-audit',
    label: 'Audit Logging',
    group: 'Feature Management',
    description: 'Control availability of audit logging.',
    icon: FileTextIcon,
    component: form('feature-audit', [
      toggle('enabled', 'Enable Audit Logging', true),
      toggle('anonymous', 'Log Anonymous Requests'),
    ]),
  },
  {
    id: 'feature-language',
    label: 'Language Management',
    group: 'Feature Management',
    description: 'Control availability of language and translation management.',
    icon: LanguagesIcon,
    component: form('feature-language', [
      toggle('enabled', 'Enable Language Management', true),
      toggle('translations', 'Allow Custom Translations'),
    ]),
  },
  {
    id: 'password-policy',
    label: 'Password Policy',
    group: 'Identity Management',
    description: 'Define requirements for account passwords.',
    icon: LockPasswordIcon,
    component: form('password-policy', [
      number('length', 'Minimum Password Length', 12, 1, 128),
      toggle('uppercase', 'Require Uppercase Letters', true),
      toggle('lowercase', 'Require Lowercase Letters', true),
      toggle('digit', 'Require Numbers', true),
      toggle('symbol', 'Require Special Characters'),
    ]),
  },
  {
    id: 'lockout',
    label: 'Lockout',
    group: 'Identity Management',
    description: 'Configure lockout after failed sign-in attempts.',
    icon: LockKeyholeIcon,
    component: form('lockout', [
      toggle('enabled', 'Enable Account Lockout', true),
      number('attempts', 'Maximum Failed Attempts', 5, 1, 100),
      number('duration', 'Lockout Duration (Minutes)', 15),
    ]),
  },
  {
    id: 'verification',
    label: 'Identity Verification',
    group: 'Identity Management',
    description: 'Configure verification requirements for sign-in.',
    icon: FaceIdIcon,
    component: form('verification', [
      toggle('email', 'Require Verified Email'),
      toggle('phone', 'Require Verified Phone Number'),
    ]),
  },
  {
    id: 'user-profile',
    label: 'User Profile',
    group: 'Identity Management',
    description: 'Choose which profile details users may update.',
    icon: UserRoundIcon,
    component: form('user-profile', [
      toggle('name', 'Allow Name Changes', true),
      toggle('email', 'Allow Email Changes', true),
      toggle('phone', 'Allow Phone Number Changes', true),
    ]),
  },
  {
    id: 'sessions',
    label: 'Sessions',
    group: 'Identity Management',
    description: 'Configure session limits and account access.',
    icon: ClockIcon,
    component: form('sessions', [
      number('concurrent', 'Maximum Concurrent Sessions', 5, 1, 100),
      toggle(
        'passwordChange',
        'Revoke Other Sessions After Password Change',
        true,
      ),
    ]),
  },
  {
    id: 'tenants',
    label: 'Tenants',
    group: 'SaaS Management',
    description: 'Configure tenant registration preferences.',
    icon: UsersRoundIcon,
    component: form('tenants', [
      toggle('registration', 'Allow Tenant Self Registration'),
      toggle('approval', 'Require Approval For New Tenants', true),
    ]),
  },
  {
    id: 'editions',
    label: 'Editions',
    group: 'SaaS Management',
    description: 'Configure edition and trial preferences.',
    icon: Layers01Icon,
    component: form('editions', [
      toggle('trials', 'Enable Trial Editions'),
      number('trialDays', 'Trial Duration (Days)', 14, 1, 365),
    ]),
  },
];
