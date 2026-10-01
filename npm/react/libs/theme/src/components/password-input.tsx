import * as React from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { ViewIcon, EyeOffIcon, KeyRoundIcon } from '@hugeicons/core-free-icons';
import { InputGroup, InputGroupAddon, InputGroupInput } from './ui/input-group';

type Props = {showLeadingIcon?: boolean;};
export function PasswordInput({
  showLeadingIcon = true,
  ...inputProps
}: React.ComponentProps<typeof InputGroupInput> & Props) {
  const [visible, setVisible] = React.useState(false);

  return (
    <InputGroup className="password-input">
      {showLeadingIcon && (
        <InputGroupAddon>
          <HugeiconsIcon icon={KeyRoundIcon} strokeWidth={2} />
        </InputGroupAddon>
      )}

      <InputGroupInput {...inputProps} type={visible ? 'text' : 'password'} />

      <InputGroupAddon align="inline-end" className="pr-2.5 has-[>button]:mr-0">
        <button
          id="togglePassword"
          type="button"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Hide password' : 'Show password'}
          aria-pressed={visible}
        >
          {visible ? <HugeiconsIcon icon={EyeOffIcon} strokeWidth={2} className="size-4" /> : <HugeiconsIcon icon={ViewIcon} strokeWidth={2} className="size-4" />}
        </button>
      </InputGroupAddon>
    </InputGroup>
  );
}
