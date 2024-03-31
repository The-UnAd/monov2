import graphql from 'babel-plugin-relay/macro';

import {
  Box,
  Button,
  List,
  ListItem,
  MenuItem,
  Typography,
} from '@mui/material';
import { useMutation } from 'react-relay';
import type { AnnouncementsQuery } from './__generated__/AnnouncementsQuery.graphql';
import type {
  AnnouncementsSendMessageMutation,
  AnnouncementsSendMessageMutation$variables,
} from './__generated__/AnnouncementsSendMessageMutation.graphql';
import type { RelayRoute, RouteDefinition } from '../Router/withRelay';
import { SubmitHandler, useForm } from 'react-hook-form';
import TextInput from '../Components/TextInput';
import SelectInput from '../Components/SelectInput';
import QuickModal from '../Components/QuickModal';
import { useState } from 'react';
import { pluralize } from '../util';
import CodeBlock from '../Components/CodeBlock';
import ErrorModal from '../Components/ErrorModal';

export const AnnouncementsQueryDef = graphql`
  query AnnouncementsQuery {
    countClients
    countSubscribers
  }
`;

const successModalStyle = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 600,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
};

const errorModalStyle = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 600,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
};

type FormInputs =
  AnnouncementsSendMessageMutation$variables['sendMessageInput'];

export default function AnnouncementsPage({
  data,
}: Readonly<RelayRoute<AnnouncementsQuery>>) {
  const {
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<FormInputs>({
    defaultValues: {
      audience: 'CLIENTS',
      message: 'Testing 123...',
    },
  });

  const [sendMessageMutation, sendMessageInFlight] =
    useMutation<AnnouncementsSendMessageMutation>(graphql`
      mutation AnnouncementsSendMessageMutation(
        $sendMessageInput: SendMessageInput!
      ) {
        sendMessage(input: $sendMessageInput) {
          errors {
            ... on SendMessageFailed {
              message
              sid
            }
            ... on Error {
              message
            }
          }
          sent
        }
      }
    `);

  const [sendResult, setSendResult] = useState<
    AnnouncementsSendMessageMutation['response']['sendMessage'] | null
  >(null);
  const [error, setError] = useState<Error | null>(null);

  const sendMessage: SubmitHandler<FormInputs> = (sendMessageInput) => {
    // TODO: warn that this may incur charges
    sendMessageMutation({
      variables: {
        sendMessageInput,
      },
      onError(error) {
        setError(error);
      },
      onCompleted({ sendMessage }) {
        setSendResult(sendMessage);
      },
    });
  };
  return (
    <Box>
      <h1>Announcements</h1>
      <p>Number of clients: {data.countClients}</p>
      <p>Number of subscribers: {data.countSubscribers}</p>
      <form onSubmit={handleSubmit(sendMessage)}>
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
        <Button type="submit" disabled={sendMessageInFlight}>
          Send message
        </Button>
      </form>
      {error && (
        <ErrorModal
          error={error}
          open={!!error}
          onClose={() => setError(null)}
        />
      )}
      {sendResult && (
        <QuickModal
          sx={successModalStyle}
          title={`${pluralize(sendResult?.sent ?? 0, 'Message', 'Messages')} Sent`}
          open={!!sendResult}
          onClose={() => setSendResult(null)}
        >
          <Box>
            {sendResult?.errors?.length === 0 ? (
              <Typography>All messages sent successfully.</Typography>
            ) : (
              <>
                <Typography>
                  {`${pluralize(sendResult?.errors?.length ?? 0, 'Error', 'Errors')} Occurred`}
                </Typography>
                <List>
                  {sendResult?.errors?.map(({ message }) => (
                    <ListItem key={message}>
                      <Typography>{message}</Typography>
                    </ListItem>
                  ))}
                </List>
              </>
            )}
            <Typography variant="caption">
              Click away from this dialog to close.
            </Typography>
          </Box>
        </QuickModal>
      )}
    </Box>
  );
}

export const route: RouteDefinition<AnnouncementsQuery> = {
  path: '/announcements',
  component: AnnouncementsPage,
  gqlQuery: AnnouncementsQueryDef,
  query: require('./__generated__/AnnouncementsQuery.graphql.ts'),
};
