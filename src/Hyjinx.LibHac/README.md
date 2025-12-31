# LibHac
LibHac is a .NET library that reimplements some parts of the Nintendo Switch operating system, also known as Horizon OS.

One of the other main functions of the library is opening and extracting common content file formats used by Horizon.

Most content is imported and exported using a standard [IStorage](Fs/IStorage2.cs) interface. This means that reading content bodies can easily be done by chaining different storage implementations together.

## Disclaimer
This project does **not** contain or distribute any code, keys, or mechanisms to decrypt protected or encrypted content. It is provided **solely** for lawful research, interoperability, or format support purposes as permitted under applicable law, including but not limited to 17 U.S.C. § 1201(f).

Users are solely responsible for ensuring that their use of this library complies with all relevant laws and regulations, including those related to access controls, digital rights management (DRM), and copyright. 

If any encrypted files are detected, an exception will be thrown indicating an encrypted file is detected and prevent working with the file.

## Credits
Originally developed by Alex Barney (aka @thealexbarney) and is a continuation of his works.
