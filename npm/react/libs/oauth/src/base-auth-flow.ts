import {
  type Configuration,
  allowInsecureRequests,
  discovery,
  tokenRevocation,
} from 'openid-client';
import { AbstractAuthFlow, isDevMode } from '@axiomframework/react-core';

const noop = (): void => {};

export abstract class BaseAuthFlow extends AbstractAuthFlow {
  protected configuration?: Configuration;

  /**
   * Entry of authentication flow
   */
  override async initialize(): Promise<void> {
    await this.discover();
  }

  override async logout(): Promise<void> {
    if (!this.configuration) {
      return;
    }
    const accessToken = this.storage.get()?.accessToken;
    if (accessToken) {
      await tokenRevocation(this.configuration, accessToken);
    }

    this.storage.clear();
  }

  protected async discover(): Promise<void> {
    const realmName = 'master'; //Tenant
    const configuration = await discovery(
      new URL(`${this.options.authority}/realms/${realmName}`),
      this.options.clientId,
      {},
      undefined,
      { execute: [isDevMode() ? allowInsecureRequests : noop] },
    );

    this.configuration = configuration;
  }
}
