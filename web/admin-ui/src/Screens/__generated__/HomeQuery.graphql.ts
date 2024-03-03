/**
 * @generated SignedSource<<c15f51fad26915512b7deca16d79d9b6>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type HomeQuery$variables = Record<PropertyKey, never>;
export type HomeQuery$data = {
  readonly clients: {
    readonly edges: ReadonlyArray<{
      readonly cursor: string;
      readonly node: {
        readonly id: string;
        readonly name: string;
        readonly subscriberPhoneNumbers: ReadonlyArray<{
          readonly phoneNumber: string;
        }>;
        readonly subscribers: ReadonlyArray<{
          readonly clients: ReadonlyArray<{
            readonly name: string;
          }>;
          readonly phoneNumber: string;
        }>;
        readonly subscription: {
          readonly status: string | null | undefined;
        } | null | undefined;
        readonly subscriptionId: string | null | undefined;
      };
    }> | null | undefined;
  } | null | undefined;
};
export type HomeQuery = {
  response: HomeQuery$data;
  variables: HomeQuery$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "kind": "Literal",
    "name": "order",
    "value": {
      "name": "ASC"
    }
  }
],
v1 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "cursor",
  "storageKey": null
},
v2 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "id",
  "storageKey": null
},
v3 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "name",
  "storageKey": null
},
v4 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "subscriptionId",
  "storageKey": null
},
v5 = {
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
v6 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "phoneNumber",
  "storageKey": null
},
v7 = {
  "alias": null,
  "args": null,
  "concreteType": "Subscriber",
  "kind": "LinkedField",
  "name": "subscriberPhoneNumbers",
  "plural": true,
  "selections": [
    (v6/*: any*/)
  ],
  "storageKey": null
};
return {
  "fragment": {
    "argumentDefinitions": [],
    "kind": "Fragment",
    "metadata": null,
    "name": "HomeQuery",
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
              (v1/*: any*/),
              {
                "alias": null,
                "args": null,
                "concreteType": "Client",
                "kind": "LinkedField",
                "name": "node",
                "plural": false,
                "selections": [
                  (v2/*: any*/),
                  (v3/*: any*/),
                  (v4/*: any*/),
                  (v5/*: any*/),
                  (v7/*: any*/),
                  {
                    "alias": null,
                    "args": null,
                    "concreteType": "Subscriber",
                    "kind": "LinkedField",
                    "name": "subscribers",
                    "plural": true,
                    "selections": [
                      (v6/*: any*/),
                      {
                        "alias": null,
                        "args": null,
                        "concreteType": "Client",
                        "kind": "LinkedField",
                        "name": "clients",
                        "plural": true,
                        "selections": [
                          (v3/*: any*/)
                        ],
                        "storageKey": null
                      }
                    ],
                    "storageKey": null
                  }
                ],
                "storageKey": null
              }
            ],
            "storageKey": null
          }
        ],
        "storageKey": "clients(order:{\"name\":\"ASC\"})"
      }
    ],
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "HomeQuery",
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
              (v1/*: any*/),
              {
                "alias": null,
                "args": null,
                "concreteType": "Client",
                "kind": "LinkedField",
                "name": "node",
                "plural": false,
                "selections": [
                  (v2/*: any*/),
                  (v3/*: any*/),
                  (v4/*: any*/),
                  (v5/*: any*/),
                  (v7/*: any*/),
                  {
                    "alias": null,
                    "args": null,
                    "concreteType": "Subscriber",
                    "kind": "LinkedField",
                    "name": "subscribers",
                    "plural": true,
                    "selections": [
                      (v6/*: any*/),
                      {
                        "alias": null,
                        "args": null,
                        "concreteType": "Client",
                        "kind": "LinkedField",
                        "name": "clients",
                        "plural": true,
                        "selections": [
                          (v3/*: any*/),
                          (v2/*: any*/)
                        ],
                        "storageKey": null
                      }
                    ],
                    "storageKey": null
                  }
                ],
                "storageKey": null
              }
            ],
            "storageKey": null
          }
        ],
        "storageKey": "clients(order:{\"name\":\"ASC\"})"
      }
    ]
  },
  "params": {
    "cacheID": "e1507c7749e758b0a8f3bd6cb0748b05",
    "id": null,
    "metadata": {},
    "name": "HomeQuery",
    "operationKind": "query",
    "text": "query HomeQuery {\n  clients(order: {name: ASC}) {\n    edges {\n      cursor\n      node {\n        id\n        name\n        subscriptionId\n        subscription {\n          status\n        }\n        subscriberPhoneNumbers {\n          phoneNumber\n        }\n        subscribers {\n          phoneNumber\n          clients {\n            name\n            id\n          }\n        }\n      }\n    }\n  }\n}\n"
  }
};
})();

(node as any).hash = "8832362ac9f777f66904c704ca969d09";

export default node;
