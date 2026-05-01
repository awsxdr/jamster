const zip = require('cross-zip');
const path = require('path');
const fs = require('fs');

const distFolder = path.join(__dirname, '..', 'dist');
const tempFolder = path.join(__dirname, 'tmp');

try {
    fs.statSync(tempFolder);
    fs.rmSync(tempFolder, { recursive: true, force: true });
} catch { 

}

const outFile = path.join(tempFolder, 'build.zip');

fs.mkdirSync(tempFolder);

zip.zip(distFolder, outFile, () => {
    fs.rmSync(distFolder, { recursive: true, force: true });
    fs.mkdirSync(distFolder);

    fs.copyFileSync(outFile, path.join(distFolder, 'test.1.0.0.zip'));
    fs.rmSync(tempFolder, { recursive: true, force: true });
});

