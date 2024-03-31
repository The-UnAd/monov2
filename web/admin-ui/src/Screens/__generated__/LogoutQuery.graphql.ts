/**
 * @generated SignedSource<<85c11fda855e6b5114f2d39e29e2d11d>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type LogoutQuery$variables = Record<PropertyKey, never>;
export type LogoutQuery$data = {
  readonly viewer: {
    readonly id: string;
  } | null | undefined;
};
export type LogoutQuery = {
  response: LogoutQuery$data;
  variables: LogoutQuery$variables;
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
    "name": "LogoutQuery",
    "selections": (v0/*: any*/),
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "LogoutQuery",
    "selections": (v0/*: any*/)
  },
  "params": {
    "cacheID": "75d3cc532e11d0634122e9c8f7ce86c2",
    "id": null,
    "metadata": {},
    "name": "LogoutQuery",
    "operationKind": "query",
    "text": "query LogoutQuery {\n  viewer {\n    id\n  }\n}\n"
  }
};
})();

(node as any).hash = "f2a1372f23d11e43cedb0cfdd99ff9c9";

export default node;
