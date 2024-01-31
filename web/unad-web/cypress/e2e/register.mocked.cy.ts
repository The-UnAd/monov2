import { createSha1Hash } from '../../src/lib/crypto';
const clientId = createSha1Hash('test');

describe('The Register Page, Mocked', () => {
  beforeEach(() => {
    cy.intercept(
      'POST',
      `/api/register/${Cypress.env('MAILOSAUR_SMS_NUMBER').substring(1)}`,
      {
        fixture: 'register.json',
      }
    ).as('register');
    cy.intercept('POST', '/api/register/validate', {
      fixture: 'validate.json',
    }).as('validate');
    cy.intercept('POST', '/api/register/pay', {
      statusCode: 302,
      headers: {
        location: `/pay/success?session_id=123&clientId=${clientId}`,
      },
    }).as('stripeRedirect');
    // TODO: I would have to mock stripe on the server side for this to work
    cy.intercept('GET', 'https://api.stripe.com/v1/checkout/sessions/**', {
      fixture: 'session.json',
    }).as('stripeSession');
  });

  it('should navigate to the pay page after registering', async () => {
    cy.visit('/register');

    cy.get('input[name="name"]').type('test');
    cy.get('input[name="phone"]').type(
      Cypress.env('MAILOSAUR_SMS_NUMBER').substring(1)
    );
    cy.get('button[type="submit"]').should('not.be.disabled');
    cy.get('button[type="submit"]').click();

    cy.get('input[name="otp"]').type('123456');
    cy.get('button[type="submit"]').should('not.be.disabled');
    cy.get('button[type="submit"]').click();

    cy.get('a[href*="pay"]').click();
    cy.url().should('include', '/pay');
    // cy.get('button[type="submit"]').click();
    // cy.url().should('include', '/success');
  });
});

// Prevent TypeScript from reading file as legacy script
export {};
