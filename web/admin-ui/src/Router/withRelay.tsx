import React, { useCallback, useContext, useMemo } from 'react';
import {
  type PreloadedQuery,
  usePreloadedQuery,
  useQueryLoader,
} from 'react-relay';
import type {
  GraphQLTaggedNode,
  OperationType,
  PreloadableConcreteRequest,
} from 'relay-runtime';

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
interface RelayComponentWrapperProps<TQuery extends OperationType> {
  readonly Component: React.ComponentType<any>;
  readonly gqlQuery: GraphQLTaggedNode;
  readonly queryReference: PreloadedQuery<TQuery>;
}

const ComponentWrapper = React.forwardRef<unknown, ComponentWrapperProps>(
  ({ Component, ...props }, ref) => {
    return <Component ref={ref} {...props} />;
  }
);
ComponentWrapper.displayName = 'RelayComponentWrapper';

function RelayComponentWrapper<T extends OperationType>({
  Component,
  gqlQuery,
  queryReference,
  ...props
}: RelayComponentWrapperProps<T>) {
  const data = usePreloadedQuery(gqlQuery, queryReference);
  return <Component Component={Component} data={data} {...props} />;
}

export type RelayRoute<T extends OperationType> = {
  readonly data: T['response'];
};

export interface RouteDefinition<T extends OperationType> {
  readonly path: string;
  readonly query: PreloadableConcreteRequest<T>;
  readonly gqlQuery: GraphQLTaggedNode;
  readonly skeleton?: React.ReactNode | JSX.Element | (() => JSX.Element);
  readonly component: React.ComponentType<any>;
  readonly fetchPolicy?:
    | 'store-or-network'
    | 'store-and-network'
    | 'network-only';
}

type RelayScreenWrapperProps<T extends OperationType = OperationType> =
  RouteDefinition<T> & {
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
  gqlQuery,
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
        <RelayComponentWrapper
          Component={component}
          queryReference={queryReference}
          gqlQuery={gqlQuery}
          {...props}
        />
      </React.Suspense>
    </RelayScreenContext.Provider>
  );
}

export interface RelayWrapperProps {
  readonly [key: string]: any;
}

export interface RelayNavigatorProps<T extends OperationType = OperationType> {
  readonly screens: RouteDefinition<T>[];
}

export default function withRelay<T extends OperationType = OperationType>(
  WrappedNavigator: React.ComponentType<any>,
  routeDefList: RouteDefinition<T>[],
  suspenseFallback: React.ReactNode | JSX.Element | (() => JSX.Element)
) {
  const screens = routeDefList.map(({ query, component, ...rest }) => {
    return {
      ...rest,
      component: function RelayQueryScreen(props: RelayScreenWrapperProps) {
        return (
          <RelayScreenWrapper {...props} query={query} component={component} />
        );
      },
    };
  });

  return function RelayContextWrapper(wrapperProps: any) {
    const [contextValue] = React.useState({ suspenseFallback });
    return (
      <RelayNavigatorContext.Provider value={contextValue}>
        <WrappedNavigator {...wrapperProps} screens={screens} />
      </RelayNavigatorContext.Provider>
    );
  };
}
