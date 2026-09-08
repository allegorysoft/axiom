import type { core } from 'zod';

export type RawIssue = core.$ZodRawIssue;
export type ZodIssueInvalidStringFormat = core.$ZodIssueInvalidStringFormat;

export type AxiomInvalidFormatResolver = (
  issue: ZodIssueInvalidStringFormat,
) => { key: string } | null;

export type ValidationOptions = {
  formatResolvers: Record<string, AxiomInvalidFormatResolver>;
};
