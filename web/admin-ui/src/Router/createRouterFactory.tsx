import { memo } from 'react';
import { Route, RouteProps, useParams } from 'wouter';

import ErrorBoundary from '../ErrorBoundary';
import { RelayNavigatorProps } from './withRelay';

type RelayNavigationScreenProps = RouteProps &
  Readonly<{
    Component: React.ComponentType<any>;
  }>;

const RelayNavigationRoute = memo(function RelayNavigationScreen({
  Component,
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

export default function createRouterFactory() {
  return function RouterWrapper({ screens }: RelayNavigatorProps) {
    return (
      <>
        {screens.map(({ path, component, ...r }) => (
          <Route key={path} path={path}>
            <RelayNavigationRoute Component={component} {...r} />
          </Route>
        ))}
      </>
    );
  };
}
