import { z } from 'zod';

export const schema = z
  .object({
    usernameOrEmail: z.string().trim().min(3),
    password: z
      .string()
      .trim()
      .min(7)
      .regex(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).+$/),
  })
  .required();

export type LoginParams = z.infer<typeof schema>;
