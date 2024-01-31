import MailosaurClient from 'mailosaur';

import { Client } from '../../src/models/Client';
const name = 'seed';
const clientId = Client.hashId(name);

describe('The Subscribe Page', () => {
  before(() => {
    cy.exec('npm run db:reset && npm run db:seed').its('code').should('eq', 0);
  });

  it(
    'should show success after OTP verification',
    {
      retries: {
        runMode: 0,
        openMode: 0,
      },
    },
    async () => {
      const testStart = new Date();

      cy.visit(`/subscribe/${clientId}`);

      cy.get('input[name="phone"]').type(
        Cypress.env('MAILOSAUR_SMS_NUMBER').substring(1)
      );
      cy.get('button[data-testid="Subscribe__subscribe"]').should(
        'not.be.disabled'
      );
      cy.get('button[data-testid="Subscribe__subscribe"]').click();

      const mailosaur = new MailosaurClient(Cypress.env('MAILOSAUR_API_KEY'));

      cy.get('input[name="otp"]').then(($input) => {
        return new Cypress.Promise((resolve, reject) => {
          mailosaur.messages
            .get(
              Cypress.env('MAILOSAUR_SERVER_ID'),
              {
                sentTo: Cypress.env('MAILOSAUR_SMS_NUMBER'),
              },
              {
                receivedAfter: testStart,
              }
            )
            .then((message) => {
              const code = /(\d{6})/.exec(message.text.body)[0];
              cy.wrap($input).type(code);
              resolve();
            })
            .catch(reject);
        });
      });
      const beforeOtpSubmit = new Date();
      cy.get('button[data-testid="Subscribe__validate"]').should(
        'not.be.disabled'
      );
      cy.get('button[data-testid="Subscribe__validate"]').click();

      cy.wait(2000)
        .then(() => {
          return new Cypress.Promise((resolve, reject) => {
            mailosaur.messages
              .get(
                Cypress.env('MAILOSAUR_SERVER_ID'),
                {
                  sentTo: Cypress.env('MAILOSAUR_SMS_NUMBER'),
                },
                {
                  receivedAfter: beforeOtpSubmit,
                }
              )
              .then((message) => {
                resolve(message.text.body);
              })
              .catch(reject);
          });
        })
        .should('contain', 'You will now receive announcements');
      cy.get('p[data-testid="Subscribe__success"]').should(
        'have.text',
        'Success!'
      );
    }
  );
});

// Prevent TypeScript from reading file as legacy script
export {};
