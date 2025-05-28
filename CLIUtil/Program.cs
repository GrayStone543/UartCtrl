// See https://aka.ms/new-console-template for more information
using CLIUtil;
using System.IO.Ports;

Console.WriteLine("Hello, CLIUtil!");

PortChat portChat = new PortChat();
portChat.Start();

//Console.WriteLine("Press any key to exit...");