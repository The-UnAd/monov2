import MailosaurClient from 'mailosaur';

describe('The Account Page', () => {
  before(() => {
    cy.exec('npm run db:reset && npm run db:seed').its('code').should('eq', 0);
  });

  it('successfully loads', () => {
    cy.visit('/account');
  });

  it('should show the login page', () => {
    cy.visit('/account');

    cy.get('[data-testid="Login__heading"').contains('Login');
  });

  it(
    'logs in successfully',
    {
      retries: {
        runMode: 0,
        openMode: 0,
      },
    },
    async () => {
      const testStart = new Date();
      cy.visit('/account');

      cy.get('input[name="phone"]').type(
        Cypress.env('MAILOSAUR_SMS_NUMBER').substring(1)
      );
      cy.get('button[type="submit"]').should('not.be.disabled');
      cy.get('button[type="submit"]').click();

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
              const [code] = /(\d{6})/.exec(message.text.body);
              cy.wrap($input).type(code);
              resolve();
            })
            .catch(reject);
        });
      });

      cy.get('button[type="submit"]').should('not.be.disabled');
      cy.get('button[type="submit"]').click();

      cy.url().should('include', 'account/');
      cy.get('[data-testid="Account__heading"').contains('Account');
    }
  );
});

// Prevent TypeScript from reading file as legacy script
export {};
