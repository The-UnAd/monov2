import {
  useController,
  type UseControllerProps,
  type FieldValues,
} from 'react-hook-form';
import {
  FormControl,
  InputLabel,
  Select,
  type SelectProps,
} from '@mui/material';
import { useId } from 'react';

type SelectInputProps<T extends FieldValues> = SelectProps &
  UseControllerProps<T>;

const SelectInput = <T extends FieldValues>({
  label,
  children,
  fullWidth,
  ...props
}: SelectInputProps<T>) => {
  const { field } = useController(props);
  const id = useId();

  return (
    <FormControl fullWidth={fullWidth}>
      <InputLabel id={id}>{label}</InputLabel>
      <Select labelId={id} {...field} {...props}>
        {children}
      </Select>
    </FormControl>
  );
};

export default SelectInput;
