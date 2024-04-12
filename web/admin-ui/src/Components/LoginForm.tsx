import {
  Box,
  Button,
  List,
  ListItem,
  MenuItem,
  Typography,
} from '@mui/material';
import { SubmitHandler, useForm } from 'react-hook-form';
import TextInput from '../Components/TextInput';
import SelectInput from '../Components/SelectInput';
import QuickModal from '../Components/QuickModal';
import { useState } from 'react';
import { pluralize } from '../util';
import CodeBlock from '../Components/CodeBlock';
import ErrorModal from '../Components/ErrorModal';
import { useAuth } from '../AuthProvider';

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

type FormInputs = {
  username: string;
  password: string;
};

type LoginFormProps = Readonly<{
  onLogin: () => void;
}>;

export default function LoginForm({ onLogin }: LoginFormProps) {
  const {
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<FormInputs>({
    defaultValues: {
      username: '',
      password: '',
    },
  });

  const { storeToken } = useAuth();
  const [loginInFlight, setLoginInFlight] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const login: SubmitHandler<FormInputs> = async ({ username, password }) => {
    try {
      setLoginInFlight(true);
      const res = await fetch('/auth', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
        },
        body: JSON.stringify({ username, password }),
      });

      const json = await res.json();
      if (!res.ok) {
        throw new Error(json.message);
      }
      storeToken(json.tokenId);
      onLogin();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoginInFlight(false);
    }
  };
  return (
    <Box>
      <form onSubmit={handleSubmit(login)}>
        <TextInput
          control={control}
          name="username"
          placeholder="Username"
          rules={{ required: true }}
        />
        {errors.username && <span>This field is required</span>}
        <TextInput
          control={control}
          name="password"
          type="password"
          rules={{ required: true }}
        />
        <Button type="submit">Login</Button>
      </form>
      {error && (
        <ErrorModal
          error={error}
          open={!!error}
          onClose={() => setError(null)}
        />
      )}
      {loginInFlight && (
        <QuickModal open={loginInFlight} allowClose={false} title="Logging In">
          <Typography>Please wait...</Typography>
        </QuickModal>
      )}
    </Box>
  );
}
