# .NET DIDComm v2 Mediator
## A Mediator for the .NET ecosystem
This Mediator is written in C# and built for the .NET ecosystem. It provides a server-project and a client-library to communicate with the mediator. The database uses MS-SQL with EF.
It allows SSI agents to connect to the mediator, run thorugh the Mediator Corrdination Protocol and then use the Mediator to recieve 'basic messages' using the Pickup Protocol.

**A Azure hosted version of the mediator can be found here: [https://mediator.blocktrust.dev](https://mediator.blocktrust.dev)**

To get to the oob_url directly use this [link](https://mediator.blocktrust.dev/oob_url). 


The mediator is based on two other blocktrust libraries.
* [Blocktrust DIDComm v2](https://github.com/bsandmann/blocktrust.DIDComm)
* [Blocktrust PeerDID](https://github.com/bsandmann/blocktrust.PeerDID)

The projects use a Nuget-feed which is currently private, so the DIDComm and PeerDID libraries have to be build beforehand.

If you have any trouble feel free to contact me at: sandmann@blocktrust.dev and I'll help you setup the environment until a more detailed description will be available.


## Features
This Mediator implements the following protocols:

* [Out-of-Band Messages 2.0](https://identity.foundation/didcomm-messaging/spec/#out-of-band-messages)
* [Routing Protocol 2.0](https://identity.foundation/didcomm-messaging/spec/#routing-protocol-20)
* [Mediator Coordination Protocol 2.0](https://didcomm.org/mediator-coordination/2.0/)
* [Pickup Protocol 3.0](https://didcomm.org/pickup/3.0/)
* [Trust Ping Protocol 2.0](https://identity.foundation/didcomm-messaging/spec/#trust-ping-protocol-20)
* [Discover Features Protocol 2.0](https://identity.foundation/didcomm-messaging/spec/#discover-features-protocol-20)
* [Basic Message Protocol 2.0](https://didcomm.org/basicmessage/2.0/)
* [Shorten URL Protocol 1.0](https://didcomm.org/shorten-url/1.0/)
* [DIDComm v2 Return-Route Extension](https://github.com/decentralized-identity/didcomm-messaging/blob/main/extensions/return_route/main.md)

## Notes
The project is mostly compatible with the RootsId Mediator project. That means messages send from the Jupyter notebooks provided in the [RootsId repository](https://github.com/roots-id/didcomm-mediator) to the .net mediator should be processed correctly. The other way around also works: You can use the .net client-library to send messages to the RootsId Mediator.
For ease of use simply try to use the provided Test projects which simulate a complete flow a mediator-setup, sending basic messages and picking them up.
