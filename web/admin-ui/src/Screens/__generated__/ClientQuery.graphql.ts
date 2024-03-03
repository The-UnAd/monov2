/**
 * @generated SignedSource<<dc5aafd40095f29963e3bcc763126ad6>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
import { FragmentRefs } from "relay-runtime";
export type ClientQuery$variables = {
  clientId: string;
};
export type ClientQuery$data = {
  readonly client: {
    readonly id: string;
    readonly " $fragmentSpreads": FragmentRefs<"Client_name">;
  } | null | undefined;
};
export type ClientQuery = {
  response: ClientQuery$data;
  variables: ClientQuery$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "defaultValue": null,
    "kind": "LocalArgument",
    "name": "clientId"
  }
],
v1 = [
  {
    "kind": "Variable",
    "name": "id",
    "variableName": "clientId"
  }
],
v2 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "id",
  "storageKey": null
};
return {
  "fragment": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Fragment",
    "metadata": null,
    "name": "ClientQuery",
    "selections": [
      {
        "alias": null,
        "args": (v1/*: any*/),
        "concreteType": "Client",
        "kind": "LinkedField",
        "name": "client",
        "plural": false,
        "selections": [
          (v2/*: any*/),
          {
            "args": null,
            "kind": "FragmentSpread",
            "name": "Client_name"
          }
        ],
        "storageKey": null
      }
    ],
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Operation",
    "name": "ClientQuery",
    "selections": [
      {
        "alias": null,
        "args": (v1/*: any*/),
        "concreteType": "Client",
        "kind": "LinkedField",
        "name": "client",
        "plural": false,
        "selections": [
          (v2/*: any*/),
          {
            "alias": null,
            "args": null,
            "kind": "ScalarField",
            "name": "name",
            "storageKey": null
          }
        ],
        "storageKey": null
      }
    ]
  },
  "params": {
    "cacheID": "2824cd59cdf877c3bfcdb4a0fb636742",
    "id": null,
    "metadata": {},
    "name": "ClientQuery",
    "operationKind": "query",
    "text": "query ClientQuery(\n  $clientId: ID!\n) {\n  client(id: $clientId) {\n    id\n    ...Client_name\n  }\n}\n\nfragment Client_name on Client {\n  name\n}\n"
  }
};
})();

(node as any).hash = "ba87305b69f330871e713bd112802bff";

export default node;
