using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WCS.Network;
using System.Threading;



namespace WCS.Network.Http
{
    public class Client
    {
        public async static Task<HttpStatusCode> PostRequestAsync<T>(string url, T obj) where T : class
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            long contentLength = 0;
            byte[] contentBytes = Serializer.Serialize(obj, out contentLength);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = contentLength;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(contentBytes, 0, (int)contentLength);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> PostRequestStreamAsync<T>(string url, T obj) where T : class, ISerialize
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            var sendStream = SendStreamPool.instance.Pop();
            obj.Serialize(sendStream.bw);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = sendStream.position;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(sendStream.buffer, 0, sendStream.position);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
            }
            finally
            {
                SendStreamPool.instance.Push(sendStream);
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> PostRequestAsync<T>(string url, T obj, Action<string> out_data) where T : class
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            long contentLength = 0;
            byte[] contentBytes = Serializer.Serialize(obj, out contentLength);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = contentLength;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(contentBytes, 0, (int)contentLength);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                    else
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                            {
                                string temp = await sr.ReadToEndAsync().ConfigureAwait(false);
                                out_data(temp);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> PostRequestAsync(string url, string data, Action<string> out_data)
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            byte[] contentBytes = Encoding.ASCII.GetBytes(data);
            long contentLength = contentBytes.Length;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = contentLength;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(contentBytes, 0, (int)contentLength);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                    else
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                            {
                                string temp = await sr.ReadToEndAsync().ConfigureAwait(false);
                                out_data(temp);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> PostRequestAsync(string url, SendStream sendStream, Action<ReadStream> out_data)
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = sendStream.position;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(sendStream.buffer, 0, (int)sendStream.position);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                    else
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            int readlen = 0;
                            int resultLength = 0;

                            var readStream = ReadStreamPool.instance.Pop();

                            do
                            {
                                readlen = s.Read(readStream.buffer, resultLength, NET_define.PACKET_BUFFER_SIZE - resultLength);
                                resultLength += readlen;
                            } while (0 < readlen);

                            if (0 < resultLength)
                            {
                                out_data(readStream);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }

        public static HttpStatusCode PostRequest(string url, SendStream sendStream, out ReadStream out_data)
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            // request.Accept = "application/json";
            request.ContentLength = sendStream.position;
            request.Proxy = null;

            out_data = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(sendStream.buffer, 0, (int)sendStream.position);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                    else
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            int readlen = 0;
                            int resultLength = 0;

                            var readStream = ReadStreamPool.instance.Pop();

                            do
                            {
                                readlen = s.Read(readStream.buffer, resultLength, NET_define.PACKET_BUFFER_SIZE - resultLength);
                                resultLength += readlen;
                            } while (0 < readlen);

                            if (0 < resultLength)
                            {
                                out_data = readStream;
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> FileUploadAsync(string url, string filename, byte[] data)
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;

            var client = new HttpClient();
            var form = new MultipartFormDataContent();

            // form.Add(new StringContent("Name"), "Name");
            form.Add(new ByteArrayContent(data, 0, data.Length), "data", filename);

            try
            {
                using (HttpResponseMessage response = await client.PostAsync(url, form).ConfigureAwait(false))
                {
                    error_code = response.StatusCode;

                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }

        public async static Task<HttpStatusCode> FileDownloadAsync(string url, string filename, string down_filename)
        {
            HttpStatusCode error_code = HttpStatusCode.NotFound;

            byte[] contentBytes = System.Text.Encoding.Default.GetBytes(filename);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 10000;
            request.ContentType = "application/x-www-form-urlencoded";//"application/json";
            //// request.Accept = "application/json";
            request.ContentLength = contentBytes.Length;
            request.Proxy = null;

            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(contentBytes, 0, (int)contentBytes.Length);
                    stream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    error_code = response.StatusCode;
                    if (HttpStatusCode.OK != error_code)
                    {
                        WCS.logger.Error(error_code.ToString());
                    }
                    else
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            using (Stream fs = File.OpenWrite(down_filename))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                do
                                {
                                    bytesRead = s.Read(buffer, 0, buffer.Length);
                                    fs.Write(buffer, 0, bytesRead);
                                } while (bytesRead != 0);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                WCS.logger.Error(e.ToString());
                WCS.logger.Error(e.Status.ToString());
            }

            return error_code;
        }
    }

    public class Util
    {
        public static void AspNetCoreHttpResponse<T>(System.IO.Stream httpStream, T packet) where T : class, ISerialize
        {
            var stream = SendStreamPool.instance.Pop();
            packet.Serialize(stream.bw);
            httpStream.Write(stream.buffer, 0, stream.position);
            SendStreamPool.instance.Push(stream);
        }
    }

    public class Serializer
    {
        public static string Serialize<T>(T data) where T : class
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(data);
            }
            catch (Exception e)
            {
                WCS.logger.Error(e.ToString());
            }
            return null;
        }

        public static byte[] Serialize<T>(T info, out long length) where T : class
        {
            try
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(info));
                length = buffer.Length;
                return buffer;
            }
            catch (Exception e)
            {
                WCS.logger.Error(e.ToString());
            }
            length = 0;
            return null;
        }

        public static T Deserialize<T>(string info) where T : class
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(info);
            }
            catch (Exception e)
            {
                WCS.logger.Error(e.ToString());
            }
            return default(T);
        }
    }

    public class PacketUrl
    {
        // client -> login
        public static readonly string URL_login_loginauth = "/login/loginauth";
        public static readonly string URL_login_lobbyaddr = "/login/lobbyaddr";

        // client -> lobby
        public static readonly string URL_lobby_login = "/lobby/login";
        public static readonly string URL_lobby_notify = "/lobby/notify";

        public static readonly string URL_lobby_game_connect = "/lobby/gameconnect";
        public static readonly string URL_lobby_match_pooling = "/lobby/matchpooling";
        public static readonly string URL_lobby_match_start = "/lobby/matchstart";
        public static readonly string URL_lobby_match_cancel = "/lobby/matchcancel";

        public static readonly string URL_lobby_user_set = "/lobby/userset";
        public static readonly string URL_lobby_card_get = "/lobby/cardget";
        public static readonly string URL_lobby_lineup_get = "/lobby/lineupget";
        public static readonly string URL_lobby_lineup_set = "/lobby/lineupset";
        public static readonly string URL_lobby_slot_get = "/lobby/slotget";
        public static readonly string URL_lobby_slot_set = "/lobby/slotset";
        public static readonly string URL_lobby_property_set = "/lobby/propertyset";
        public static readonly string URL_lobby_mail_get = "/lobby/mailget";
        public static readonly string URL_lobby_mail_receive = "/lobby/mailreceive";
        public static readonly string URL_lobby_friend_get = "/lobby/friendget";

        public static readonly string URL_lobby_capsule_prep = "/lobby/capsuleprep";
        public static readonly string URL_lobby_capsule_open = "/lobby/capsuleopen";
        public static readonly string URL_lobby_capsule_standby = "/lobby/capsulestandby";
        public static readonly string URL_lobby_shop_item_buy = "/lobby/shopitembuy";
        public static readonly string URL_lobby_card_levelup = "/lobby/cardlevelup";

        public static readonly string URL_lobby_rank_get = "/lobby/rankget";
        public static readonly string URL_lobby_rank_getdetail = "/lobby/rankgetdetail";
        public static readonly string URL_lobby_rank_getscroll = "/lobby/rankgetscroll";

        public static readonly string URL_lobby_cheat_player = "/lobby/cheatplayer";
        public static readonly string URL_lobby_cheat_equipment = "/lobby/cheatequipment";
        public static readonly string URL_lobby_cheat_currency = "/lobby/cheatcurrency";
        public static readonly string URL_lobby_cheat_capsule = "/lobby/cheatcapsule";
        public static readonly string URL_lobby_cheat_property = "/lobby/cheatproperty";

        // server -> login (rpc)
        public static readonly string URL_login_rpc_infolist = "/login/rpcinfolist";

        // login -> lobby
        public static readonly string URL_lobby_duplicatelogin = "/lobby/duplicatelogin";

        // client -> community

        // gm manager
        public static readonly string URL_gm_manager_userlogin = "/manager/userlogin";
        public static readonly string URL_gm_manager_serverlist = "/manager/serverlist";
        public static readonly string URL_gm_manager_fileupload = "/manager/fileupload";
        public static readonly string URL_gm_manager_filedownload = "/manager/filedownload";
        public static readonly string URL_gm_manager_tablepatch = "/manager/tablepatch";
        public static readonly string URL_gm_manager_tablerepatch = "/manager/tablerepatch";
        public static readonly string URL_gm_manager_tablepatchlist = "/manager/tablepatchlist";
        public static readonly string URL_gm_manager_tablepatchfilelist = "/manager/tablepatchfilelist";
        public static readonly string URL_gm_manager_serverpatch = "/manager/serverpatch";
        public static readonly string URL_gm_manager_servergroupstate = "/manager/servergroupstate";
        public static readonly string URL_gm_manager_servercontrol = "/manager/servercontrol";
        public static readonly string URL_gm_manager_gmnotice = "/manager/gmnotice";

        public static readonly string URL_gm_manager_gmuser_list = "/manager/gmuserlist";
        public static readonly string URL_gm_manager_gmuser_update = "/manager/gmuserupdate";
        // gm server
        public static readonly string URL_gm_server_fileupload = "/server/fileupload";
        public static readonly string URL_gm_server_tablepatch = "/server/tablepatch";
        public static readonly string URL_gm_server_serverpatch = "/server/serverpatch";
        public static readonly string URL_gm_server_servergroupstate = "/server/servergroupstate";
        public static readonly string URL_gm_server_servercontrol = "/server/servercontrol";
        public static readonly string URL_gm_server_gmnotice = "/server/gmnotice";

        public static readonly string URL_gm_userlock = "/gm/userlock";
    }
}

//namespace WCS
//{
//    public partial class netWorkManager
//    {


//        public delegate void HttpRecv<T>(T res);
//        //웹이벤트
//        private netContext _webContext = null;
//        public ManualResetEvent WebEvent { get; private set; }

//        public void WebSetPacketProcess(string session, ReadStream stream)
//        {
//            _webContext.PushQueue(session, stream);
//            WebEvent.Set();
//        }

//        public void WebContextSwitchIndex()
//        {
//            _webContext.SwitchIndex();
//        }
//        public bool GetWebContext(out netContext.Context context)
//        {
//            return _webContext.PopQueue(out context);
//        }
//        public void PushWebContext(netContext.Context context)
//        {
//            ReadStreamPool.instance.Push(context.stream);

//            if (null == _webContext)
//                return;

//            _webContext.Push(context);
//        }
//        public void ContextSwitchIndex()
//        {
//            _webContext.SwitchIndex();
//        }

//        public async Task<HttpStatusCode> PostRequestLobbyAsync<T>(string url, SendStream sendStream, Action<T> out_data)
//        {
//            HttpStatusCode error_code = HttpStatusCode.NotFound;
//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//            request.Method = "POST";
//            request.Timeout = 20000;
//            request.ContentType = "application/json";

//            request.ContentLength = sendStream.position;
//            request.Proxy = null;

//            try
//            {
//                using (Stream stream = request.GetRequestStream())
//                {
//                    stream.Write(sendStream.buffer, 0, (int)sendStream.position);
//                    stream.Close();
//                }

//                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
//                {
//                    error_code = response.StatusCode;
//                    if (HttpStatusCode.OK != error_code)
//                    {
//                        WCS.logger.Error(error_code.ToString());
//                    }
//                    else
//                    {
//                        using (Stream s = response.GetResponseStream())
//                        {
//                            int readlen = 0;
//                            int resultLength = 0;

//                            var readStream = ReadStreamPool.instance.Pop();

//                            do
//                            {
//                                readlen = s.Read(readStream.buffer, resultLength, NET_define.PACKET_BUFFER_SIZE - resultLength);
//                                resultLength += readlen;
//                            } while (0 < readlen);

//                            if (0 < resultLength)
//                            {
//                                //    out_data(readStream);

//                                if (System.Net.HttpStatusCode.OK == error_code)
//                                {
//                                    // if (null == readStream)
//                                    //    return;


//                                    var res_result = new PT_Result2C();

//                                    string restext = Encoding.UTF8.GetString(readStream.buffer).Trim('\0');

//                                    string[] resList = restext.Split('|');
//                                    for (int i = 0; i < resList.Length; i++)
//                                    {
//                                        int nameIndex = resList[i].IndexOf("{");
//                                        if (nameIndex < 0)
//                                        {
//                                            //실패
//                                            break;
//                                        }
//                                        else
//                                        {
//                                            string name = resList[i].Substring(0, nameIndex);
//                                            string resJson = resList[i].Substring(nameIndex);

//                                            string typename = typeof(T).ToString();
//                                            if (name.CompareTo(nameof(PT_Result2C)) == 0)
//                                            {
//                                                res_result = Newtonsoft.Json.JsonConvert.DeserializeObject<PT_Result2C>(resJson);
//                                                if (res_result.result != 1)
//                                                {
//                                                    //실패
//                                                }
//                                            }
//                                            else if (typename.CompareTo(name) == 0)
//                                            {

//                                                T resTemp = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(resJson);
//                                                out_data(resTemp);
//                                            }
//                                            else
//                                            {

//                                                byte[] postData = System.Text.Encoding.UTF8.GetBytes(resJson);
//                                                var sendStream1 = ReadStreamPool.instance.Pop();
//                                                sendStream1.Set(postData, 0, resJson.Length); 
//                                                WebSetPacketProcess(name, sendStream1);


//                                            }


//                                        }

//                                    }



//                                }

//                            }
//                            ReadStreamPool.instance.Push(readStream);
//                        }
//                    }
//                }

//            }
//            catch (WebException e)
//            {
//                WCS.logger.Error(e.ToString());
//                WCS.logger.Error(e.Status.ToString());
//            }

//            return error_code;
//        }
//        //public static HttpStatusCode PostRequest(string url, SendStream sendStream, out ReadStream out_data)
//        //{
//        //    HttpStatusCode error_code = HttpStatusCode.NotFound;
//        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//        //    request.Method = "POST";
//        //    request.Timeout = 10000;
//        //    request.ContentType = "application/x-www-form-urlencoded";//"application/json";
//        //    // request.Accept = "application/json";
//        //    request.ContentLength = sendStream.position;
//        //    request.Proxy = null;

//        //    out_data = null;

//        //    try
//        //    {
//        //        using (Stream stream = request.GetRequestStream())
//        //        {
//        //            stream.Write(sendStream.buffer, 0, (int)sendStream.position);
//        //            stream.Close();
//        //        }

//        //        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//        //        {
//        //            error_code = response.StatusCode;
//        //            if (HttpStatusCode.OK != error_code)
//        //            {
//        //                WCS.logger.Error(error_code.ToString());
//        //            }
//        //            else
//        //            {
//        //                using (Stream s = response.GetResponseStream())
//        //                {
//        //                    int readlen = 0;
//        //                    int resultLength = 0;

//        //                    var readStream = ReadStreamPool.instance.Pop();

//        //                    do
//        //                    {
//        //                        readlen = s.Read(readStream.buffer, resultLength, define.PACKET_BUFFER_SIZE - resultLength);
//        //                        resultLength += readlen;
//        //                    } while (0 < readlen);

//        //                    if (0 < resultLength)
//        //                    {
//        //                        out_data = readStream;
//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }
//        //    catch (WebException e)
//        //    {
//        //        WCS.logger.Error(e.ToString());
//        //        WCS.logger.Error(e.Status.ToString());
//        //    }

//        //    return error_code;
//        //}

//        //public async static Task<HttpStatusCode> FileUploadAsync(string url, string filename, byte[] data)
//        //{
//        //    HttpStatusCode error_code = HttpStatusCode.NotFound;

//        //    var client = new HttpClient();
//        //    var form = new MultipartFormDataContent();

//        //    // form.Add(new StringContent("Name"), "Name");
//        //    form.Add(new ByteArrayContent(data, 0, data.Length), "data", filename);

//        //    try
//        //    {
//        //        using (HttpResponseMessage response = await client.PostAsync(url, form).ConfigureAwait(false))
//        //        {
//        //            error_code = response.StatusCode;

//        //            if (HttpStatusCode.OK != error_code)
//        //            {
//        //                WCS.logger.Error(error_code.ToString());
//        //            }
//        //        }
//        //    }
//        //    catch (WebException e)
//        //    {
//        //        WCS.logger.Error(e.ToString());
//        //        WCS.logger.Error(e.Status.ToString());
//        //    }

//        //    return error_code;
//        //}

//        //public async static Task<HttpStatusCode> FileDownloadAsync(string url, string filename, string down_filename)
//        //{
//        //    HttpStatusCode error_code = HttpStatusCode.NotFound;

//        //    byte[] contentBytes = System.Text.Encoding.Default.GetBytes(filename);

//        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//        //    request.Method = "POST";
//        //    request.Timeout = 10000;
//        //    request.ContentType = "application/x-www-form-urlencoded";//"application/json";
//        //    //// request.Accept = "application/json";
//        //    request.ContentLength = contentBytes.Length;
//        //    request.Proxy = null;

//        //    try
//        //    {                
//        //        using (Stream stream = request.GetRequestStream())
//        //        {
//        //            stream.Write(contentBytes, 0, (int)contentBytes.Length);
//        //            stream.Close();
//        //        }

//        //        using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
//        //        {
//        //            error_code = response.StatusCode;
//        //            if (HttpStatusCode.OK != error_code)
//        //            {
//        //                WCS.logger.Error(error_code.ToString());
//        //            }
//        //            else
//        //            {
//        //                using (Stream s = response.GetResponseStream())
//        //                {
//        //                    using (Stream fs = File.OpenWrite(down_filename))
//        //                    {
//        //                        byte[] buffer = new byte[4096];
//        //                        int bytesRead;
//        //                        do
//        //                        {
//        //                            bytesRead = s.Read(buffer, 0, buffer.Length);
//        //                            fs.Write(buffer, 0, bytesRead);
//        //                        } while (bytesRead != 0);
//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }
//        //    catch (WebException e)
//        //    {
//        //        WCS.logger.Error(e.ToString());
//        //        WCS.logger.Error(e.Status.ToString());
//        //    }

//        //    return error_code;
//        //}
//    }

//    //public class Util
//    //{
//    //    public static void AspNetCoreHttpResponse<T>(System.IO.Stream httpStream, T packet) where T : class, ISerialize
//    //    {
//    //        var stream = SendStreamPool.instance.Pop();
//    //        packet.Serialize(stream.bw);
//    //        httpStream.Write(stream.buffer, 0, stream.position);
//    //        SendStreamPool.instance.Push(stream);
//    //    }
//    //}

//    public class Serializer
//    {
//        public static string Serialize<T>(T data) where T : class
//        {
//            try
//            {
//                return Newtonsoft.Json.JsonConvert.SerializeObject(data);
//            }
//            catch (Exception e)
//            {
//                WCS.logger.Error(e.ToString());
//            }
//            return null;
//        }

//        public static byte[] Serialize<T>(T info, out long length) where T : class
//		{
//			try
//			{
//				var buffer = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(info));
//				length = buffer.Length;
//				return buffer;
//			}
//			catch (Exception e)
//			{
//				WCS.logger.Error(e.ToString());
//			}
//			length = 0;
//			return null;
//		}

//        public static T Deserialize<T>(string info) where T : class
//        {
//            try
//            {
//                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(info);
//            }
//            catch (Exception e)
//            {
//                WCS.logger.Error(e.ToString());
//            }
//            return default(T);
//        }
//    }


//}
