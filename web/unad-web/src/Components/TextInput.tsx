import React, { useId } from 'react';
import { Controller, useFormContext } from 'react-hook-form';

type TextInputProps = React.InputHTMLAttributes<HTMLInputElement> & {
  name: string;
  defaultValue?: string;
  'data-testid'?: string;
  label: string;
};

const TextInput = (props: TextInputProps) => {
  const {
    name,
    defaultValue,
    // @ts-ignore
    'data-testid': testId,
    label,
    ...rest
  } = props;
  const {
    control,
    formState: { errors },
  } = useFormContext();
  const idHint = useId();
  return (
    <Controller
      name={name}
      control={control}
      defaultValue={defaultValue ?? ''}
      render={({ field }) => {
        const { ref, ...fieldProps } = field;
        return (
          <div className="col-12 text-center">
            <input
              id={`input-${name}`}
              aria-describedby={`error-${name}-${idHint}`}
              {...rest}
              {...fieldProps}
              data-testid={testId}
            />
            {!!errors[name] && (
              <div
                id={`error-${name}-${idHint}`}
                className="form-message text-center"
              >
                <p className="quaternary">
                  {(errors[name]?.message ?? '') as string}
                </p>
              </div>
            )}
          </div>
        );
      }}
    />
  );
};

export default TextInput;
