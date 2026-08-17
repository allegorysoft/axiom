import {
  type Configuration,
  type TokenEndpointResponse,
  type TokenEndpointResponseHelpers,
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

  protected setToken(
    token: TokenEndpointResponse & TokenEndpointResponseHelpers,
  ) {
    this.storage.set({
      accessToken: token.access_token,
      refreshToken: token.refresh_token,
      expiresAt: token.expires_in
        ? Date.now() + token.expires_in * 1000
        : undefined,
    });
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
}
