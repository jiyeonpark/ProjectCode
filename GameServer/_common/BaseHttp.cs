using BestHTTP;
using Newtonsoft.Json;
using System;

namespace LetsBaseball.Network.Http
{
    public class BaseHttp
    {
        private HTTPRequest request;

        public void Get(Uri url, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Get, (req, resp) =>
            {
                callback(req, resp);
            });

            request.Send();
        }

        public void Post(Uri url, object field, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Post, (req, resp) =>
            {
                callback(req, resp);
            });

            request.AddHeader("Content-Type", "application/json");
            request.RawData = new System.Text.UTF8Encoding().GetBytes(JsonConvert.SerializeObject(field));
            request.Send();
        }

        public void Post(Uri url, byte[] field, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Post, (req, resp) =>
            {
                callback(req, resp);
            });

            request.AddHeader("Content-Type", "application/byte");
            request.RawData = field;
            request.Send();
        }

        public void Put(Uri url, object field, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Put, (req, resp) =>
            {
                callback(req, resp);
            });

            request.AddHeader("Content-Type", "application/json");
            request.RawData = new System.Text.UTF8Encoding().GetBytes(JsonConvert.SerializeObject(field));
            request.Send();
        }

        public void Put(Uri url, byte[] field, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Put, (req, resp) =>
            {
                callback(req, resp);
            });

            request.AddHeader("Content-Type", "application/byte");
            request.RawData = field;
            request.Send();
        }

        public void Delete(Uri url, OnRequestFinishedDelegate callback)
        {
            request = new HTTPRequest(url, HTTPMethods.Delete, (req, resp) =>
            {
                callback(req, resp);
            });

            request.Send();
        }
    }
}
