import type { JwtPayload } from 'jsonwebtoken';

import { TokenResponse } from '@/pages/api/login.api';

export async function subscribeToClient(
  phone: string,
  clientId: string
): Promise<void> {
  const resp = await fetch('/api/subscribe', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      phone,
      clientId,
    }),
  });
  if (!resp.ok) {
    const error = await resp.json();
    throw new Error(error.message);
  }
}

export async function validateOtp(
  otp: string,
  phone: string,
  clientId: string
): Promise<boolean> {
  const resp = await fetch('/api/validate', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      otp,
      phone,
      clientId,
    }),
  });

  if (!resp.ok) {
    const error = await resp.json();
    throw new Error(error.message);
  }
  const { code } = await resp.json();
  return code;
}

export async function generateOtp(phone: string): Promise<void> {
  const resp = await fetch('/api/otp', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      phone,
    }),
  });
  if (!resp.ok) {
    const error = await resp.json();
    throw new Error(error.message);
  }
}

export async function login(
  phone: string,
  otp: string
): Promise<TokenResponse> {
  const resp = await fetch('/api/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      phone,
      otp,
    }),
  });

  if (!resp.ok) {
    const error = await resp.json();
    throw new Error(error.message);
  }
  return resp.json();
}

export async function validateJwt(token: string): Promise<JwtPayload> {
  const resp = await fetch('/api/jwt', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      token,
    }),
  });

  if (!resp.ok) {
    const error = await resp.json();
    throw new Error(error.message);
  }
  const payload = await resp.json();
  debugger;
  return payload;
}
