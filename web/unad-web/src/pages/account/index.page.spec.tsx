import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import fetchMock from 'jest-fetch-mock';

import Login from './index.page';
jest.mock('next/router', () => require('next-router-mock'));

const DefaultOtp = '123123';
const DefaultPhoneNumber = '1234567890';
const FormattedPhoneNumber = `+1${DefaultPhoneNumber}`;

const Sut = withIntl(Login);

describe('Subscribe', () => {
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
    const phone = screen.getByTestId('PhoneNumberForm__phone');
    const phoneSubmit = screen.getByTestId('PhoneNumberForm__submit');
    return {
      phone,
      phoneSubmit,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders only Phone input initially', () => {
    render(<Sut />);

    const container = screen.getByTestId('Login__container');
    const otpForm = screen.queryByRole('input', { name: 'otp' });
    expect(container).toBeInTheDocument();
    expect(otpForm).not.toBeInTheDocument();
  });

  it('renders OTP form after successful registration', async () => {
    fetchMock.mockOnce('/api/otp', {
      status: 200,
    });

    const { phone, phoneSubmit, user } = setup();

    await user.type(phone, DefaultPhoneNumber);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const otp = screen.getByTestId('OtpForm__otp');
    expect(otp).toBeInTheDocument();
  });

  it('renders success message after successful OTP entry', async () => {
    fetchMock.mockOnce('/api/otp', {
      status: 200,
    });

    const tokenBody = JSON.stringify({
      token: 'test',
    });
    fetchMock.mockOnce(tokenBody, {
      url: '/api/login',
      status: 200,
    });
    fetchMock.mockOnce(
      JSON.stringify({
        sub: 'test',
      }),
      {
        status: 200,
        url: '/api/jwt',
      }
    );

    const { phone, phoneSubmit, user } = setup();

    await user.type(phone, DefaultPhoneNumber);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock).toHaveBeenCalledWith('/api/otp', {
      body: JSON.stringify({
        phone: FormattedPhoneNumber,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });
    const otp = screen.getByTestId('OtpForm__otp');
    expect(otp).toBeInTheDocument();
    const otpSubmit = screen.getByTestId('OtpForm__submit');
    expect(otp).toBeInTheDocument();
    await user.type(otp, DefaultOtp);

    expect(otp).toHaveValue(DefaultOtp);
    expect(otpSubmit).toBeEnabled();

    await user.click(otpSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(fetchMock).toHaveBeenCalledWith('/api/login', {
      body: JSON.stringify({
        phone: FormattedPhoneNumber,
        otp: DefaultOtp,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    expect(fetchMock).toHaveBeenCalledWith('/api/jwt', {
      body: JSON.stringify({
        token: 'test',
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });
  });

  it('renders error message after failed registration', async () => {
    fetchMock.mockOnce(JSON.stringify({ message: 'error' }), {
      status: 500,
      url: '/api/otp',
    });

    const { phone, phoneSubmit, user } = setup();

    await user.type(phone, DefaultPhoneNumber);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const error = screen.getByTestId('Login__error');
    expect(error).toHaveTextContent('error');
  });

  it('renders error message after failed OTP entry', async () => {
    fetchMock.mockOnce('/api/otp', {
      status: 200,
    });
    fetchMock.mockOnce(JSON.stringify({ message: 'error' }), {
      status: 500,
      url: '/api/login',
    });

    const { phone, phoneSubmit, user } = setup();

    await user.type(phone, DefaultPhoneNumber);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock).toHaveBeenCalledWith('/api/otp', {
      body: JSON.stringify({
        phone: FormattedPhoneNumber,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });
    const otp = screen.getByTestId('OtpForm__otp');
    expect(otp).toBeInTheDocument();
    const otpSubmit = screen.getByTestId('OtpForm__submit');
    expect(otp).toBeInTheDocument();
    await user.type(otp, DefaultOtp);

    expect(otp).toHaveValue(DefaultOtp);
    expect(otpSubmit).toBeEnabled();

    await user.click(otpSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock).toHaveBeenCalledWith('/api/login', {
      body: JSON.stringify({
        phone: FormattedPhoneNumber,
        otp: DefaultOtp,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    const error = screen.getByTestId('Login__error');
    expect(error).toHaveTextContent('error');
  });
});
