import { z } from 'zod';
import { DEFAULT_PATTERNS } from '@axiomframework/react-shared';

export const schema = z
  .object({
    usernameOrEmail: z.string().trim().min(3),
    password: z.string().trim().min(7).regex(DEFAULT_PATTERNS.password),
  })
  .required();

export type LoginParams = z.infer<typeof schema>;
