import { config } from 'zod';
import { provideInitializers } from '@axiomframework/react-core';
import type { RawIssue, ValidationOptions } from './models';
import { DEFAULT_ERRORS, setFormatResolver } from './errors';

export function configureValidation(options?: ValidationOptions) {
  if (options?.formatResolvers) {
    const resolvers = Object.entries(options.formatResolvers);
    for (const [key, resolver] of resolvers) {
      setFormatResolver(key, resolver);
    }
  }

  provideInitializers({ configure: configureCustomErrors });
}

function configureCustomErrors() {
  const customError = (issue: RawIssue) => {
    const error = DEFAULT_ERRORS[issue.code];

    if (!error) {
      return issue.message;
    }

    let args = error.args?.(issue);

    if (issue.code === 'invalid_format' && args) {
      args = { key: String(args['key']) };
    }

    return { message: JSON.stringify({ key: error.key, args }) };
  };

  config({ customError });
}
