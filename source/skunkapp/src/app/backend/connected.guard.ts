import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { BackendService } from './backend.service';

export const connectedGuard: CanActivateFn = (route, state) => {
  const backend = inject(BackendService);
  const router = inject(Router);
  return backend.connected
    ? true
    : router.parseUrl('/');
};
