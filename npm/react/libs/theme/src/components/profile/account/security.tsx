import { useState } from 'react';

import { Button } from '../../ui/button';
import { Input } from '../../ui/input';
import { Field, FieldGroup, FieldLabel, FieldDescription } from '../../ui/field';
import { Separator } from '../../ui/separator';
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
  DialogHeader,
  DialogFooter,
  DialogClose,
} from '../../ui/dialog';

export default function SecuritySection() {
  const [setupOpen, setSetupOpen] = useState(false);
  return (
    <FieldGroup>
      <Field>
        <h3 className="font-medium">Change Password</h3>
        <FieldDescription>
          Password changes will be available when account services are
          connected.
        </FieldDescription>
      </Field>
      <Field>
        <FieldLabel htmlFor="current-password">Current Password</FieldLabel>
        <Input
          id="current-password"
          type="password"
          autoComplete="current-password"
          disabled
        />
      </Field>
      <Field>
        <FieldLabel htmlFor="new-password">New Password</FieldLabel>
        <Input
          id="new-password"
          type="password"
          autoComplete="new-password"
          disabled
        />
      </Field>
      <Field>
        <FieldLabel htmlFor="confirm-password">Confirm New Password</FieldLabel>
        <Input
          id="confirm-password"
          type="password"
          autoComplete="new-password"
          disabled
        />
      </Field>
      <div>
        <Button disabled>Update password</Button>
      </div>
      <Separator />
      <Field>
        <h3 className="font-medium">Two-Factor Authentication</h3>
        <FieldDescription>
          Add another layer of protection with a verification code from an
          authenticator app.
        </FieldDescription>
        <div className="mt-2">
          <Button variant="outline" onClick={() => setSetupOpen(true)}>
            Set Up Two-Factor Authentication
          </Button>
        </div>
      </Field>
      <Dialog open={setupOpen} onOpenChange={setSetupOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Set Up Two-Factor Authentication</DialogTitle>
            <DialogDescription>
              Authenticator setup will be available when account services are
              connected.
            </DialogDescription>
          </DialogHeader>
          <ol className="list-decimal space-y-3 pl-5 text-sm text-muted-foreground">
            <li>Scan your account QR code with an authenticator app.</li>
            <li>Enter the six-digit code to verify your device.</li>
            <li>Save your recovery codes in a safe place.</li>
          </ol>
          <Field>
            <FieldLabel htmlFor="verification-code">
              Verification Code
            </FieldLabel>
            <Input
              id="verification-code"
              placeholder="000000"
              inputMode="numeric"
              autoComplete="one-time-code"
              maxLength={6}
              disabled
            />
          </Field>
          <DialogFooter>
            <DialogClose render={<Button variant="outline" />}>
              Close
            </DialogClose>
            <Button disabled>Enable two-factor authentication</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </FieldGroup>
  );
}
