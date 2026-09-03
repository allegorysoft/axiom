import type { HttpMiddleware } from '../http/http-client';
import { getErrorHandler } from '../error-handling/error-handler';

const exceptionHandlerMiddleware: HttpMiddleware = async (context, next) => {
  try {
    return await next();
  } catch (error) {
    getErrorHandler().handle(error, {
      url: context.url,
      method: context.request.method!,
    });

    throw error;
  }
};

export { exceptionHandlerMiddleware };
