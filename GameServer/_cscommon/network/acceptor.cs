#if SERVER_UNITY

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WCS.Network
{
    class Acceptor
    {
        private string _acceptName = string.Empty;
        private ClientTokenManager _clientTokenManager = null;
        private Socket _socket_listen = null;
        private SocketAsyncEventArgsPool _pool_event_args = null;
        private Thread _thread = null;
        private ManualResetEvent _event = null;
        private bool _accept_loop = true;
        
        
        public Acceptor(string acceptName, ClientTokenManager clientTokenManager_)
        {            
            _acceptName = acceptName;
            _clientTokenManager = clientTokenManager_;
            
        }

        public void Initialize(int port)
        {            
            _pool_event_args = new SocketAsyncEventArgsPool(this.eventHander_Accept);
            _thread = new Thread(Run);
            _thread.Name = "Acceptor";
            _event = new ManualResetEvent(false);

            CreateSocket(port, NET_define.LISTEN_BACKLOG_SIZE);
            
        }

        private void CreateSocket(int port, int backlog)
        {
            _socket_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket_listen.Bind(new IPEndPoint(IPAddress.Any, port));
            _socket_listen.Listen(backlog);
        }
        
        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _accept_loop = false;

            _event.Set();

            _thread.Join();
        }

        private async void Run()
        {
            bool pending = false;

            while(_accept_loop)
            {
                WCS.logger.Info($"{_acceptName}, _clientTokenManager pool size : {_clientTokenManager.PoolSize()}");

                if (0 == _clientTokenManager.PoolSize())
                {   
                    WCS.logger.Error($"AcceptRun({_acceptName}) client token empty.");                    
                    await Task.Delay(TimeSpan.FromMilliseconds(NET_define.LISTEN_WAIT_MSEC)).ConfigureAwait(false);
                    continue;
                }

                _event.Reset();
                
                var e = _pool_event_args.Pop();

                if (null != e)
                {
                    try
                    {
                        //this.semaphoreAcceptedClients.WaitOne();
                        pending = _socket_listen.AcceptAsync(e);

                        if (false == pending)
                        {
                            AcceptProcess(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        CloseAcceptSocket(e);
                        WCS.logger.Error(ex.Message);
                        continue;
                    }

                    
                }
                else
                {
                    WCS.logger.Error($"AcceptRun({_acceptName}). _pool_event_args is empty. e is null.");
                }

                _event.WaitOne();
            }

            WCS.logger.Info($"AcceptRun({_acceptName}). exit.");
        }

        private void eventHander_Accept(object sender, SocketAsyncEventArgs e)
        {
            AcceptProcess(e);
        }

        private void AcceptProcess(SocketAsyncEventArgs e)
        {
            _event.Set();
                        
            if (SocketError.Success != e.SocketError)
            {
                CloseAcceptSocket(e);
            }
            else
            {
                _clientTokenManager.eventHander_Connected(e.AcceptSocket, null);

                e.AcceptSocket = null;
                _pool_event_args.Push(e);
            }
        }

        private void CloseAcceptSocket(SocketAsyncEventArgs e)
        {
            e.AcceptSocket.Close();
            _pool_event_args.Push(e);
            logger.Error("CloseAcceptSocket SocketAsyncEventArgs");
        }
    }
}

#endif