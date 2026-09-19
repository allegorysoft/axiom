import * as React from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { EyeIcon, EyeOffIcon, LockIcon } from '@hugeicons/core-free-icons';
import { InputGroup, InputGroupAddon, InputGroupInput } from './ui/input-group';

export function PasswordInput({
  ...inputProps
}: React.ComponentProps<typeof InputGroupInput>) {
  const [visible, setVisible] = React.useState(false);

  return (
    <InputGroup>
      <InputGroupAddon>
        <HugeiconsIcon icon={LockIcon} strokeWidth={2} />
      </InputGroupAddon>

      <InputGroupInput {...inputProps} type={visible ? 'text' : 'password'} />

      <InputGroupAddon align="inline-end">
        <button
          id="togglePassword"
          type="button"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Hide password' : 'Show password'}
          tabIndex={-1}
        >
          {visible ? <HugeiconsIcon icon={EyeOffIcon} strokeWidth={2} /> : <HugeiconsIcon icon={EyeIcon} strokeWidth={2} />}
        </button>
      </InputGroupAddon>
    </InputGroup>
  );
}
