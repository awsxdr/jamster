# CRG Shim

This folder contains files derived from the 
[CRG scoreboard project](https://github.com/rollerderby/scoreboard), used
under its MIT License.

## License

The CRG scoreboard files are licensed under the MIT License:

MIT License

Copyright (C) 2008-2012 Mr Temper <MrTemper@CarolinaRollergirls.com>
Copyright (C) 2012-2025 The CRG developers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Modifications

The following files have been modified from the originals (as of 2026-05-18):

- json/WS.js - Changed to use SignalR instead of pure web sockets

Unmodified files are included as-is from the CRG project.

## Bundled Third-Party Libraries

The `external/` folder contains libraries bundled with the original CRG project.
Each has its own license:

| Component | License | License file |
|-----------|---------|--------------|
| jQuery | MIT | *(in source file header)* |
| jQuery UI | MIT | `external/jquery-ui/LICENSE.txt` |
| jQuery File Upload | MIT | *(in source file header)* |
| jquery-qrcode | MIT | `external/jquery-qrcode/MIT-LICENSE.txt` |
| Font Awesome | MIT (CSS) / SIL OFL 1.1 (fonts) | *(see fontawesome.io)* |
| gamepad.js | UNLICENSE | *(in source file header)* |

The `fonts/` folder contains:

| Font | License | License file |
|------|---------|--------------|
| Liberation Sans | GPL v2 + font embedding exception | `fonts/LiberationSans-License.txt` |
| Inter | SIL OFL 1.1 | `fonts/inter/LICENSE.txt` |
