import MailosaurClient from 'mailosaur';

describe('The Register Page', () => {
  before(() => {
    // cy.exec('redis-server --daemonize yes');
    // cy.exec('npm run db:reset && npm run db:seed');
    cy.exec('npm run db:reset').its('code').should('eq', 0);
  });

  beforeEach(() => {
    // mocking stripe confirm payment intent endpoint
    cy.intercept('GET', 'https://checkout.stripe.com/c/pay/**', {
      statusCode: 302,
      headers: {
        location: `${Cypress.config().baseUrl}/pay/success`,
      },
    }).as('stripeRedirect');
  });

  it(
    'should navigate to the pay page after registering',
    {
      retries: {
        runMode: 0,
        openMode: 0,
      },
    },
    async () => {
      const testStart = new Date();

      cy.visit('/register');

      cy.get('input[name="name"]').type('test');
      cy.get('input[name="phone"]').type(
        Cypress.env('MAILOSAUR_SMS_NUMBER').substring(1)
      );
      cy.get('input[name="terms"]').check();
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
              const code = /(\d{6})/.exec(message.text.body)[0];
              cy.wrap($input).type(code);
              resolve();
            })
            .catch(reject);
        });
      });
      const beforeOtpSubmit = new Date();
      cy.get('button[type="submit"]').should('not.be.disabled');
      cy.get('button[type="submit"]').click();

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
        .should('contain', 'Welcome to UnAd');

      cy.get('a[href*="pay"]').click();
      cy.url().should('include', '/pay');

      // cy.get('button[type="submit"]').click();
      // cy.url({ timeout: 15000 }).should('include', 'checkout.stripe.com');
      // cy.get('#email').type(Cypress.env('MAILOSAUR_EMAIL'));
      // cy.get('#cardNumber').type('4242424242424242');
      // cy.get('#cardExpiry').type('1225');
      // cy.get('#cardCvc').type('123');
      // cy.get('#billingName').type('Bob Barker');
      // cy.get('#billingPostalCode').type('32210');
      // cy.get('#enableStripePass').click();
      // cy.get('button[type="submit"]').click();

      // cy.url().should('include', '/success');
    }
  );
});

// Prevent TypeScript from reading file as legacy script
export {};
