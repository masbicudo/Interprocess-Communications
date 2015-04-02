        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.IO.Pipes;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;

        namespace NamedPipeClient
        {
            static class Program
            {
                static void Main(string[] args)
                {
                    var client = new PipeClient("MeuNamedPipe");
                    client.ProcessServer += ProcessServer;
                    client.Iniciar();
                    client.EsperarAteDesconectar();
                }

                private static void ProcessServer(PipeClient sender, NamedPipeClientStream stream)
                {
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream);

                    var random = new Random();

                    var colorStr = reader.ReadLine();
                    var color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colorStr);
                    Console.ForegroundColor = color;

                    int num;
                    while (true)
                    {
                        var recv = reader.ReadLine();
                        Console.WriteLine("Received: " + recv);
                        var recvNum = int.Parse(recv);
                        num = recvNum;

                        Thread.Sleep(1000);

                        num += random.Next(10);
                        Console.WriteLine("Send: " + num);
                        writer.WriteLine(num);
                        writer.Flush();
                        stream.WaitForPipeDrain();

                        Thread.Sleep(1000);
                    }
                }
            }
        }
