import { z } from 'zod';

export default z
  .object({
    username: z.string().min(3).nonoptional(),
    password: z.string().min(6).nonoptional(),
  })
  .required();
