import type {
  RawIssue,
  InvalidStringFormat,
  AxiomInvalidFormatResolver,
} from './models';

type ValidationError = {
  key: string;
  args?: (issue: RawIssue) => { [key: string]: unknown } | null;
};

export const DEFAULT_ERRORS: Record<string, ValidationError> = {
  too_small: {
    key: 'AxiomBase:Min',
    args: (issue) => {
      //TODO: check issue.origin return per type
      return { length: issue.minimum };
    },
  },

  invalid_format: {
    key: 'AxiomBase:InvalidFormat',
    args: (issue) => {
      let value: ReturnType<AxiomInvalidFormatResolver> = null;

      for (const resolver of Object.values(invalidFormatResolvers)) {
        value = resolver(<InvalidStringFormat>issue);
      }

      return value;
    },
  },
};

const invalidFormatResolvers: Record<string, AxiomInvalidFormatResolver> = {
  password: (issue) => {
    if (issue.path?.includes('password')) {
      return { key: 'AxiomBase:InvalidFormatPassword' };
    }

    return null;
  },
};

export function setFormatResolver(
  key: string,
  resolver: AxiomInvalidFormatResolver,
) {
  invalidFormatResolvers[key] = resolver;
}
