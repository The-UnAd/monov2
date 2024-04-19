# User Flow

## Sign Up

1. hit `/register` page
    - enter name and phone
    - verify phone
    - `/api/validate` call creates `Client` record
2. redirected to `/pay/[clientId]` page
    - choose a price tier
3. redirected to `/pay/[clientId]/[priceTierId]` page
    - redirects to payment processor
    - payment accepted
    - sends message to `subscriptions` Kafka topic with JSON serialized `clientId`, `priceTierId`, and `paymentConfirmationId`
