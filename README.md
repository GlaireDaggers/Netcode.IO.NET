# Netcode.IO.NET
A pure managed C# implementation of the Netcode.IO spec

# Project goals
The goal of this project is to provide a pure managed implementation of the [Netcode.IO spec](https://github.com/networkprotocol/netcode.io) coded against .NET 3.5 and using zero native DLLs or wrappers for maximum portability.
Instead of using libsodium like the original C reference implementation, this implementation uses a customized version of the Bouncy Castle cryptography library. You can find the original source code here: https://github.com/bcgit/bc-csharp

Additionally, it is designed for use in games. To this end, it has been designed from the ground up to have as minimal an impact on GC allocations as possible. For the most part, you should not see any GC impact from using Netcode.IO.NET at all.

# API usage
Most of the API resides in the namespace `NetcodeIO.NET`

## Server API
To create and start a new server:
```c#
Server server = new Server(
	maxClients,		// int maximum number of clients which can connect to this server at one time
	publicAddress, port,	// string public address and int port clients will connect to
	protocolID,		// ulong protocol ID shared between clients and server
	privateKeyBytes		// byte[32] private crypto key shared between backend servers
);
server.Start();			// start the server running
```

To listen for various events:
```c#
// Called when a client has connected
server.OnClientConnected += clientConnectedHandler;		// void( RemoteClient client )

// Called when a client disconnects
server.OnClientDisconnected += clientDisconnectedHandler;	// void( RemoteClient client )

// Called when a payload has been received from a client
// Note that you should not keep a reference to the payload, as it will be returned to a pool after this call completes.
server.OnClientMessageRecieved += messageReceivedHandler;	// void( RemoteClient client, byte[] payload, int payloadSize )
```

To send a payload to a remote client connected to the server:
```c#
remoteClient.Send(byte[] payload, int payloadSize);

// or:
server.SendPayload( RemoteClient client, byte[] payload, int payloadSize );
```

To disconnect a client:
```c#
server.Disconnect( RemoteClient client );
```

To get at the arbitrary 256-byte user data which can be passed with a connect token:
```c#
remoteClient.UserData; // byte[256]
```

To stop a server and disconnect any clients:
```c#
server.Stop();
```

## Client API
To create a new client:
```c#
Client client = new Client();
```

To listen for various events:
```c#
// Called when the client's state has changed
// Use this to detect when a client has connected to a server, or has been disconnected from a server, or connection times out, etc.
client.OnStateChanged += clientStateChanged;			// void( ClientState state )

// Called when a payload has been received from the server
// Note that you should not keep a reference to the payload, as it will be returned to a pool after this call completes.
client.OnMessageReceived += messageReceivedHandler;		// void( byte[] payload, int payloadSize )
```

To connect to a server using a connect token:
```c#
client.Connect( connectToken );		// byte[2048] public connect token as returned by a TokenFactory
```

To send a message to a server when connected:
```c#
client.Send( byte[] payload, int payloadSize );
```

To disconnect a client:
```c#
client.Disconnect();
```

## TokenFactory API
TokenFactory can be used to generate the public connect tokens used by clients to connect to game servers.
To create a new TokenFactory:
```c#
TokenFactory tokenFactory = new TokenFactory(
	protocolID,		// must be the same protocol ID as passed to both client and server constructors
	privateKey		// byte[32], must be the same as the private key passed to the Server constructor
);
```

To generate a new 2048-byte public connect token:
```c#
tokenFactory.GenerateConnectToken(
	addressList,		// IPEndPoint[] list of addresses the client can connect to. Must have at least one and no more than 32.
	expirySeconds,		// in how many seconds will the token expire
	serverTimeout,		// how long it takes until a connection attempt times out and the client tries the next server.
	sequenceNumber,		// ulong token sequence number used to uniquely identify a connect token.
	clientID,		// ulong ID used to uniquely identify this client
	userData		// byte[], up to 256 bytes of arbitrary user data (available to the server as RemoteClient.UserData)
);
```

# A note about UDP and unreliability
Netcode.IO.NET is a pure port of the Netcode.IO protocol - nothing more, and nothing less.
At its heart, Netcode.IO is an encryption and connection based abstraction on top of UDP. And, just like UDP, it has zero guarantees about reliability. Your messages may not make it, and they may not make it in order. That's just a fact of the internet.
That said, any game will almost certainly need some kind of reliability layer. To that end, my [ReliableNetcode.NET](https://github.com/KillaMaaki/ReliableNetcode.NET) project provides an agnostic and easy to use reliability layer you can use to add this functionality to your game.
