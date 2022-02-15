using System;
using UnityEngine;

namespace LetsBaseball.Network.Http
{
    public class Payloader
    {
        private Action<WCS.Network.wce_err> fail;
        private Action<string, Exception> error;
        protected bool ispopup = false;

        public Payloader OnFail(string msg, WCS.Network.wce_err code)
        {
            CustomLog.Error(WLogType.all, msg);

            if (LobbyServer.IsCreate && LobbyServer.Inst.pollingState == WCS.PoolingState.None && ispopup)
            {
                GMgr.Inst._CommonPopupMgr.ClosePopup();
                ispopup = false;
            }
            GMgr.Inst._CommonPopupInfo.FailPopup(msg, code);
            fail?.Invoke(code);

            return this;
        }

        public Payloader OnError(string msg, Exception ex = null)
        {
            if (ex != null) CustomLog.Exception(WLogType.all, ex);
            else CustomLog.Error(WLogType.all, msg);

            if (LobbyServer.IsCreate && LobbyServer.Inst.pollingState == WCS.PoolingState.None && ispopup)
            {
                GMgr.Inst._CommonPopupMgr.ClosePopup();
                ispopup = false;
            }
            if (ex == null)
                GMgr.Inst._CommonPopupInfo.ErrorPopup(Define.StrSB(GameManager.Inst.serverVersion.ToString(), " 서버 다운 : 서버팀에 문의 !!"));
            error?.Invoke(msg, ex);

            return this;
        }

        public Payloader Callback(Action<WCS.Network.wce_err> fail, Action<string, Exception> error)
        {
            if (fail != null)
                this.fail += fail;
            if (error != null)
                this.error += error;

            return this;
        }
    }

    public class Payloader<T> : Payloader
    {
        private Action<T> complete;
        private Action<T> success;

        public Payloader<T> OnComplete(T data)
        {
            if (LobbyServer.IsCreate && LobbyServer.Inst.pollingState == WCS.PoolingState.None && ispopup)
            {
                GMgr.Inst._CommonPopupMgr.ClosePopup();
                ispopup = false;
            }
            complete?.Invoke(data);

            return this;
        }

        public Payloader<T> OnSuccess(T data)
        {
            if (LobbyServer.IsCreate && LobbyServer.Inst.pollingState == WCS.PoolingState.None && ispopup)
            {
                GMgr.Inst._CommonPopupMgr.ClosePopup();
                ispopup = false;
            }
            success?.Invoke(data);

            return this;
        }

        public Payloader<T> Callback(Action<T> complete = null, Action<T> success = null, Action<WCS.Network.wce_err> fail = null, Action<string, Exception> error = null)
        {
            if (LobbyServer.IsCreate && LobbyServer.Inst.pollingState == WCS.PoolingState.None)
            {
                GMgr.Inst._CommonPopupMgr.OpenPopup(CommonPopup.PopupState.Waiting);
                ispopup = true;
            }
            Callback(fail, error);
            if (complete != null)
                this.complete += complete;
            if (success != null)
                this.success += success;

            return this;
        }
    }
}