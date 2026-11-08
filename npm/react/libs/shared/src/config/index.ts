import type { ValidationOptions } from '../validation/models';
import { configureValidation } from '../validation/config';

type SharedOptions = {
  validation?: ValidationOptions;
};

export function configureShared(options?: SharedOptions) {
  configureValidation(options?.validation);
}
