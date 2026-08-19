import {
  authorizationCodeGrant,
  buildAuthorizationUrl,
  calculatePKCECodeChallenge,
  randomPKCECodeVerifier,
  randomState,
} from 'openid-client';
import { BaseAuthFlow } from './base-auth-flow';

const PKCE_CODE_VERIFIER_KEY = 'pkce_code_verifier';
const OAUTH_STATE_KEY = 'oauth_state';

export class CodeAuthFlow extends BaseAuthFlow {
  override async initialize(): Promise<void> {
    await super.initialize();

    const currentUrl = new URL(window.location.href);

    if (currentUrl.searchParams.has('code')) {
      await this.handleCallback(currentUrl);
    }
  }

  private async handleCallback(currentUrl: URL): Promise<void> {
    const pkceCodeVerifier =
      sessionStorage.getItem(PKCE_CODE_VERIFIER_KEY) ?? undefined;
    const expectedState = sessionStorage.getItem(OAUTH_STATE_KEY) ?? undefined;

    try {
      const token = await authorizationCodeGrant(
        this.configuration!,
        currentUrl,
        { pkceCodeVerifier, expectedState },
      );

      sessionStorage.removeItem(PKCE_CODE_VERIFIER_KEY);
      sessionStorage.removeItem(OAUTH_STATE_KEY);

      this.setToken(token);
    } catch {}

    window.history.replaceState({}, '', window.location.pathname);
  }

  override async login(): Promise<void> {
    if (this.storage.get()?.accessToken) {
      return;
    }

    await this.redirectToAuthorization();
  }

  private async redirectToAuthorization(): Promise<void> {
    const redirectUri = this.options.redirectUri;

    if (!redirectUri) {
      throw new Error(
        'redirectUri is required but was not provided in options',
      );
    }

    const codeVerifier = randomPKCECodeVerifier();
    const codeChallenge = await calculatePKCECodeChallenge(codeVerifier);
    const state = randomState();

    sessionStorage.setItem(PKCE_CODE_VERIFIER_KEY, codeVerifier);
    sessionStorage.setItem(OAUTH_STATE_KEY, state);

    const authorizationUrl = buildAuthorizationUrl(this.configuration!, {
      redirect_uri: ensureEndsWithSlash(redirectUri),
      scope: this.options.scope,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state,
    });

    window.location.assign(authorizationUrl.href);
  }

  override redirectToLogin(navigator?: () => void, returnUrl?: string): void {
    this.login();
  }
}

function ensureEndsWithSlash(value?: string | null): string {
  if (!value) {
    return '/';
  }

  return value.endsWith('/') ? value : `${value}/`;
}
