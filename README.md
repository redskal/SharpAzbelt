## SharpAzbelt

#### Overview

This is an attempt to port [Azbelt](https://github.com/daddycocoaman/azbelt) by Leron Gray from Nim to C#. It can be used to enumerate and pilfer Azure-related credentials from Windows boxes and Azure IaaS resources (VM, VMSS, WVD, etc).

When using Azbelt from the Sliver Armory it would crash and kill my implants, so I wanted to fix that. I'm definitely not great with C#, but I had no desire to work with Nim either, and I wanted to be able to use in-process `execute-assembly` to run it.

#### Modules

 - `aadjoin` - Gets info about machine AAD status via `NetGetAadJoinInformation`
 - `credman` - Gets credentials from Credential Manager
 - `env` - Looks for Azure/AAD specific environment variables that may contain secrets
 - `managed` - Calls IMDS endpoint to get info about machine with managed identity
 - `msal` - Looks in various MSAL caches for tokens. Tokens are parsed to display scope and validity
 - `sso` - If machine is AAD joined, get signed PRT cookie
 - `tbres` - Gets tokens from Token Broker cache
 - `all` - Runs all enumeration except SSO

#### Acknowledgements

The project is a port of MC Ohm-I's [azbelt](https://github.com/daddycocoaman/azbelt).
It makes use of code from:
 - [https://github.com/leechristensen/RequestAADRefreshToken](https://github.com/leechristensen/RequestAADRefreshToken)
 - [https://github.com/xpn/WAMBam](https://github.com/xpn/WAMBam)
 - [https://gist.github.com/benpturner/c7376718558bb118111c7cad651a25ce](https://gist.github.com/benpturner/c7376718558bb118111c7cad651a25ce)

#### Licence

Respect the licenses of any code used, adapted or otherwise from the original projects above.

For my own code, it's provided under [\#YOLO Public License](LICENSE)