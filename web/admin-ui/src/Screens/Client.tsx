import graphql from 'babel-plugin-relay/macro';
import { useFragment, usePreloadedQuery } from 'react-relay/hooks';

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

export default function ClientPage(props: any) {
  const data = usePreloadedQuery<ClientQuery>(
    ClientQueryDef,
    props.queryReference
  );

  return (
    <div>
      <ClientName client={data.client} />
    </div>
  );
}

export const route = {
  path: '/client/:clientId',
  component: ClientPage,
  query: require('./__generated__/ClientQuery.graphql.ts'),
};
