import { memo } from 'react';
import {
  Route,
  RouteProps,
  Router,
  useParams,
  type RouterOptions,
} from 'wouter';

import ErrorBoundary from '../ErrorBoundary';
import { RelayNavigatorProps } from './withRelay';

type RelayNavigationScreenProps = RouteProps & {
  readonly Component: React.ComponentType<any>;
  readonly path: string;
};

const RelayNavigationRoute = memo(function RelayNavigationScreen({
  Component,
  path,
  ...props
}: RelayNavigationScreenProps) {
  const params = useParams();
  return (
    <ErrorBoundary>
      <Component queryVars={params} {...props} />
    </ErrorBoundary>
  );
});

RelayNavigationRoute.displayName = 'RelayNavigationScreen';

export default function createRouterFactory(
  options: RouterOptions = {},
  LayoutComponent: React.ComponentType<any>
) {
  return function RouterWrapper({
    screens,
    ...wrapperProps
  }: RelayNavigatorProps) {
    return (
      <Router {...wrapperProps} {...options}>
        <LayoutComponent>
          {screens.map(({ path, component, ...r }) => (
            <Route key={path} path={path}>
              <RelayNavigationRoute Component={component} path={path} {...r} />
            </Route>
          ))}
        </LayoutComponent>
      </Router>
    );
  };
}
