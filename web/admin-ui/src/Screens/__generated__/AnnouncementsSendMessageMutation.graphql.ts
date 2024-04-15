/**
 * @generated SignedSource<<cc8b550b9fa92d015684bf105674a1cf>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { ConcreteRequest, Mutation } from 'relay-runtime';
export type Audience = "ACTIVE_CLIENTS" | "ACTIVE_CLIENTS_WITHOUT_SUBSCRIBERS" | "ALL_CLIENTS" | "EVERYONE" | "SUBSCRIBERS" | "%future added value";
export type SendMessageInput = {
  audience: Audience;
  message: string;
};
export type AnnouncementsSendMessageMutation$variables = {
  sendMessageInput: SendMessageInput;
};
export type AnnouncementsSendMessageMutation$data = {
  readonly sendMessage: {
    readonly errors: ReadonlyArray<{
      readonly message?: string;
      readonly sid?: string | null | undefined;
    }> | null | undefined;
    readonly sent: number | null | undefined;
  };
};
export type AnnouncementsSendMessageMutation = {
  response: AnnouncementsSendMessageMutation$data;
  variables: AnnouncementsSendMessageMutation$variables;
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
    "kind": "Variable",
    "name": "input",
    "variableName": "sendMessageInput"
  }
],
v2 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "message",
  "storageKey": null
},
v3 = {
  "kind": "InlineFragment",
  "selections": [
    (v2/*: any*/),
    {
      "alias": null,
      "args": null,
      "kind": "ScalarField",
      "name": "sid",
      "storageKey": null
    }
  ],
  "type": "SendMessageFailed",
  "abstractKey": null
},
v4 = {
  "kind": "InlineFragment",
  "selections": [
    (v2/*: any*/)
  ],
  "type": "Error",
  "abstractKey": "__isError"
},
v5 = {
  "alias": null,
  "args": null,
  "kind": "ScalarField",
  "name": "sent",
  "storageKey": null
};
return {
  "fragment": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Fragment",
    "metadata": null,
    "name": "AnnouncementsSendMessageMutation",
    "selections": [
      {
        "alias": null,
        "args": (v1/*: any*/),
        "concreteType": "SendMessagePayload",
        "kind": "LinkedField",
        "name": "sendMessage",
        "plural": false,
        "selections": [
          {
            "alias": null,
            "args": null,
            "concreteType": null,
            "kind": "LinkedField",
            "name": "errors",
            "plural": true,
            "selections": [
              (v3/*: any*/),
              (v4/*: any*/)
            ],
            "storageKey": null
          },
          (v5/*: any*/)
        ],
        "storageKey": null
      }
    ],
    "type": "Mutation",
    "abstractKey": null
  },
  "kind": "Request",
  "operation": {
    "argumentDefinitions": (v0/*: any*/),
    "kind": "Operation",
    "name": "AnnouncementsSendMessageMutation",
    "selections": [
      {
        "alias": null,
        "args": (v1/*: any*/),
        "concreteType": "SendMessagePayload",
        "kind": "LinkedField",
        "name": "sendMessage",
        "plural": false,
        "selections": [
          {
            "alias": null,
            "args": null,
            "concreteType": null,
            "kind": "LinkedField",
            "name": "errors",
            "plural": true,
            "selections": [
              {
                "alias": null,
                "args": null,
                "kind": "ScalarField",
                "name": "__typename",
                "storageKey": null
              },
              (v3/*: any*/),
              (v4/*: any*/)
            ],
            "storageKey": null
          },
          (v5/*: any*/)
        ],
        "storageKey": null
      }
    ]
  },
  "params": {
    "cacheID": "7b7b3ee150caa5d84cc85d561132ddb3",
    "id": null,
    "metadata": {},
    "name": "AnnouncementsSendMessageMutation",
    "operationKind": "mutation",
    "text": "mutation AnnouncementsSendMessageMutation(\n  $sendMessageInput: SendMessageInput!\n) {\n  sendMessage(input: $sendMessageInput) {\n    errors {\n      __typename\n      ... on SendMessageFailed {\n        message\n        sid\n      }\n      ... on Error {\n        __isError: __typename\n        message\n      }\n    }\n    sent\n  }\n}\n"
  }
};
})();

(node as any).hash = "41b8c6e15039a3444c485fa1a6f02696";

export default node;
