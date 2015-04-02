        using System;
        using System.IO.Pipes;
        using System.Threading;

        namespace NamedPipeClient
        {
            public class PipeClient
            {
                private Thread runningThread;
                private volatile bool pareAssimQuePossivel;

                public string PipeName { get; private set; }

                public bool PareAssimQuePossivel
                {
                    get { return this.pareAssimQuePossivel; }
                }

                public PipeClient(string pipeName)
                {
                    this.PipeName = pipeName;
                }

                private void ClientProcessorThread()
                {
                    while (true)
                    {
                        try
                        {
                            using (var pipeStream = new NamedPipeClientStream(".", this.PipeName, PipeDirection.InOut))
                            {
                                pipeStream.Connect(10000);
                                try
                                {
                                    this.OnProcessServer(pipeStream);
                                }
                                finally
                                {
                                    pipeStream.WaitForPipeDrain();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            // If there are no more available connections
                            // (254 is in use already) then just
                            // keep looping until one is available
                        }
                    }
                }

                public void Iniciar()
                {
                    this.runningThread = new Thread(this.ClientProcessorThread);
                    this.runningThread.Start();
                }

                public void Parar()
                {
                    this.pareAssimQuePossivel = true;
                }

                public void Abortar()
                {
                    this.runningThread.Abort();
                }

                public event ProcessServer ProcessServer;

                protected virtual void OnProcessServer(NamedPipeClientStream o)
                {
                    if (this.ProcessServer != null)
                        this.ProcessServer(this, o);
                }

                public void EsperarAteDesconectar()
                {
                    if (this.runningThread != null)
                        this.runningThread.Join();
                }
            }

            public delegate void ProcessServer(PipeClient sender, NamedPipeClientStream stream);
        }