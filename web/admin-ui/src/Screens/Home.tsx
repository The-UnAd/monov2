import graphql from 'babel-plugin-relay/macro';
import { Link } from 'wouter';

import { RelayRoute, RouteDefinition } from '../Router/withRelay';
import type { HomeQuery } from './__generated__/HomeQuery.graphql';
import { Box, List, ListItem } from '@mui/material';

export const HomeQueryDef = graphql`
  query HomeQuery {
    clients(order: { name: ASC }) {
      edges {
        cursor
        node {
          id
          name
          subscriberPhoneNumbers {
            __typename
          }
        }
      }
    }
  }
`;

export default function HomePage({ data }: RelayRoute<HomeQuery>) {
  return (
    <Box>
      <List>
        {data?.clients?.edges?.map(({ node: client }) => (
          <ListItem key={client.id}>
            <span>
              <Link to={`/client/${client.id}`}>{client.name}</Link> has{' '}
              {client.subscriberPhoneNumbers.length} subscribers
            </span>
          </ListItem>
        ))}
      </List>
    </Box>
  );
}

export const route: RouteDefinition<HomeQuery> = {
  path: '/',
  gqlQuery: HomeQueryDef,
  component: HomePage,
  query: require('./__generated__/HomeQuery.graphql.ts'),
};
