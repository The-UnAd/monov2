import { memo } from 'react';
import { Route, Router, useParams } from 'wouter';

import ErrorBoundary from '../ErrorBoundary';

const RelayNavigationRoute = memo(function RelayNavigationScreen({
  Component,
  path,
  ...props
}) {
  const params = useParams(path);
  return (
    <ErrorBoundary>
      <Component queryVars={params} {...props} />
    </ErrorBoundary>
  );
});

RelayNavigationRoute.displayName = 'RelayNavigationScreen';

export default function relayRouterFactory() {
  return function RouterWrapper({ screens, ...wrapperProps }) {
    return (
      <Router {...wrapperProps}>
        {screens.map(({ path, component, ...r }) => (
          <Route key={path} path={path}>
            <RelayNavigationRoute Component={component} path={path} {...r} />
          </Route>
        ))}
      </Router>
    );
  };
}
