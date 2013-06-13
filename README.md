Fleck.Wamp
==========

WebSocket Application Messaging Protocol (WAMP) implementation based upon Jason Staten's Fleck project (https://github.com/statianzo/Fleck).

Details of the WAMP specification can be found at http://www.wamp.ws.

## Supported WebSocket versions ##
While Fleck supports Hixie-Draft-76/Hybi-00, Hybi-07, Hybi-10 and Hybi-13, for WebSocket purposes, only the last three are supported for WAMP due to the draft specification missing the Sec-WebSocket-Protocol header, which must be set to "wamp" for WAMP to be used as the SubProtocol.

## Dependencies ##
- Fleck (d'uh!) - Git submodule, https://github.com/statianzo/Fleck
- Newtonsoft.Json (aka JSON.Net) - Git submodule, https://github.com/JamesNK/Newtonsoft.Json
- Moq - NuGet package, http://nuget.org/packages/Moq/ (For test suites only)
- NUnit - NuGet package, http://nuget.org/packages/NUnit/ (For test suites only)

## For Building on Mono/Linux: ##
- Needs Mono 3.0.10 installed (for BigInteger support required by Newtonsoft.Json)
  - For Ubuntu, add Vsevolod Kukol's Experimental Mono Packages ```ppa:v-kukol/mono-testing``` to sources.list (https://launchpad.net/~v-kukol/+archive/mono-testing)
- To enable xbuild to download the missing packages as part of the build:
  ```
  export EnableNuGetPackageRestore=true
  ```
- Run 
  ```
  mozroots --import --sync
  ```
  to grab the root SSL certificates and install them in the .Net cache

## License ##

The MIT License (MIT)

Copyright (c) 2013 Adam May

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
