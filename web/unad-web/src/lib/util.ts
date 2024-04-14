export function sanitizePhoneNumber(phone: string) {
  return `+1${phone.trim().replace(/(\D{10})$/g, '')}`;
}

export function validatePhone(phone: string) {
  if (!phone.match(/^\+[1-9]\d{1,14}$/)) {
    throw new Error(`Invalid phone number "${phone}"`);
  }
}
