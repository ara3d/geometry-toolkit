using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

[ExecuteAlways]
public class IpcClient : MonoBehaviour
{
    public bool Send = false;
    
    // Update is called once per frame
    void Update()
    {
        if (!Send) return;
        Send = false;
        const int port = 13;
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

        using TcpClient client = new();
        client.Connect(ipEndPoint);
        using var stream = client.GetStream();
        var msg = "What time is it?";
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }
}

/*
 * 
// Remote object.
public class RemoteObject : MarshalByRefObject
{
    private int callCount = 0;

    public int GetCount()
    {
        callCount++;
        return (callCount);
    }
}

public static class RemoteConfig
{
    public const int Port = 9090;
    public const string Name = "RemoteObject.rem";
    public static string Uri => $"tcp://localhost:{Port}/{Name}";
}


public class Server
{
    public static void Main(string[] args)
    {
        // Create the server channel.
        var serverChannel = new TcpChannel(RemoteConfig.Port);

        // Register the server channel.
        ChannelServices.RegisterChannel(serverChannel, false);

        // Show the name of the channel.
        Console.WriteLine($"The name of the channel is {serverChannel.ChannelName}.");

        // Show the priority of the channel.
        Console.WriteLine($"The priority of the channel is {serverChannel.ChannelPriority}.");

        // Show the URIs associated with the channel.
        var data = (ChannelDataStore)serverChannel.ChannelData;
        foreach (var uri in data.ChannelUris)
        {
            Console.WriteLine($"The channel URI is {uri}.");
        }

        // Expose an object for remote calls.
        RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(RemoteObject), RemoteConfig.Name,
            WellKnownObjectMode.Singleton);

        // Parse the channel's URI.
        var urls = serverChannel.GetUrlsForUri(RemoteConfig.Name);
        if (urls.Length > 0)
        {
            var objectUrl = urls[0];
            var channelUri = serverChannel.Parse(objectUrl, out var objectUri);
            Console.WriteLine("The object URL is {0}.", objectUrl);
            Console.WriteLine("The object URI is {0}.", objectUri);
            Console.WriteLine("The channel URI is {0}.", channelUri);
        }

        // Wait for the user prompt.
        Console.WriteLine("Press ENTER to exit the server.");
        Console.ReadLine();
    }
}

public class Client
{
    public static void Main(string[] args)
    {
        // Create the channel.
        var clientChannel = new TcpChannel();

        // Register the channel.
        ChannelServices.RegisterChannel(clientChannel, false);

        // Register as client for remote object.
        var remoteType = new WellKnownClientTypeEntry(typeof(RemoteObject), RemoteConfig.Uri);
        RemotingConfiguration.RegisterWellKnownClientType(remoteType);

        // Create a message sink.
        var messageSink = clientChannel.CreateMessageSink(RemoteConfig.Uri, null, out var objectUri);
        Console.WriteLine($"The URI of the message sink is {objectUri}.");
        if (messageSink != null)
        {
            Console.WriteLine($"The type of the message sink is {messageSink.GetType()}.");
        }

        // Create an instance of the remote object.
        var service = new RemoteObject();

        // Invoke a method on the remote object.
        Console.WriteLine("The client is invoking the remote object.");
        Console.WriteLine($"The remote object has been called {service.GetCount()} times.");
    }
}*/