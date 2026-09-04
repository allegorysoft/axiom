import { describe, expect, it, vi } from 'vitest';
import { HttpError, NetworkError } from '../error-handling/models';
import { ErrorHandler } from '../error-handling/error-handler';

describe('ErrorHandler', () => {
  describe('constructor', () => {
    it('uses provided handlers', () => {
      const handlers = {
        401: vi.fn(),
      };
      const onNetworkError = vi.fn();
      const onUnknownError = vi.fn();

      const errorHandler = new ErrorHandler({
        handlers,
        onNetworkError,
        onUnknownError,
      });

      const httpError = new HttpError(
        new Response(null, {
          status: 401,
          statusText: 'Unauthorized',
        }),
      );

      errorHandler.handle(httpError);

      expect(handlers[401]).toHaveBeenCalledOnce();
      expect(handlers[401]).toHaveBeenCalledWith(httpError);
    });
  });

  describe('handle', () => {
    it('handles network errors', () => {
      const onNetworkError = vi.fn();
      const errorHandler = new ErrorHandler({ onNetworkError });
      const error = new NetworkError('offline', {
        url: '/users',
        method: 'GET',
      });

      errorHandler.handle(error);

      expect(onNetworkError).toHaveBeenCalledOnce();
      expect(onNetworkError).toHaveBeenCalledWith(error);
    });

    it('handles HTTP errors using their status handler', () => {
      const onUnauthorized = vi.fn();
      const errorHandler = new ErrorHandler({
        handlers: {
          401: onUnauthorized,
        },
      });

      const error = new HttpError(
        new Response(null, {
          status: 401,
          statusText: 'Unauthorized',
        }),
      );

      errorHandler.handle(error);

      expect(onUnauthorized).toHaveBeenCalledOnce();
      expect(onUnauthorized).toHaveBeenCalledWith(error);
    });

    it('falls back to unknown error handler when HTTP status is not handled', () => {
      const onUnknownError = vi.fn();
      const handler = new ErrorHandler({ onUnknownError });

      const error = new HttpError(
        new Response(null, {
          status: 418,
          statusText: "I'm a teapot",
        }),
      );

      handler.handle(error);

      expect(onUnknownError).toHaveBeenCalledWith(error);
    });

    it('falls back to unknown error handler for unknown errors', () => {
      const onUnknownError = vi.fn();
      const errorHandler = new ErrorHandler({ onUnknownError });
      const error = new Error('Something went wrong');

      errorHandler.handle(error);

      expect(onUnknownError).toHaveBeenCalledOnce();
      expect(onUnknownError).toHaveBeenCalledWith(error);
    });

    it('does not call unknown error handler for handled errors', () => {
      const onUnauthorized = vi.fn();
      const onUnknownError = vi.fn();

      const errorHandler = new ErrorHandler({
        handlers: {
          401: onUnauthorized,
        },
        onUnknownError,
      });

      const error = new HttpError(
        new Response(null, {
          status: 401,
          statusText: 'Unauthorized',
        }),
      );

      errorHandler.handle(error);

      expect(onUnauthorized).toHaveBeenCalledOnce();
      expect(onUnknownError).not.toHaveBeenCalled();
    });

    it('handles a NetworkError before checking HTTP status handlers', () => {
      const onNetworkError = vi.fn();
      const onUnknownError = vi.fn();

      const errorHandler = new ErrorHandler({
        onNetworkError,
        onUnknownError,
      });

      const error = new NetworkError('timeout');

      errorHandler.handle(error);

      expect(onNetworkError).toHaveBeenCalledOnce();
      expect(onUnknownError).not.toHaveBeenCalled();
    });
  });

  describe('set', () => {
    it('updates status handlers', () => {
      const firstHandler = vi.fn();
      const secondHandler = vi.fn();

      const errorHandler = new ErrorHandler({
        handlers: {
          401: firstHandler,
        },
      });

      const error = new HttpError(
        new Response(null, {
          status: 401,
          statusText: 'Unauthorized',
        }),
      );

      errorHandler.handle(error);

      errorHandler.set({
        handlers: {
          401: secondHandler,
        },
      });

      errorHandler.handle(error);

      expect(firstHandler).toHaveBeenCalledOnce();
      expect(secondHandler).toHaveBeenCalledOnce();
    });

    it('updates network error handler', () => {
      const firstHandler = vi.fn();
      const secondHandler = vi.fn();

      const errorHandler = new ErrorHandler({
        onNetworkError: firstHandler,
      });

      const error = new NetworkError('offline');

      errorHandler.handle(error);

      errorHandler.set({
        onNetworkError: secondHandler,
      });

      errorHandler.handle(error);

      expect(firstHandler).toHaveBeenCalledOnce();
      expect(secondHandler).toHaveBeenCalledOnce();
    });

    it('updates unknown error handler', () => {
      const firstHandler = vi.fn();
      const secondHandler = vi.fn();
      const error = new Error('Unknown');

      const errorHandler = new ErrorHandler({
        onUnknownError: firstHandler,
      });

      errorHandler.handle(error);

      errorHandler.set({
        onUnknownError: secondHandler,
      });

      errorHandler.handle(error);

      expect(firstHandler).toHaveBeenCalledOnce();
      expect(secondHandler).toHaveBeenCalledOnce();
    });

    it('does not change handlers when corresponding options are omitted', () => {
      const onNetworkError = vi.fn();
      const onUnknownError = vi.fn();

      const errorHandler = new ErrorHandler({
        onNetworkError,
        onUnknownError,
      });

      errorHandler.set({});

      errorHandler.handle(new NetworkError('offline'));
      errorHandler.handle(new Error('Unknown'));

      expect(onNetworkError).toHaveBeenCalledOnce();
      expect(onUnknownError).toHaveBeenCalledOnce();
    });
  });
});
