Fleck.Wamp
==========


For Building on Mono/Linux:
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
