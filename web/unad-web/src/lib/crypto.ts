import { createHash } from 'crypto';

export function createSha1Hash(input: string): string {
  const sha1 = createHash('sha1');
  const hash = sha1.update(input).digest('hex');
  return hash;
}

// Thanks, ChatGPT!
export function generateSlug(phoneNumber: string) {
  // Convert phone number to bytes
  const phoneBuffer = Buffer.from(phoneNumber, 'utf-8');

  // Generate SHA-256 hash
  const hash = createHash('sha256');
  const hashBytes = hash.update(phoneBuffer).digest();

  // Take the first 12 characters of the hash
  const hashHex = hashBytes.toString('hex').substring(0, 12);

  // Encode the hash to base64
  const slugBuffer = Buffer.from(hashHex, 'hex');
  const slug = slugBuffer.toString('base64url');

  return slug;
}
