import { type ValidationOptions, configureValidation } from '../validation';

type SharedOptions = {
  validation?: ValidationOptions;
};

export function configureShared(options?: SharedOptions) {
  configureValidation(options?.validation);
}
