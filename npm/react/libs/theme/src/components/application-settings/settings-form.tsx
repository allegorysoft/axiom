import { useState } from 'react';

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

import type { FormDefinition } from './models';
import { useDrafts } from './draft-hooks';
import { draftStore } from './draft-store';

export function SettingsForm({ definition }: { definition: FormDefinition }) {
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
