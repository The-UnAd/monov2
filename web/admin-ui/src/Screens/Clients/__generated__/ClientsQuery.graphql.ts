/**
 * @generated SignedSource<<7a45169dc209b50a48c45c41c2b1e11e>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
import { FragmentRefs } from "relay-runtime";
export type ClientsQuery$variables = Record<PropertyKey, never>;
export type ClientsQuery$data = {
  readonly " $fragmentSpreads": FragmentRefs<"ClientTable_clients">;
};
export type ClientsQuery = {
  response: ClientsQuery$data;
  variables: ClientsQuery$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "kind": "Literal",
    "name": "first",
    "value": 15
  },
  {
    "kind": "Literal",
    "name": "order",
    "value": {
      "name": "ASC"
    }
  }
];
return {
  "fragment": {
    "argumentDefinitions": [],
    "kind": "Fragment",
    "metadata": null,
    "name": "ClientsQuery",
    "selections": [
      {
        "args": null,
        "kind": "FragmentSpread",
        "name": "ClientTable_clients"
      }
    ],
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "ClientsQuery",
    "selections": [
      {
        "alias": null,
        "args": (v0/*: any*/),
        "concreteType": "ClientsConnection",
        "kind": "LinkedField",
        "name": "clients",
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
        "storageKey": "clients(first:15,order:{\"name\":\"ASC\"})"
      },
      {
        "alias": null,
        "args": (v0/*: any*/),
        "filters": [
          "order"
        ],
        "handle": "connection",
        "key": "ClientTable_clients",
        "kind": "LinkedHandle",
        "name": "clients"
      }
    ]
  },
  "params": {
    "cacheID": "b5bd12988e4f3b10b1af5c70d035a59e",
    "id": null,
    "metadata": {},
    "name": "ClientsQuery",
    "operationKind": "query",
    "text": "query ClientsQuery {\n  ...ClientTable_clients\n}\n\nfragment ClientTable_clients on Query {\n  clients(first: 15, order: {name: ASC}) {\n    edges {\n      node {\n        id\n        name\n        subscription {\n          status\n        }\n        customerId\n        phoneNumber\n        slug\n        subscribeLink\n        subscriberCount\n        __typename\n      }\n      cursor\n    }\n    pageInfo {\n      endCursor\n      hasNextPage\n    }\n  }\n}\n"
  }
};
})();

(node as any).hash = "7f8acc15031ba49e8d4b2f99b2331537";

export default node;
