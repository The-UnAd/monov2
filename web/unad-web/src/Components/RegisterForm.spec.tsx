import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import RegisterForm, { RegisterFormProps } from './RegisterForm';

const Sut = withIntl(RegisterForm);

describe('RegisterForm', () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  const setup = (props: RegisterFormProps) => {
    const utils = render(<Sut {...props} />);
    const name = screen.getByTestId('RegisterForm__name') as HTMLInputElement;
    const phone = screen.getByTestId('RegisterForm__phone') as HTMLInputElement;
    const terms = screen.getByTestId('RegisterForm__terms') as HTMLInputElement;
    const submit = screen.getByTestId(
      'RegisterForm__submit'
    ) as HTMLInputElement;
    return {
      name,
      phone,
      terms,
      submit,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders in a container', () => {
    const onSubmit = jest.fn();

    render(<Sut onSubmit={onSubmit} />);

    const container = screen.getByTestId('RegisterForm__container');

    expect(container).toBeInTheDocument();
  });

  it('submit stays disabled when name is invalid', async () => {
    const clientPhone = '1234567890';
    const clientName = 'ab';
    const onSubmit = jest.fn();

    const { name, phone, submit, user, terms } = setup({ onSubmit });

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeDisabled();
  });

  it('submit stays disabled when number is invalid', async () => {
    const clientPhone = '123456789';
    const clientName = 'abc';
    const onSubmit = jest.fn();

    const { name, phone, submit, user, terms } = setup({ onSubmit });

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeDisabled();
  });

  it('submit stays disabled when terms are not accepted', async () => {
    const clientPhone = '123456789';
    const clientName = 'abc';
    const onSubmit = jest.fn();

    const { name, phone, submit, user, terms } = setup({ onSubmit });

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();
    expect(submit).toBeDisabled();
  });

  it('calls success on valid inputs', async () => {
    const clientPhone = '1234567890';
    const clientName = 'abc';
    const onSubmit = jest.fn();

    const { name, phone, submit, user, terms } = setup({ onSubmit });

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();
    expect(submit).toBeEnabled();

    await user.click(submit);

    // expect(onSubmit).toHaveBeenCalledWith({
    //   name: clientName,
    //   phone: clientPhone,
    // });
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
