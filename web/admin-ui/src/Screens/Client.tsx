import graphql from 'babel-plugin-relay/macro';
import { useFragment } from 'react-relay/hooks';

import { RelayRoute } from '../Router/withRelay';
import { ClientQuery } from './__generated__/ClientQuery.graphql';

export const ClientQueryDef = graphql`
  query ClientQuery($clientId: ID!) {
    client(id: $clientId) {
      id
      ...Client_name
    }
  }
`;

function ClientName({ client }: any) {
  const data = useFragment(
    graphql`
      fragment Client_name on Client {
        name
      }
    `,
    client
  );
  return <p>Client: {data.name}!</p>;
}

export default function ClientPage({ data }: RelayRoute<ClientQuery>) {
  return (
    <div>
      <ClientName client={data.client} />
    </div>
  );
}

export const route = {
  path: '/client/:clientId',
  gqlQuery: ClientQueryDef,
  component: ClientPage,
  query: require('./__generated__/ClientQuery.graphql.ts'),
};
