        using System;
        using System.IO.Pipes;
        using System.Threading;

        namespace NamedPipesServer
        {
            public class PipeServer
            {
                private Thread runningThread;
                private volatile bool pareAssimQuePossivel;

                public string PipeName { get; private set; }

                public PipeServer(string pipeName)
                {
                    this.PipeName = pipeName;
                }

                private void ServerLoop()
                {
                    while (!this.pareAssimQuePossivel)
                        this.ProcessNextClient();
                }

                public void Iniciar()
                {
                    this.runningThread = new Thread(this.ServerLoop);
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

                private void ProcessClientThread(object o)
                {
                    using (var pipeStream = (NamedPipeServerStream)o)
                    {
                        try
                        {
                            this.OnProcessClient(pipeStream);
                        }
                        finally
                        {
                            pipeStream.WaitForPipeDrain();
                            if (pipeStream.IsConnected)
                                pipeStream.Disconnect();
                        }
                    }
                }

                public event ProcessClient ProcessClient;

                protected virtual void OnProcessClient(NamedPipeServerStream o)
                {
                    if (this.ProcessClient != null)
                        this.ProcessClient(this, o);
                }

                private void ProcessNextClient()
                {
                    try
                    {
                        var pipeStream = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, -1);
                        pipeStream.WaitForConnection();

                        // Spawn a new thread for each request and continue waiting
                        var t = new Thread(this.ProcessClientThread);
                        t.Start(pipeStream);
                    }
                    catch (Exception e)
                    {
                        // If there are no more available connections
                        // (254 is in use already) then just
                        // keep looping until one is available
                    }
                }

                public void EsperarAteDesconectar()
                {
                    if (this.runningThread != null)
                        this.runningThread.Join();
                }
            }

            public delegate void ProcessClient(PipeServer sender, NamedPipeServerStream stream);
        }