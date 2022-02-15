using System;
using WCS.Network;

namespace LetsBaseball.Network.Http
{
    public enum ServerVersion
    {
        none = 0,
        local_juho,
        local_migaeng,
        dev,
        planner,        // 기획자 서버용


        aws_alpha,      // 외부접속용
        aws_test,       // 내부 테스트용

        // 
        service,        // 배포 : serverinfo 다운참조
    }

    public class BaseLogicHttp : BaseHttp
    {
        #region URL
        public static string urlLogin = "";
        public static string UrlLogin {
            get {
                if (GameManager.IsCreate)
                {
                    switch (GameManager.Inst.serverVersion)
                    {
                        case ServerVersion.local_juho: return "http://10.95.8.29:52000";
                        case ServerVersion.local_migaeng: return "http://10.95.8.15:52000";
                        case ServerVersion.dev: return "http://10.95.8.132:52000";
                        case ServerVersion.planner: return "http://10.95.8.98:52000";
                        case ServerVersion.aws_alpha: return "http://13.209.217.50:52000";
                        case ServerVersion.aws_test: return "http://52.79.143.249:52000";
                    }
                }
                return "http://10.95.8.132:52000"; 
            } 
        }
        public static int serverGid = 0;
        public static int ServerGid
        {
            get
            {
                if (GameManager.IsCreate)
                {
                    switch (GameManager.Inst.serverVersion)
                    {
                        case ServerVersion.local_juho: return 50005;
                        case ServerVersion.local_migaeng: return 50006;
                        case ServerVersion.dev: return 50001;
                        case ServerVersion.planner: return 50001;
                        case ServerVersion.aws_alpha: return 51001;
                        case ServerVersion.aws_test: return 51002;
                    }
                }
                return 50001;
            }
        }
        public static string urlLobby;
        public static int lobbyPort;
        public static string urlGame;
        public static ushort gamePort;
        public static long roomID;
        public static long matchID;
        public static wce_match_type matchType;
        #endregion

        Payloader<T> Converter<T>(Payloader<T> payloader, BestHTTP.HTTPResponse res) where T : IDeserialize, new()
        {
            try
            {
                if (res == null)
                {
                    payloader.OnError("Payloader HTTPResponse null");
                    return payloader;
                }

                if (res.IsSuccess)
                {
                    var readStream = ReadStreamPool.instance.Pop();
                    Buffer.BlockCopy(res.Data, 0, readStream.buffer, 0, res.Data.Length);

                    var response = new T();
                    response.Deserialize(readStream.br);
                    ReadStreamPool.instance.Push(readStream);

                    CustomLog.Log(WLogType.all, "BaseLogicHttp Payloader ", response.ToString());

                    if (res.Data != null)
                    {
                        wp_web_base webbase = response as wp_web_base;
                        wce_err err = (wce_err)webbase.result;
                        if (err == wce_err.none)
                        {
                            //성공
                            payloader.OnSuccess(response);
                        }
                        else
                        {
                            //실패
                            payloader.OnFail(Define.StrSB("Fail ", err, " : ", response), err);
                        }
                    }
                    else
                    {   //실패
                        payloader.OnFail(Define.StrSB("Fail ", wce_err.empty, " : ", response), wce_err.empty);
                    }

                    payloader.OnComplete(response);
                }
                else
                {   //실패
                    payloader.OnFail(Define.StrSB("Fail http send : StatusCode = ", res.StatusCode, " : ", res.DataAsText), wce_err.packet_send_failed);
                }
            }
            catch (Exception ex)
            {
                //코드 에러
                payloader.OnError(Define.StrSB("Fail client error : ", ex.Message), ex);
            }

            return payloader;
        }

        public Payloader<T> Post<T>(string url, ISerialize request) where T : IDeserialize, new()
        {
            if (GMgr.IsCreate) GMgr.Inst._SystemMgr.CheckNetworkPopup();

            SendStream sendStream = SendStreamPool.instance.Pop();
            request.Serialize(sendStream.bw);

            var payloader = new Payloader<T>();
            Post(new Uri(url), sendStream.buffer, (req, res) =>
            {
                payloader = Converter<T>(payloader, res);
            });

            SendStreamPool.instance.Push(sendStream);

            return payloader;
        }

        public Payloader<T> Put<T>(string url, ISerialize request) where T : IDeserialize, new()
        {
            SendStream sendStream = SendStreamPool.instance.Pop();
            request.Serialize(sendStream.bw);

            var payloader = new Payloader<T>();
            Put(new Uri(url), sendStream.buffer, (req, res) =>
            {
                payloader = Converter<T>(payloader, res);
            });

            SendStreamPool.instance.Push(sendStream);

            return payloader;
        }
    }
}
