import type { RouteObject } from 'react-router';
import { MODULE_INFO } from './config/module';

export function module1Routes(): RouteObject[] {
  return [
    {
      path: MODULE_INFO.url,
      handle: { module: MODULE_INFO.name },
      children: [
        {
          path: 'entity-1',
          lazy: () => import('./components/entity-1'),
        },
        {
          path: 'entity-2',
          lazy: () => import('./components/entity-2'),
        },
      ],
    },
  ];
}
