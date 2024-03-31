/**
 * @generated SignedSource<<a49eb1db606efdd1f8e7efbb577a4077>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type AnnouncementsQuery$variables = Record<PropertyKey, never>;
export type AnnouncementsQuery$data = {
  readonly countClients: number;
  readonly countSubscribers: number;
};
export type AnnouncementsQuery = {
  response: AnnouncementsQuery$data;
  variables: AnnouncementsQuery$variables;
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
    "name": "AnnouncementsQuery",
    "selections": (v0/*: any*/),
    "type": "Query",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": [],
    "kind": "Operation",
    "name": "AnnouncementsQuery",
    "selections": (v0/*: any*/)
  },
  "params": {
    "cacheID": "d8523fbb86ca4139fdd90516cc294345",
    "id": null,
    "metadata": {},
    "name": "AnnouncementsQuery",
    "operationKind": "query",
    "text": "query AnnouncementsQuery {\n  countClients\n  countSubscribers\n}\n"
  }
};
})();

(node as any).hash = "e76929cec8c7f0e74bd3b88e125b0b23";

export default node;
