using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_Client_Server
{
    class Program
    {
        // Server details
        static int port = 8080;
        static string ipAddress = "127.0.0.1";

        // Client details
        static string serverIPAddress = "127.0.0.1";
        static int serverPort = 8080;

        // Data structures for server
        static Dictionary<string, List<Dictionary<string, int>>> serverCollection = new Dictionary<string, List<Dictionary<string, int>>>()
        {
            {"SetA", new List<Dictionary<string, int>>() {
                new Dictionary<string, int>() { { "One", 1 }, { "Two", 2 } }
            }},
            {"SetB", new List<Dictionary<string, int>>() {
                new Dictionary<string, int>() { { "Three", 3 }, { "Four", 4 } }
            }},
            {"SetC", new List<Dictionary<string, int>>() {
                new Dictionary<string, int>() { { "Five", 5 }, { "Six", 6 } }
            }},
             {"SetD", new List<Dictionary<string, int>>() {
                new Dictionary<string, int>() { { "Seven", 7 }, { "Eight", 8 } }
            }},
            {"SetE", new List<Dictionary<string, int>>() {
                new Dictionary<string, int>() { { "Nine", 9 }, { "Ten", 10 } }
            }}
        };

        static void Main(string[] args)
        {
            // Start server and client in parallel
            Task serverTask = Task.Run(() => StartServer());
            Task clientTask = Task.Run(() => StartClient());

            // Wait for both tasks to complete
            Task.WaitAll(serverTask, clientTask);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Start server
        static void StartServer()
        {
            // Create a TCP/IP socket
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to a local endpoint
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            serverSocket.Bind(localEndPoint);
            // Start listening for connections
            serverSocket.Listen(10);
            Console.WriteLine("Server started on port: " + port);

            // Accept a client connection
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("Client connected from: " + clientSocket.RemoteEndPoint);

                // Handle client request in a separate thread
                Task.Run(() => HandleClientRequest(clientSocket));
            }
        }

        // Handle client request
        static void HandleClientRequest(Socket clientSocket)
        {
            // Receive data from the client
            byte[] buffer = new byte[1024];
            int bytesReceived = clientSocket.Receive(buffer);
            string receivedString = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            Console.WriteLine("Received string from client: " + receivedString);

            // Process the received string
            string response = ProcessRequest(receivedString);

            // Send response to the client
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            clientSocket.Send(responseBytes);
            // Close client connection
            clientSocket.Close();
        }

        // Process client request
        static string ProcessRequest(string receivedString)
        {
            // Split the string into key and value
            string[] parts = receivedString.Split('-');
            string key = parts[0];
            string value = parts[1];

            // Check if the key is present in the server collection
            if (serverCollection.ContainsKey(key))
            {
                // Retrieve the subset for the corresponding key
                List<Dictionary<string, int>> subset = serverCollection[key];

                // Check if the value is present in the subset
                foreach (Dictionary<string, int> entry in subset)
                {
                    if (entry.ContainsKey(value))
                    {
                        // Retrieve the value
                        int count = entry[value];

                        // Send the current time 'count' times with 1 second interval
                        for (int i = 0; i < count; i++)
                        {
                            // Send the current time
                            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            byte[] currentTimeBytes = Encoding.ASCII.GetBytes(currentTime);
                            Thread.Sleep(1000);
                            Console.WriteLine("Sending response to client: " + currentTime);
                        }

                        // Return "OK"
                        return "OK";
                    }
                }
            }

            // If the key or value is not found, return "EMPTY"
            return "EMPTY";
        }

        // Start client
        static void StartClient()
        {
            // Create a TCP/IP socket
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the server
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIPAddress), serverPort);
            clientSocket.Connect(remoteEndPoint);
            Console.WriteLine("Client connected to server: " + remoteEndPoint);
            // Send data to the server
            string message = "SetA-Two";
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(messageBytes);
            Console.WriteLine("Sent message to server: " + message);

            // Receive data from the server
            byte[] buffer = new byte[1024];
            int bytesReceived = clientSocket.Receive(buffer);
            string receivedString = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            Console.WriteLine("Received string from server: " + receivedString);

            // Close the client connection
            clientSocket.Close();
        }
    }
}




