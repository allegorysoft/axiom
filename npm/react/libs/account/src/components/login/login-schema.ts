import { z } from 'zod';

export const schema = z
  .object({
    usernameOrEmail: z.string().trim().min(3).nonoptional(),
    password: z.string().trim().min(5).nonoptional(),
  })
  .required();

export type LoginParams = z.infer<typeof schema>;
