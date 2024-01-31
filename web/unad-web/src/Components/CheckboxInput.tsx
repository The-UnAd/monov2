import React, { ReactNode, useId } from 'react';
import { Controller, useFormContext } from 'react-hook-form';

type CheckboxInputProps = React.InputHTMLAttributes<HTMLInputElement> & {
  name: string;
  labelText:
    | string
    | JSX.Element
    | (string | JSX.Element)[]
    | Iterable<ReactNode>;
  'data-testid'?: string;
};

const CheckboxInput = (props: CheckboxInputProps) => {
  const {
    name,
    defaultValue,
    'data-testid': testId,
    labelText,
    ...rest
  } = props;
  const {
    control,
    formState: { errors },
  } = useFormContext();
  const errorIdHint = useId();
  return (
    <Controller
      name={name}
      control={control}
      defaultValue={defaultValue ?? ''}
      render={({ field }) => {
        const { ref, ...fieldProps } = field;
        return (
          <>
            <input
              id={`input-${name}`}
              aria-describedby={`error-${name}-${errorIdHint}`}
              {...rest}
              {...fieldProps}
              data-testid={testId}
            />
            <p>{labelText}</p>\
            {!!errors[name] && (
              <div
                id={`error-${name}-${errorIdHint}`}
                className="form-message text-center"
              >
                <p className="quaternary">
                  {(errors[name]?.message ?? '') as string}
                </p>
              </div>
            )}
          </>
        );
      }}
    />
  );
};

export default CheckboxInput;
