# UnAd Infrastructure

The 30,000ft view of the UnAd infrastructure is quite simple.  The two main processes, [unad-functions](./components/unad-functions.md), and the [Signup Site](./components/signup-site.md) are deployed as Docker Containers to ECS.  These containers utilize an [ElastiCache] (Redis) instance, and an [RDS](#rds) database cluster as the primary operational source of truth.  

We rely on two third-party services, [Twilio](#twilio) and [Stripe](#stripe).  

```d2
--8<-- "diagrams/30000ft.d2"
```

## Twilio

Arguably the most critical service we rely on day-to-day is [Twilio](https://www.twilio.com/en-us).  It not only allows the sending an receiving of SMS messages, but even more crucially, the ability to call [webhooks](./components/unad-functions.md) to handle received SMS messages.

For more details, see [here](./integrations/twilio.md)

## Stripe

[Stripe](https://stripe.com/) is our payment processor, source of truth for the product catalog, and another source of mission-critical [webhooks](./components/unad-functions.md).

For more details, see [here](./integrations/stripe.md).

## ElastiCache

[Amazon ElastiCache for Redis](https://aws.amazon.com/elasticache/redis/) serves as an easy and reliable data source for ephemeral data such a such as user sessions.

## RDS

The main operational database [`userdb`](./database/userdb.md), is implemented as a PostgreSQL database on [Amazon Aurora Serverless V2](https://aws.amazon.com/rds/aurora/serverless/).
