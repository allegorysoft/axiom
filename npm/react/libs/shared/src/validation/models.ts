import type { core } from 'zod';

export type RawIssue = core.$ZodRawIssue;
export type InvalidStringFormat = core.$ZodIssueInvalidStringFormat;

export type AxiomInvalidFormatResolver = (
  issue: InvalidStringFormat,
) => { key: string } | null;

export type ValidationOptions = {
  formatResolvers: Record<string, AxiomInvalidFormatResolver>;
};
