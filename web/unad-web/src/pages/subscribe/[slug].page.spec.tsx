import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import fetchMock from 'jest-fetch-mock';
import mockRouter from 'next-router-mock';

import Subscribe from './[slug].page';

jest.mock('next/router', () => jest.requireActual('next-router-mock'));

const DefaultOtp = '123123';
const DefaultPhoneNumber = '1234567890';
const FormattedPhoneNumber = `+1${DefaultPhoneNumber}`;
const DefaultClientId = 'test';
const DefaultName = 'test';

const Sut = withIntl(Subscribe);

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
    const utils = render(<Sut clientId={DefaultClientId} name={DefaultName} />);
    const phone = screen.getByTestId('SubscribeForm__phone');
    const phoneSubmit = screen.getByTestId('SubscribeForm__submit');
    const terms = screen.getByTestId('SubscribeForm__terms');
    return {
      phone,
      phoneSubmit,
      terms,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders only Phone input initially', () => {
    render(<Sut clientId={DefaultClientId} name={DefaultName} />);

    const container = screen.getByTestId('Subscribe__container');
    const otpForm = screen.queryByRole('input', { name: 'otp' });
    expect(container).toBeInTheDocument();
    expect(otpForm).not.toBeInTheDocument();
  });

  it('renders OTP form after successful registration', async () => {
    fetchMock.mockOnce('/api/otp', {
      status: 200,
    });

    const { phone, phoneSubmit, terms, user } = setup();

    await user.type(phone, DefaultPhoneNumber);
    await user.click(terms);

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
    const validateRequestBody = {
      otp: DefaultOtp,
      phone: FormattedPhoneNumber,
      clientId: DefaultClientId,
    };
    const validateResponse = {
      code: 'test',
    };
    fetchMock.mockOnce(JSON.stringify(validateResponse), {
      url: '/api/validate',
      status: 200,
    });
    fetchMock.mockOnce('/api/subscribe', {
      status: 200,
    });

    const { phone, phoneSubmit, terms, user } = setup();

    await user.type(phone, DefaultPhoneNumber);
    await user.click(terms);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

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

    expect(fetchMock).toHaveBeenCalledWith('/api/validate', {
      body: JSON.stringify(validateRequestBody),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    expect(fetchMock).toHaveBeenCalledWith('/api/subscribe', {
      body: JSON.stringify({
        phone: FormattedPhoneNumber,
        clientId: 'test',
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    expect(mockRouter).toMatchObject({
      asPath: `/success/${validateResponse.code}`,
    });
  });

  it('renders error message after failed registration', async () => {
    fetchMock.mockOnce(JSON.stringify({ message: 'error' }), {
      status: 500,
      url: '/api/otp',
    });

    const { phone, phoneSubmit, terms, user } = setup();

    await user.type(phone, DefaultPhoneNumber);
    await user.click(terms);

    expect(phone).toHaveValue(DefaultPhoneNumber);
    expect(phoneSubmit).toBeEnabled();

    await user.click(phoneSubmit);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const error = screen.getByTestId('Subscribe__error');
    expect(error).toHaveTextContent('error');
  });

  it('renders error message after failed OTP entry', async () => {
    fetchMock.mockOnce('/api/otp', {
      status: 200,
    });
    fetchMock.mockOnce(JSON.stringify({ message: 'error' }), {
      status: 500,
      url: '/api/validate',
    });

    const { phone, phoneSubmit, terms, user } = setup();

    await user.type(phone, DefaultPhoneNumber);
    await user.click(terms);

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
    expect(fetchMock).toHaveBeenCalledWith('/api/validate', {
      body: JSON.stringify({
        otp: DefaultOtp,
        phone: FormattedPhoneNumber,
        clientId: DefaultClientId,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
    });

    const error = screen.getByTestId('Subscribe__error');
    expect(error).toHaveTextContent('error');
  });
});
