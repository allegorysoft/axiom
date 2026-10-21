import { z } from 'zod';

export const schema = z
  .object({
    name: z.string().trim().min(1),
    surname: z.string().trim().min(1),
    username: z.string().trim().min(3),
    email: z.email().trim(),
    password: z
      .string()
      .trim()
      .min(7)
      .regex(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).+$/),
    confirmPassword: z.string().trim().min(7),
  })
  .required()
  .refine((data) => data.password === data.confirmPassword, {
    message: 'AxiomAccount:PasswordsDoNotMatch',
    path: ['confirmPassword'],
  });

export type SignUpParams = z.infer<typeof schema>;
