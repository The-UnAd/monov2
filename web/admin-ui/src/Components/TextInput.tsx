import {
  useController,
  type UseControllerProps,
  type FieldValues,
} from 'react-hook-form';
import { Input, InputProps } from '@mui/material';

type TextInputProps<T extends FieldValues> = InputProps & UseControllerProps<T>;

const TextInput = <T extends FieldValues>(props: TextInputProps<T>) => {
  const { field, fieldState } = useController(props);

  return <Input {...props} {...field} />;
};

export default TextInput;
