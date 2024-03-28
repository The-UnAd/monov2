import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import fetchMock from 'jest-fetch-mock';
import mockRouter from 'next-router-mock';

import Register from './register.page';

jest.mock('next/router', () => jest.requireActual('next-router-mock'));

const Sut = withIntl(Register);

const clientPhone = '1234567890';
const clientName = 'abc';
const clientId = 'test';

describe('Register', () => {
  beforeAll(() => {
    fetchMock.enableMocks();
  });
  afterEach(() => {
    jest.restoreAllMocks();
    fetchMock.resetMocks();
  });
  afterAll(() => {
    fetchMock.disableMocks();
  });

  const setup = () => {
    const utils = render(<Sut />);
    const name = screen.getByTestId('RegisterForm__name');
    const phone = screen.getByTestId('RegisterForm__phone');
    const terms = screen.getByTestId('RegisterForm__terms');

    const registerSubmit = screen.getByTestId('RegisterForm__submit');
    return {
      name,
      phone,
      terms,
      registerSubmit,
      user: userEvent.setup(),
      getErrorElement() {
        return screen.getByTestId('Register__error');
      },
      ...utils,
    };
  };

  it('renders only Register form initially', () => {
    render(<Sut />);

    const registerForm = screen.getByTestId('RegisterForm__container');
    const otpForm = screen.queryByRole('input', { name: 'otp' });
    expect(registerForm).toBeInTheDocument();
    expect(otpForm).not.toBeInTheDocument();
  });

  it('renders OTP form after successful registration', async () => {
    const registerData = { name: clientName, phone: clientPhone };
    fetchMock.mockIf(
      `/api/register/${encodeURIComponent(clientPhone)}`,
      JSON.stringify(registerData)
    );

    const { name, phone, registerSubmit, user, terms } = setup();

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();
    expect(registerSubmit).toBeEnabled();

    await user.click(registerSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const otp = screen.getByTestId('OtpForm__otp');
    expect(otp).toBeInTheDocument();
  });

  it('renders success message after successful OTP entry', async () => {
    const registerData = { name: clientName, phone: clientPhone };
    fetchMock.mockIf(
      `/api/register/${encodeURIComponent(clientPhone)}`,
      JSON.stringify(registerData)
    );

    const { name, phone, registerSubmit, user, terms } = setup();

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();

    expect(registerSubmit).toBeEnabled();

    await user.click(registerSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const otp = screen.getByTestId('OtpForm__otp');
    const otpSubmit = screen.getByTestId('OtpForm__submit');
    expect(otp).toBeInTheDocument();
    await user.type(otp, '123123');

    expect(otp).toHaveValue('123123');
    expect(otpSubmit).toBeEnabled();

    fetchMock.mockIf('/api/register/validate', JSON.stringify({ clientId }));

    await user.click(otpSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(2);

    expect(mockRouter).toMatchObject({
      pathname: `/pay/${clientId}`,
    });
  });

  it('renders error message after failed registration', async () => {
    fetchMock.mockIf(
      `/api/register/${encodeURIComponent(clientPhone)}`,
      JSON.stringify({ message: 'test' }),
      {
        status: 500,
      }
    );

    const { name, phone, registerSubmit, user, terms, getErrorElement } =
      setup();

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();
    expect(registerSubmit).toBeEnabled();

    await user.click(registerSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const error = getErrorElement();
    expect(error).toBeInTheDocument();
  });

  it('renders error message after invalid OTP entry', async () => {
    const registerData = { name: clientName, phone: clientPhone };
    fetchMock.mockIf(
      `/api/register/${encodeURIComponent(clientPhone)}`,
      JSON.stringify(registerData)
    );
    const { name, phone, registerSubmit, user, terms, getErrorElement } =
      setup();

    await user.type(name, clientName);
    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(name).toHaveValue(clientName);
    expect(phone).toHaveValue(clientPhone);
    expect(terms).toBeChecked();

    expect(registerSubmit).toBeEnabled();

    await user.click(registerSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const otp = screen.getByTestId('OtpForm__otp');
    const otpSubmit = screen.getByTestId('OtpForm__submit');
    expect(otp).toBeInTheDocument();
    await user.type(otp, '123123');

    expect(otp).toHaveValue('123123');
    expect(otpSubmit).toBeEnabled();

    fetchMock.mockIf(
      '/api/register/validate',
      JSON.stringify({ message: 'test' }),
      {
        status: 500,
      }
    );

    await user.click(otpSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(2);

    const error = getErrorElement();
    expect(error).toBeInTheDocument();
  });
});
