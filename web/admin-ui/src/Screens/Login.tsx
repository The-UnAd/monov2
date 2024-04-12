import graphql from 'babel-plugin-relay/macro';

import {
  RelayRoute,
  RouteDefinition,
  useRelayScreenContext,
} from '../Router/withRelay';
import type { LoginQuery } from './__generated__/LoginQuery.graphql';
import { Box, Typography } from '@mui/material';
import { Redirect } from 'wouter';
import LoginForm from '../Components/LoginForm';

export const LoginQueryDef = graphql`
  query LoginQuery {
    viewer {
      id
    }
  }
`;

// TODO: look into making sure we reload the page after login
// and more generally, making sure we load data on every nav, not just the first one
export default function LoginPage({ data }: Readonly<RelayRoute<LoginQuery>>) {
  const { refresh } = useRelayScreenContext();
  if (data.viewer?.id) {
    debugger;
    return <Redirect to="/" />;
  }

  return (
    <Box>
      <Typography variant="h1">Login</Typography>
      <pre>{JSON.stringify(data, null, 2)}</pre>
      <LoginForm onLogin={refresh} />
    </Box>
  );
}

export const route: RouteDefinition<LoginQuery> = {
  path: '/login',
  gqlQuery: LoginQueryDef,
  component: LoginPage,
  query: require('./__generated__/LoginQuery.graphql.ts'),
};
