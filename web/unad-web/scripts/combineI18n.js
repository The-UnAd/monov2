const glob = require('glob');
const fs = require('fs');
const path = require('path');

// Get all files matching the pattern
glob('i18n-cache/**/*.i18n.*.json', (err, files) => {
  if (err) {
    console.error(err);
    return;
  }

  const map = files.reduce((acc, file) => {
    const [, ...keyParts] = file.split('/');
    const [fileKey] = keyParts.slice(-1)[0].split('.');
    const key = [...keyParts.slice(0, -1), fileKey].join('/');
    const [locale] = path.basename(file).split('.').slice(-2);

    acc[locale] = acc[locale] ?? {};
    const contents = fs.readFileSync(file, 'utf8');
    if (!contents) {
      return acc;
    }
    acc[locale][key] = JSON.parse(contents ?? '{}');
    return acc;
  }, {});

  console.log('Writing files...');
  for (const locale of Object.keys(map)) {
    fs.writeFileSync(
      `i18n/${locale}.json`,
      JSON.stringify(map[locale], null, 2)
    );
  }
  console.log('Done!');
});
