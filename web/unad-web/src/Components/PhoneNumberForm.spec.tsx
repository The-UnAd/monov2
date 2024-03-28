import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import PhoneNumberForm, { PhoneNumberFormProps } from './PhoneNumberForm';

const Sut = withIntl(PhoneNumberForm);

describe('PhoneNumberForm', () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  const setup = (props: PhoneNumberFormProps) => {
    const utils = render(<Sut {...props} />);
    const phone = screen.getByTestId('PhoneNumberForm__phone');
    const submit = screen.getByTestId('PhoneNumberForm__submit');
    return {
      phone,
      submit,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders in a container', () => {
    const onSubmit = jest.fn();

    render(<Sut onSubmit={onSubmit} />);

    const input = screen.getByTestId('PhoneNumberForm__phone');

    expect(input).toBeInTheDocument();
  });

  it('submit stays disabled when inputs are invalid', async () => {
    const clientPhone = '123456789';
    const onSubmit = jest.fn();

    const { phone, submit, user } = setup({ onSubmit });

    await user.type(phone, clientPhone);

    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeDisabled();
  });

  it('calls success on valid inputs', async () => {
    const clientPhone = '1234567890';

    const onSubmit = jest.fn();

    const { phone, submit, user } = setup({ onSubmit });

    await user.type(phone, clientPhone);

    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeEnabled();

    await user.click(submit);

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
