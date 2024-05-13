# Signup Site (`web/unad-web`)

The signup site is hosted in Production at [https://signup.unad.tech](https://signup.unad.tech).

## Process Flows

The basic signup flow is as follows:

```d2

Client {
  shape: person
}

Signup Site: {
  Register: {
    shape: page

    link: '#otp flow'
  }
  Select Plan: {
    shape: page
  }
  Success: {
    shape: page
  }
}

Twilio: {
  shape: cloud
}

Payment Processor: {
  shape: cloud
  Payment: {
    shape: page
  }
}

# Page Flow
Signup Site.Register -> Signup Site.Select Plan
Signup SIte.Register -> Twilio: Send OTP
Twilio -> Client: Send OTP
Signup Site.Select Plan -> Payment Processor.Payment: Redirect
Payment Processor.Payment -> Signup Site.Success: Redirect

# Interactions
Client -> Signup Site.Register: Enter Phone Number
Client -> Payment Processor.Payment: Enter Payment Details

```

### OTP Flow

```d2
User Flow: {
  shape: sequence_diagram
  Client -> Register Page: Enter Phone Number
  Register Page -> Client: Send OTP
  Client -> Register Page: Enter OTP
  Register Page -> Plans Page: OTP Verified
}

Technical Version: {
  shape: sequence_diagram
  Client -> Register Page: Enter Phone Number
  Register Page -> Server: Request OTP
  Server -> Register Page: Return OTP
  Register Page -> Twilio: Send OTP
  Twilio -> Client: Relay OTP
  Client -> Register Page: Enter OTP
  Register Page -> Server: Verify OTP
  Server -> Register Page: OTP Verified
  Register Page -> Plans Page: Route
}
```
