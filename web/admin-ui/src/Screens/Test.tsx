import graphql from 'babel-plugin-relay/macro';

import {
  Box,
  Button,
  List,
  ListItem,
  MenuItem,
  Modal,
  Typography,
} from '@mui/material';
import { useMutation } from 'react-relay';
import type { TestQuery } from './__generated__/TestQuery.graphql';
import type { TestSendMessageMutation } from './__generated__/TestSendMessageMutation.graphql';
import type { RelayRoute, RouteDefinition } from '../Router/withRelay';
import { SubmitHandler, useForm } from 'react-hook-form';
import TextInput from '../Components/TextInput';
import SelectInput from '../Components/SelectInput';
import QuickModal from '../Components/QuickModal';
import { useState } from 'react';

export const TestQueryDef = graphql`
  query TestQuery {
    countClients
    countSubscribers
  }
`;

type Inputs = {
  audience: string;
  message: string;
};

export default function TestPage({ data }: Readonly<RelayRoute<TestQuery>>) {
  const {
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<Inputs>({
    defaultValues: {
      audience: 'CLIENTS',
      message: 'Testing 123...',
    },
  });
  const onSubmit: SubmitHandler<Inputs> = (data) => console.log(data);
  const [sendMessageMutation, sendMessageInFlight] =
    useMutation<TestSendMessageMutation>(graphql`
      mutation TestSendMessageMutation($sendMessageInput: SendMessageInput!) {
        sendMessage(input: $sendMessageInput) {
          errors
          sent
        }
      }
    `);

  const [sendResult, setSendResult] = useState<
    TestSendMessageMutation['response']['sendMessage'] | null
  >(null);
  const sendMessage = () => {
    // TODO: warn that this may incur charges
    sendMessageMutation({
      variables: {
        sendMessageInput: {
          audience: 'CLIENTS',
          message: 'Hello, world!',
        },
      },
      onError(error) {
        console.error(error);
      },
      onCompleted({ sendMessage }) {
        setSendResult(sendMessage);
      },
    });
  };
  return (
    <Box>
      <h1>Test</h1>
      <p>Number of clients: {data.countClients}</p>
      <p>Number of subscribers: {data.countSubscribers}</p>
      <form onSubmit={handleSubmit(onSubmit)}>
        <TextInput
          control={control}
          name="message"
          placeholder="Message"
          rules={{ required: true }}
        />
        {errors.message && <span>This field is required</span>}
        <SelectInput control={control} variant="outlined" name="audience">
          <MenuItem value="CLIENTS">All Clients</MenuItem>
          <MenuItem value="SUBSCRIBERS">All Subscribers</MenuItem>
        </SelectInput>
        <Button
          type="submit"
          onClick={sendMessage}
          disabled={sendMessageInFlight}
        >
          Send message
        </Button>
      </form>
      {sendResult && (
        <QuickModal
          title={`${sendResult?.sent} Messages Sent`}
          open={!!sendResult}
          onClose={() => setSendResult(null)}
        >
          <Box>
            {sendResult?.errors.length === 0 ? (
              <Typography>All messages sent successfully.</Typography>
            ) : (
              <List>
                {sendResult?.errors.map((error) => (
                  <ListItem key={error}>
                    <Typography>{error}</Typography>
                  </ListItem>
                ))}
              </List>
            )}
            <Typography variant="subtitle2">
              Click away from this dialog to close.
            </Typography>
          </Box>
        </QuickModal>
      )}
    </Box>
  );
}

export const route: RouteDefinition<TestQuery> = {
  path: '/test',
  component: TestPage,
  gqlQuery: TestQueryDef,
  query: require('./__generated__/TestQuery.graphql.ts'),
};
