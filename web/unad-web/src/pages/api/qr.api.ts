import type { NextApiRequest, NextApiResponse } from 'next';
import QRCode from 'qrcode';
import { PassThrough } from 'stream';

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  const content = req.query.content;
  try {
    const qrStream = new PassThrough();
    await QRCode.toFileStream(qrStream, content as string, {
      type: 'png',
      width: req.query.size ? Number(req.query.size) : 250,
      errorCorrectionLevel: 'H',
    });

    qrStream.pipe(res);
  } catch (err: any) {
    res.status(500).json({ message: err.message });
  }
}
