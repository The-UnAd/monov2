/**
 * @generated SignedSource<<fc3e6e09c622c39eddb0f27ba0b308d1>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Query } from 'relay-runtime';
export type AnnouncementsQuery$variables = Record<PropertyKey, never>;
export type AnnouncementsQuery$data = {
  readonly totalClients: number;
  readonly totalSubscribers: number;
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
    "name": "totalClients",
    "storageKey": null
  },
  {
    "alias": null,
    "args": null,
    "kind": "ScalarField",
    "name": "totalSubscribers",
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
    "cacheID": "7b02d99a91154233a4479d1c047eddc7",
    "id": null,
    "metadata": {},
    "name": "AnnouncementsQuery",
    "operationKind": "query",
    "text": "query AnnouncementsQuery {\n  totalClients\n  totalSubscribers\n}\n"
  }
};
})();

(node as any).hash = "19f483d74271d5e2dc7a23c5b69c3e33";

export default node;
