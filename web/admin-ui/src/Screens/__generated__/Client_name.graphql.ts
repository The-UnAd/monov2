/**
 * @generated SignedSource<<3937e8511de84b13394c39699c7a8186>>
 * @lightSyntaxTransform
 * @nogrep
 */

/* tslint:disable */
/* eslint-disable */
// @ts-nocheck

import { Fragment, ReaderFragment } from 'relay-runtime';
import { FragmentRefs } from "relay-runtime";
export type Client_name$data = {
  readonly name: string;
  readonly " $fragmentType": "Client_name";
};
export type Client_name$key = {
  readonly " $data"?: Client_name$data;
  readonly " $fragmentSpreads": FragmentRefs<"Client_name">;
};

const node: ReaderFragment = {
  "argumentDefinitions": [],
  "kind": "Fragment",
  "metadata": null,
  "name": "Client_name",
  "selections": [
    {
      "alias": null,
      "args": null,
      "kind": "ScalarField",
      "name": "name",
      "storageKey": null
    }
  ],
  "type": "Client",
  "abstractKey": null
};

(node as any).hash = "3db26c1b44025f0fb1cfba02954842f7";

export default node;
