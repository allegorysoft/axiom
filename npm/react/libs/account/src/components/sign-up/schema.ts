import { z } from 'zod';
import { DEFAULT_PATTERNS } from '@axiomframework/react-shared';

export const schema = z
  .object({
    name: z.string().trim().min(1),
    surname: z.string().trim().min(1),
    username: z.string().trim().min(3),
    email: z.email().trim(),
    password: z.string().trim().min(7).regex(DEFAULT_PATTERNS.password),
    confirmPassword: z.string().trim().min(7),
  })
  .required()
  .refine((data) => data.password === data.confirmPassword, {
    message: 'AxiomAccount:PasswordsDoNotMatch',
    path: ['confirmPassword'],
  });

export type SignUpParams = z.infer<typeof schema>;
