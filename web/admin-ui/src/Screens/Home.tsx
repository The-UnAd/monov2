import graphql from 'babel-plugin-relay/macro';
import { Link } from 'wouter';

import { RelayRoute, RouteDefinition } from '../Router/withRelay';
import type { HomeQuery } from './__generated__/HomeQuery.graphql';

export const HomeQueryDef = graphql`
  query HomeQuery {
    clients(order: { name: ASC }) {
      edges {
        cursor
        node {
          id
          name
          subscriptionId
          subscription {
            status
          }
          subscriberPhoneNumbers {
            phoneNumber
          }
          subscribers {
            phoneNumber
            clients {
              name
            }
          }
        }
      }
    }
  }
`;

export default function HomePage({ data }: RelayRoute<HomeQuery>) {
  return (
    <div>
      <ul>
        {data?.clients?.edges?.map(({ node: client }) => (
          <li key={client.id}>
            <Link to={`/client/${client.id}`}>{client.name}</Link>
          </li>
        ))}
      </ul>
    </div>
  );
}

export const route: RouteDefinition<HomeQuery> = {
  path: '/',
  gqlQuery: HomeQueryDef,
  component: HomePage,
  query: require('./__generated__/HomeQuery.graphql.ts'),
};
