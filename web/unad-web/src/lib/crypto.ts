import { createHash } from 'crypto';

export function createSha1Hash(input: string): string {
  const sha1 = createHash('sha1');
  const hash = sha1.update(input).digest('hex');
  return hash;
}
