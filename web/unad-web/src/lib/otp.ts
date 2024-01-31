import speakeasy from 'speakeasy';
const step = Number(process.env.OTP_STEP);
const window = Number(process.env.OTP_WINDOW);

export function validateToken(token: string, secret: string) {
  const result = speakeasy.totp.verify({
    encoding: 'base32',
    secret,
    token,
    step,
    window,
  });
  return result;
}

export function generateSecret() {
  return speakeasy.generateSecret({ length: 20 }).base32;
}

export function generateToken(secret: string) {
  return speakeasy.totp({
    encoding: 'base32',
    secret,
    step,
  });
}
