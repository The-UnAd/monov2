import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useTranslations } from 'next-intl';

import SubscribeForm, { SubscribeFormProps } from './SubscribeForm';

const WrappedWithIntl = (props: Omit<SubscribeFormProps, 'tFunc'>) => {
  const t = useTranslations('Components/SubscribeForm');
  return <SubscribeForm {...props} tFunc={t} />;
};

const Sut = withIntl(WrappedWithIntl);

describe('SubscribeForm', () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  const setup = (props: Omit<SubscribeFormProps, 'tFunc'>) => {
    const utils = render(<Sut {...props} />);
    const phone = screen.getByTestId('SubscribeForm__phone');
    const submit = screen.getByTestId('SubscribeForm__submit');
    const terms = screen.getByTestId('SubscribeForm__terms');
    return {
      phone,
      submit,
      terms,
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders in a container', () => {
    const onSubmit = jest.fn();

    setup({ onSubmit });

    const input = screen.getByTestId('SubscribeForm__phone');

    expect(input).toBeInTheDocument();
  });

  it('submit stays disabled when inputs are invalid', async () => {
    const clientPhone = '123123132';
    const onSubmit = jest.fn();

    const { phone, submit, user } = setup({ onSubmit });

    await user.type(phone, clientPhone);

    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeDisabled();
  });

  it('calls success on valid inputs', async () => {
    const clientPhone = '1234567890';

    const onSubmit = jest.fn();

    const { phone, submit, terms, user } = setup({ onSubmit });

    await user.type(phone, clientPhone);
    await user.click(terms);

    expect(phone).toHaveValue(clientPhone);
    expect(submit).toBeEnabled();

    await user.click(submit);

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
