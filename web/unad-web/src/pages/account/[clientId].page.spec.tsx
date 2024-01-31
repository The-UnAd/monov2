import { withIntl } from '@t/util';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import fetchMock from 'jest-fetch-mock';

import Account, { AccountProps } from './[clientId].page';
jest.mock('next/router', () => require('next-router-mock'));

const Sut = withIntl(Account);

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

  const setup = (props: AccountProps) => {
    const utils = render(<Sut {...props} />);
    return {
      user: userEvent.setup(),
      ...utils,
    };
  };

  it('renders only Phone input initially', () => {
    setup({
      name: 'test',
      subscribers: 0,
      announcementCount: 0,
      portalUrl: 'https://example.com',
      subscribeUrl: 'https://example.com',
    });

    const container = screen.getByTestId('Account__container');
    expect(container).toBeInTheDocument();
  });
});
