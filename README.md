Fleck.Wamp
==========

WebSocket Application Messaging Protocol (WAMP) implementation based upon Jason Staten's Fleck project (https://github.com/statianzo/Fleck).

Details of the WAMP specification can be found at http://www.wamp.ws.

## Usage ##

The Fleck.Wamp codebase is designed so that there are two points for reuse:
- WampCommsHandler, and
- WampServer

WampCommsHandler is a lightweight wrapper over Fleck that really only deals with deserializing the WebSockets messages into a WAMP message structure. All details of the protocol sequencing (such as the implementation of the welcome message sending and prefixes/subscriptions) is left up to the calling class.

WampServer is a more complete implementation of the WAMP specification and additionally deals with the following protocol details:
- Sending of Welcome message on opening of the socket
- Maintenance of the Prefix and Subscriptions sets
- Supports Authentication via WAMP-CRA (Challenge-Response Authentication) - Not yet completed

## Supported WebSocket versions ##
While Fleck supports Hixie-Draft-76/Hybi-00, Hybi-07, Hybi-10 and Hybi-13, for WebSocket purposes, only the last three are supported for WAMP due to the draft specification missing the Sec-WebSocket-Protocol header, which must be set to "wamp" for WAMP to be used as the SubProtocol.

Note that Fleck isn't fully compatible with the WebSockets proposed standard (RFC6455), in that it mirrors back the clients Sec-WebSocket-Protocol field, rather than selecting a suitable one from the options as indicated at the end of page 7 of the RFC (http://tools.ietf.org/html/rfc6455).

   Option fields can also be included.  In this version of the protocol,
   the main option field is |Sec-WebSocket-Protocol|, which indicates
   the subprotocol that the server has selected.  WebSocket clients
   verify that the server included one of the values that was specified
   in the WebSocket client's handshake.  A server that speaks multiple
   subprotocols has to make sure it selects one based on the client's
   handshake and specifies it in its handshake.

Fleck.Wamp in this case insists that the client only sends through the value "wamp" for the Sec-WebSocket-Protocol header value.

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
