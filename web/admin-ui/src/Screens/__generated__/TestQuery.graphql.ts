/**
 * @generated SignedSource<<2460d6c212c740d86d1072ee4acd6564>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type TestQuery$variables = Record<PropertyKey, never>;
export type TestQuery$data = {
  readonly countClients: number;
  readonly countSubscribers: number;
};
export type TestQuery = {
  response: TestQuery$data;
  variables: TestQuery$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "alias": null,
    "args": null,
    "kind": "ScalarField",
    "name": "countClients",
    "storageKey": null
  },
  {
    "alias": null,
    "args": null,
    "kind": "ScalarField",
    "name": "countSubscribers",
    "storageKey": null
  }
];
return {
  "fragment": {
    "argumentDefinitions": [],
    "kind": "Fragment",
    "metadata": null,
    "name": "TestQuery",
    "selections": (v0/*: any*/),
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "TestQuery",
    "selections": (v0/*: any*/)
  },
  "params": {
    "cacheID": "f4922e9fd5030d3c062b17b3b044cead",
    "id": null,
    "metadata": {},
    "name": "TestQuery",
    "operationKind": "query",
    "text": "query TestQuery {\n  countClients\n  countSubscribers\n}\n"
  }
};
})();

(node as any).hash = "3c92151974723dfb39e020cc37ceb652";

export default node;
