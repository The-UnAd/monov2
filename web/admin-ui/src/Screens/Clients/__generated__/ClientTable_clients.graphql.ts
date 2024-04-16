/**
 * @generated SignedSource<<da3a995210857c043955cb4aa59f4e0e>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ReaderFragment, RefetchableFragment } from 'relay-runtime';
import { FragmentRefs } from "relay-runtime";
export type ClientTable_clients$data = {
  readonly clients: {
    readonly edges: ReadonlyArray<{
      readonly cursor: string;
      readonly node: {
        readonly customerId: string | null | undefined;
        readonly id: string;
        readonly name: string;
        readonly phoneNumber: string;
        readonly slug: string;
        readonly subscribeLink: string;
        readonly subscriberCount: number;
        readonly subscription: {
          readonly status: string | null | undefined;
        } | null | undefined;
      };
    }> | null | undefined;
  } | null | undefined;
  readonly " $fragmentType": "ClientTable_clients";
};
export type ClientTable_clients$key = {
  readonly " $data"?: ClientTable_clients$data;
  readonly " $fragmentSpreads": FragmentRefs<"ClientTable_clients">;
};

const node: ReaderFragment = (function(){
var v0 = [
  "clients"
];
return {
  "argumentDefinitions": [
    {
      "defaultValue": 15,
      "kind": "LocalArgument",
      "name": "count"
    },
    {
      "defaultValue": null,
      "kind": "LocalArgument",
      "name": "cursor"
    }
  ],
  "kind": "Fragment",
  "metadata": {
    "connection": [
      {
        "count": "count",
        "cursor": "cursor",
        "direction": "forward",
        "path": (v0/*: any*/)
      }
    ],
    "refetch": {
      "connection": {
        "forward": {
          "count": "count",
          "cursor": "cursor"
        },
        "backward": null,
        "path": (v0/*: any*/)
      },
      "fragmentPathInResult": [],
      "operation": require('./ClientTablePaginationQuery.graphql')
    }
  },
  "name": "ClientTable_clients",
  "selections": [
    {
      "alias": "clients",
      "args": [
        {
          "kind": "Literal",
          "name": "order",
          "value": {
            "name": "ASC"
          }
        }
      ],
      "concreteType": "ClientsConnection",
      "kind": "LinkedField",
      "name": "__ClientTable_clients_connection",
      "plural": false,
      "selections": [
        {
          "alias": null,
          "args": null,
          "concreteType": "ClientsEdge",
          "kind": "LinkedField",
          "name": "edges",
          "plural": true,
          "selections": [
            {
              "alias": null,
              "args": null,
              "concreteType": "Client",
              "kind": "LinkedField",
              "name": "node",
              "plural": false,
              "selections": [
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "id",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "name",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "concreteType": "StripeSubscription",
                  "kind": "LinkedField",
                  "name": "subscription",
                  "plural": false,
                  "selections": [
                    {
                      "alias": null,
                      "args": null,
                      "kind": "ScalarField",
                      "name": "status",
                      "storageKey": null
                    }
                  ],
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "customerId",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "phoneNumber",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "slug",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "subscribeLink",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "subscriberCount",
                  "storageKey": null
                },
                {
                  "alias": null,
                  "args": null,
                  "kind": "ScalarField",
                  "name": "__typename",
                  "storageKey": null
                }
              ],
              "storageKey": null
            },
            {
              "alias": null,
              "args": null,
              "kind": "ScalarField",
              "name": "cursor",
              "storageKey": null
            }
          ],
          "storageKey": null
        },
        {
          "alias": null,
          "args": null,
          "concreteType": "PageInfo",
          "kind": "LinkedField",
          "name": "pageInfo",
          "plural": false,
          "selections": [
            {
              "alias": null,
              "args": null,
              "kind": "ScalarField",
              "name": "endCursor",
              "storageKey": null
            },
            {
              "alias": null,
              "args": null,
              "kind": "ScalarField",
              "name": "hasNextPage",
              "storageKey": null
            }
          ],
          "storageKey": null
        }
      ],
      "storageKey": "__ClientTable_clients_connection(order:{\"name\":\"ASC\"})"
    }
  ],
  "type": "Query",
  "abstractKey": null
};
})();

(node as any).hash = "fd014ad986d3a9fb5b9c4bc5b21a3577";

export default node;
