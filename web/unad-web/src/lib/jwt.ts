import jwt from 'jsonwebtoken';

export function signJwt(payload: string | object | Buffer): Promise<string> {
  return new Promise((resolve, reject) => {
    jwt.sign(
      payload,
      process.env.JWT_PRIVATE_KEY as jwt.Secret,
      { algorithm: 'RS256' },
      (err, token) => {
        if (err || !token) {
          return reject(err ?? 'Error signing JWT.  No token returned.');
        }
        return resolve(token);
      }
    );
  });
}

/**
 * Verify a JWT with the private key
 * @param token JWT in string format
 * @returns the decoded JWT payload
 */
export function verifyJwt(token: string): Promise<jwt.JwtPayload> {
  return new Promise((resolve, reject) => {
    jwt.verify(
      token,
      process.env.JWT_PRIVATE_KEY as jwt.Secret,
      (err, decoded) => {
        if (err) {
          return reject(err);
        }
        return resolve(decoded as jwt.JwtPayload);
      }
    );
  });
}

/**
 * Decode a JWT with the public key
 * @param token JWT in string format
 * @returns The decoded JWT payload
 */
export function decodeJwt(token: string): Promise<jwt.JwtPayload> {
  return new Promise((resolve, reject) => {
    jwt.verify(
      token,
      process.env.NEXT_PUBLIC_JWT_PUBLIC_KEY as jwt.Secret,
      (err, decoded) => {
        if (err) {
          return reject(err);
        }
        return resolve(decoded as jwt.JwtPayload);
      }
    );
  });
}
