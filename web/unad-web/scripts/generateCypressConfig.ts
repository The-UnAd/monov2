import { readFileSync, writeFileSync } from 'fs';
import { EOL } from 'os';
import { resolve } from 'path';

const envConfigFile = resolve(__dirname, '../.env.test.cy');
const targetConfigFile = resolve(__dirname, '../cypress.env.json');

function processLineByLine(src: string, dest: string) {
  const fileStream = readFileSync(src);

  const lines = fileStream.toString().split(EOL);

  const kvPairs = lines
    .map((line) => {
      const [key, value] = line.split('=');
      if (!key || !value) {
        return [null, null];
      }
      const realValue = value.replace(/"/g, '');
      return [key, realValue];
    })
    .filter(([k, v]) => k && v)
    .reduce(
      (acc, [key, value]) => ({
        ...acc,
        [key as string]: value as string,
      }),
      {} as Record<string, string>
    );
  console.log('kvPairs', kvPairs);
  writeFileSync(dest, JSON.stringify(kvPairs, null, 2));
}

processLineByLine(envConfigFile, targetConfigFile);
