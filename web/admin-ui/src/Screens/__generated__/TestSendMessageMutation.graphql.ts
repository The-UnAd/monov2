/**
 * @generated SignedSource<<2c96a812f3363117ba5f1e6301b36455>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Mutation } from 'relay-runtime';
export type Audience = "CLIENTS" | "SUBSCRIBERS" | "%future added value";
export type SendMessageInput = {
  audience: Audience;
  message: string;
};
export type TestSendMessageMutation$variables = {
  sendMessageInput: SendMessageInput;
};
export type TestSendMessageMutation$data = {
  readonly sendMessage: {
    readonly errors: ReadonlyArray<string>;
    readonly sent: number;
  };
};
export type TestSendMessageMutation = {
  response: TestSendMessageMutation$data;
  variables: TestSendMessageMutation$variables;
};

const node: ConcreteRequest = (function(){
var v0 = [
  {
    "defaultValue": null,
    "kind": "LocalArgument",
    "name": "sendMessageInput"
  }
],
v1 = [
  {
    "alias": null,
    "args": [
      {
        "kind": "Variable",
        "name": "input",
        "variableName": "sendMessageInput"
      }
    ],
    "concreteType": "SendMessagePayload",
    "kind": "LinkedField",
    "name": "sendMessage",
    "plural": false,
    "selections": [
      {
        "alias": null,
        "args": null,
        "kind": "ScalarField",
        "name": "errors",
        "storageKey": null
      },
      {
        "alias": null,
        "args": null,
        "kind": "ScalarField",
        "name": "sent",
        "storageKey": null
      }
    ],
    "storageKey": null
  }
];
return {
  "fragment": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Fragment",
    "metadata": null,
    "name": "TestSendMessageMutation",
    "selections": (v1/*: any*/),
    "type": "Mutation",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Operation",
    "name": "TestSendMessageMutation",
    "selections": (v1/*: any*/)
  },
  "params": {
    "cacheID": "fab623fba8440c16ff2be024c689ac0d",
    "id": null,
    "metadata": {},
    "name": "TestSendMessageMutation",
    "operationKind": "mutation",
    "text": "mutation TestSendMessageMutation(\n  $sendMessageInput: SendMessageInput!\n) {\n  sendMessage(input: $sendMessageInput) {\n    errors\n    sent\n  }\n}\n"
  }
};
})();

(node as any).hash = "992e32ca586ab5ea2ca3973f0d54bf92";

export default node;
