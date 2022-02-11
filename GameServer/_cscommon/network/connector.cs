using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WCS.Network
{
    class Connector
    {
        private string _connectorName = string.Empty;
        private ClientTokenManager _clientTokenManager = null;
        private SocketAsyncEventArgsPool _pool_event_args = null;
        //private List<SocketAsyncEventArgs> list = new List<SocketAsyncEventArgs>();
        public Connector(string connectorName, ClientTokenManager clientTokenManager_)
        {
            _connectorName = connectorName;
            _clientTokenManager = clientTokenManager_;

            _pool_event_args = new SocketAsyncEventArgsPool(this.eventHander_Connect);
        }

        public bool PostConnect(ConnectToken token)
        {
            bool pending = false;

            //if (0 == _clientTokenManager.PoolSize())
            //{   
            //    WCS.logger.Error($"PostConnect({_connectorName}) client token empty.");
            //    return false;
            //}
            
            var e = _pool_event_args.Pop();

            if (null  != e)
            {
                e.UserToken = token;
                e.RemoteEndPoint = token.endpoint;

                try
                {
                    pending = token.socket.ConnectAsync(e);

                    if (false == pending)
                    {
                        ConnectProcess(e);
                    }
                }
                catch (Exception ex)
                {
                    //CloseConnectSocket(e);
                    _clientTokenManager.eventHander_ConnectFail(e.UserToken as ConnectToken);
                    WCS.logger.Error(ex.Message);
                    return false;
                }

               
            }
            else
            {
                WCS.logger.Error($"PostConnect({_connectorName}). _pool_event_args is empty. e is null.");
                return false;
            }

            return true;
        }

        private void eventHander_Connect(object sender, SocketAsyncEventArgs e)
        {
            ConnectProcess(e);
        }

        private void ConnectProcess(SocketAsyncEventArgs e)
        {
            if(e.ConnectSocket == null)
            {
                logger.Error("ConnectProcess == null");
                _clientTokenManager.eventHander_ConnectFail(e.UserToken as ConnectToken);
                //CloseConnectSocket(e);
                //return;
            }
            if (SocketError.Success != e.SocketError)
            {
                _clientTokenManager.eventHander_ConnectFail(e.UserToken as ConnectToken);

                //CloseConnectSocket(e);
            }
            else
            {
                var token = e.UserToken as ConnectToken;

                _clientTokenManager.eventHander_Connected(null, token);
                
                e.UserToken = null;

                
            }
            _pool_event_args.Push(e);
        }

        private void CloseConnectSocket(SocketAsyncEventArgs e)
        {
            if (null != e.UserToken)
            {
                var token = e.UserToken as ConnectToken;

                if (null != token)
                {
                }

                e.UserToken = null;
            }

            _pool_event_args.Push(e);
        }
    }
}