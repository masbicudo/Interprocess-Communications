        using System;
        using System.IO;
        using System.IO.Pipes;
        using System.Threading;

        namespace NamedPipesServer
        {
            static class Program
            {
                static void Main(string[] args)
                {
                    var server = new PipeServer("MeuNamedPipe");
                    server.ProcessClient += ProcessClient;
                    server.Iniciar();
                    server.EsperarAteDesconectar();
                }

                private static volatile int number;
                private static ConsoleColor[] colors = new []
                                                       {
                                                           ConsoleColor.Blue,
                                                           ConsoleColor.Cyan, 
                                                           ConsoleColor.Green, 
                                                           ConsoleColor.Magenta, 
                                                           ConsoleColor.Red, 
                                                           ConsoleColor.White, 
                                                           ConsoleColor.Yellow, 
                                                       };

                private static object locker = new object();

                private static void ProcessClient(PipeServer sender, NamedPipeServerStream stream)
                {
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream);

                    var random = new Random();

                    var color = colors[Interlocked.Increment(ref number)];
                    writer.WriteLine(color);

                    int num = 0;
                    while (true)
                    {
                        num += random.Next(10);
                        lock (locker)
                        {
                            Console.ForegroundColor = color;
                        Console.WriteLine("Send: " + num);
                        }
                        writer.WriteLine(num);
                        writer.Flush();
                        stream.WaitForPipeDrain();

                        Thread.Sleep(1000);

                        var recv = reader.ReadLine();
                        lock (locker)
                        {
                            Console.ForegroundColor = color;
                            Console.WriteLine("Received: " + recv);
                        }
                        var recvNum = int.Parse(recv);
                        num = recvNum;
              
                        Thread.Sleep(1000);
                    }
                }
            }
        }
