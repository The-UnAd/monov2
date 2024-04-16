import graphql from 'babel-plugin-relay/macro';
import { RelayRoute, RouteDefinition } from '../../Router/withRelay';
import type { ClientsQuery } from './__generated__/ClientsQuery.graphql';
import ClientTable from './ClientTable';

export const ClientsQueryDef = graphql`
  query ClientsQuery {
    ...ClientTable_clients
  }
`;

type ClientsPageProps = Readonly<RelayRoute<ClientsQuery>>;

export default function ClientsPage({ data }: ClientsPageProps) {
  return (
    <div>
      <ClientTable query={data} />
    </div>
  );
}

export const route: RouteDefinition<ClientsQuery> = {
  path: '/clients',
  gqlQuery: ClientsQueryDef,
  component: ClientsPage,
  query: require('./__generated__/ClientsQuery.graphql.ts'),
};
