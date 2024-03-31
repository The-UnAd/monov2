import graphql from 'babel-plugin-relay/macro';

import {
  RelayRoute,
  RouteDefinition,
  useRelayScreenContext,
} from '../Router/withRelay';
import type { LogoutQuery } from './__generated__/LogoutQuery.graphql';
import { Box, Typography } from '@mui/material';
import { Redirect } from 'wouter';
import { useEffect } from 'react';
import { useAuth } from '../AuthProvider';

export const LogoutQueryDef = graphql`
  query LogoutQuery {
    viewer {
      id
    }
  }
`;

export default function LogoutPage({
  data,
}: Readonly<RelayRoute<LogoutQuery>>) {
  const { refresh } = useRelayScreenContext();
  const { storeToken } = useAuth();

  useEffect(() => {
    storeToken('');
    refresh();
  }, [storeToken, refresh]);

  return (
    <Box>
      <Typography variant="h1">Logging Out...</Typography>
    </Box>
  );
}

export const route: RouteDefinition<LogoutQuery> = {
  path: '/logout',
  gqlQuery: LogoutQueryDef,
  component: LogoutPage,
  query: require('./__generated__/LogoutQuery.graphql.ts'),
};
