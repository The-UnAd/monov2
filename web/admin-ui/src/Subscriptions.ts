import graphql from 'babel-plugin-relay/macro';

import type { Environment } from 'react-relay';
import type { NewSubscriberSubscriptionData } from 'NewSubscriberSubscription.graphql';

import { useSubscription } from 'react-relay';
import { useMemo } from 'react';

export function useNewSubscriberSubscription() {
  const config = useMemo(
    () => ({
      subscription: graphql`
        subscription NewSubscriberSubscription {
          subscriberAdded {
            id
          }
        }
      `,
      variables: {},
    }),
    []
  );

  return useSubscription(config);
}
