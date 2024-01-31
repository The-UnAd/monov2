import type { NextApiRequest, NextApiResponse } from 'next';
import Stripe from 'stripe';

import { createModelFactory } from '@/lib/redis';

interface JoinApiRequest extends NextApiRequest {
  body: {
    customerId: string;
    clientId: string;
  };
}

export default async function handler(
  req: JoinApiRequest,
  res: NextApiResponse
) {
  const { clientId } = req.body;
  try {
    using models = createModelFactory();
    await models.connect();
    const stripe = new Stripe(process.env.STRIPE_API_KEY as string, {
      apiVersion: '2023-10-16',
    });
    const client = await models.getClientById(clientId);
    const customerId = await client?.getStripeCustomerId();
    const session = await stripe.checkout.sessions.create({
      mode: 'subscription',
      customer: customerId,
      line_items: [
        {
          price: process.env.STRIPE_PRODUCT_PRICE_ID,
          quantity: 1,
        },
      ],
      payment_method_collection: 'if_required',
      success_url: `${process.env.SITE_HOST}/pay/success?clientId=${clientId}&session_id={CHECKOUT_SESSION_ID}`,
      cancel_url: `${process.env.SITE_HOST}/pay/canceled`,
    });

    return res.redirect(303, session.url as string);
  } catch (error: any) {
    console.error(error);
    return res.status(500).json({ message: error.message });
  }
}
