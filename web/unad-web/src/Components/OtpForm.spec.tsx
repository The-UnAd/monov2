import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import fetchMock from 'jest-fetch-mock';

import OtpForm, { OtpFormProps } from './OtpForm';

const Sut = withIntl(OtpForm);

describe('OtpForm', () => {
  beforeAll(() => {
    fetchMock.enableMocks();
  });
  afterEach(() => {
    jest.restoreAllMocks();
  });
  afterAll(() => {
    fetchMock.disableMocks();
  });

  const setup = (props: Omit<OtpFormProps, 'tFunc'>) => {
    const utils = render(<Sut {...props} tFunc={(k) => k} />);
    const otp = screen.getByTestId('OtpForm__otp');
    const submit = screen.getByTestId('OtpForm__submit');
    return {
      otp,
      submit,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders in a container', () => {
    const onSubmit = jest.fn();

    setup({ onSubmit });

    const container = screen.getByTestId('OtpForm__container');

    expect(container).toBeInTheDocument();
  });

  it('submit stays disabled when inputs are invalid', async () => {
    const onSubmit = jest.fn();
    const { otp, submit, user } = setup({ onSubmit });

    await user.type(otp, '12312');

    expect(submit).toBeDisabled();
  });

  it('calls onSuccess on valid inputs', async () => {
    const onSubmit = jest.fn();

    const clientData = { name: 'test', clientId: 'test' };
    fetchMock.mockOnce(JSON.stringify(clientData));
    const { otp, submit, user } = setup({ onSubmit });

    await user.type(otp, '123123');

    expect(otp).toHaveValue('123123');
    expect(submit).toBeEnabled();

    await user.click(submit);

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
