/**
 * @generated SignedSource<<38fe6dd02ba4d62b595f24ca8562909c>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type LoginQuery$variables = Record<PropertyKey, never>;
export type LoginQuery$data = {
  readonly viewer: {
    readonly id: string;
  } | null | undefined;
};
export type LoginQuery = {
  response: LoginQuery$data;
  variables: LoginQuery$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "alias": null,
    "args": null,
    "concreteType": "User",
    "kind": "LinkedField",
    "name": "viewer",
    "plural": false,
    "selections": [
      {
        "alias": null,
        "args": null,
        "kind": "ScalarField",
        "name": "id",
        "storageKey": null
      }
    ],
    "storageKey": null
  }
];
return {
  "fragment": {
    "argumentDefinitions": [],
    "kind": "Fragment",
    "metadata": null,
    "name": "LoginQuery",
    "selections": (v0/*: any*/),
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "LoginQuery",
    "selections": (v0/*: any*/)
  },
  "params": {
    "cacheID": "902938c098a973f47117be5d290fa3f7",
    "id": null,
    "metadata": {},
    "name": "LoginQuery",
    "operationKind": "query",
    "text": "query LoginQuery {\n  viewer {\n    id\n  }\n}\n"
  }
};
})();

(node as any).hash = "1d9263b59931212ef05cd05ce8f99d04";

export default node;
