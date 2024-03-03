import React, { useCallback, useContext, useMemo } from 'react';
import { useQueryLoader } from 'react-relay';

export interface RelayNavigatorContextType {
  readonly suspenseFallback:
    | React.ReactNode
    | JSX.Element
    | (() => JSX.Element);
}
export interface RelayScreenContextType {
  readonly queryReference: any;
  readonly refresh: () => void;
  readonly variables: any;
}

const RelayNavigatorContext = React.createContext<RelayNavigatorContextType>(
  {} as RelayNavigatorContextType
);
const RelayScreenContext = React.createContext<RelayScreenContextType>(
  {} as RelayScreenContextType
);

export function useRelayNavigatorContext() {
  return useContext(RelayNavigatorContext);
}
export function useRelayScreenContext() {
  return useContext(RelayScreenContext);
}

interface ComponentWrapperProps {
  readonly Component: React.ComponentType<any>;
  readonly [key: string]: any;
}

const ComponentWrapper = React.forwardRef<unknown, ComponentWrapperProps>(
  ({ Component, ...props }, ref) => {
    return <Component ref={ref} {...props} />;
  }
);
ComponentWrapper.displayName = 'RelayComponentWrapper';

type RelayQuery = any; // TODO: better type here

export interface RouteDefinition {
  readonly query?: RelayQuery;
  readonly skeleton?: React.ReactNode | JSX.Element | (() => JSX.Element);
  readonly component: React.ComponentType<any>;
  readonly fetchPolicy?:
    | 'store-or-network'
    | 'store-and-network'
    | 'network-only';
}

export type RelayRouteDefinition<T extends RouteDefinition> = Omit<
  T,
  'component'
> &
  T;

type RelayScreenWrapperProps = Omit<RouteDefinition, 'component'> & {
  readonly skeleton?: React.ReactNode | JSX.Element | (() => JSX.Element);
  readonly component: React.ComponentType<any>;
  readonly queryVars: {
    readonly [key: string]: any;
  };
};

function RelayScreenWrapper({
  fetchPolicy,
  query,
  skeleton,
  component,
  queryVars,
  ...props
}: RelayScreenWrapperProps) {
  const { suspenseFallback } = useRelayNavigatorContext();
  const [queryReference, loadQuery, disposeQuery] = useQueryLoader(query);

  const vars = useMemo(() => ({ ...queryVars }), [queryVars]);

  React.useEffect(() => {
    if (!queryReference) {
      loadQuery(vars, { fetchPolicy });
    }
  }, [loadQuery, disposeQuery, queryReference, vars, fetchPolicy]);

  React.useEffect(() => () => disposeQuery(), [disposeQuery]);

  const refresh = useCallback(() => {
    loadQuery(vars, { fetchPolicy: 'network-only' });
  }, [loadQuery, vars]);
  const screenContextState = useMemo(
    () => ({ queryReference, refresh, variables: vars }),
    [queryReference, vars, refresh]
  );

  if (!queryReference) {
    return <ComponentWrapper Component={skeleton ?? suspenseFallback} />;
  }

  return (
    <RelayScreenContext.Provider value={screenContextState}>
      <React.Suspense
        fallback={<ComponentWrapper Component={skeleton ?? suspenseFallback} />}
      >
        <ComponentWrapper
          Component={component}
          queryReference={queryReference}
          {...props}
        />
      </React.Suspense>
    </RelayScreenContext.Provider>
  );
}

export interface RelayWrapperProps {
  readonly [key: string]: any;
}

export interface RelayNavigatorProps {
  readonly screens: RouteDefinition[];
}

export default function withRelay(
  WrappedNavigator: React.ComponentType<RelayNavigatorProps>,
  routeDefList: RouteDefinition[],
  suspenseFallback: React.ReactNode | JSX.Element | (() => JSX.Element)
) {
  const screens = routeDefList.map(({ query, component, ...rest }) => {
    return {
      ...rest,
      component: query
        ? function RelayQueryScreen(props: RelayScreenWrapperProps) {
            return (
              <RelayScreenWrapper
                {...props}
                {...rest}
                query={query}
                component={component}
              />
            );
          }
        : function RelayScreen(props: any) {
            return (
              <ComponentWrapper Component={component} {...props} {...rest} />
            );
          },
    };
  });

  return function RelayContextWrapper(wrapperProps: RelayWrapperProps) {
    const [contextValue] = React.useState({ suspenseFallback });
    return (
      <RelayNavigatorContext.Provider value={contextValue}>
        <WrappedNavigator {...wrapperProps} screens={screens} />
      </RelayNavigatorContext.Provider>
    );
  };
}
