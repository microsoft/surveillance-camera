/*
 * Copyright (c) 2012-2014 Microsoft Mobile.
 * */

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Browser;
using System.Runtime.Serialization;

using SurveillanceCamera.Model;

namespace SurveillanceCamera
{
    /*
     * Internal states     
     * */
    public enum HttpEngineState
    {
        ENone = 0,
        EAuthenticate = 1,
        ESendNotification = 2,
    }

    /*
     * The RequestState class passes data across async calls
     * */
    public class RequestState
    {
        public RequestState()
        {
            RequestPassed = false;
            HttpEngineState = HttpEngineState.ENone;
            RequestCount = 1;
        }

        public HttpEngineState HttpEngineState
        {
            get;
            set;
        }

        public int RequestCount
        {
            get;
            set;
        }

        public bool RequestPassed
        {
            get;
            set;
        }

        public HttpWebRequest Request
        {
            get;
            set;
        }

        public byte[] PostByteArray
        {
            get;
            set;
        }

        public string Dir
        {
            get;
            set;
        }

        public string Secret
        {
            get;
            set;
        }

        public string ServiceId
        {
            get;
            set;
        }
    }


    /*
     * Class for communicate with Nokia Notifications API
     * */
    public class NotifServerApi
    {
        /* *******************************************************************
         * Nokia Notifications API
         * Needed information to connect into service:
         * */

        // Test env url
        //public static string HOST_URI = "https://alpha.one.ovi.com";
        // Prodaction env url
        public static string HOST_URI = "https://nnapi.ovi.com";

        public static string PING_URI = "/nnapi/1.0/ping";
        public static string NOTIF_URI = "/nnapi/1.0/jid/";
        // *******************************************************************

        private string _user;
        private string _password;

        private string _realm;
        private string _nonce;
        private string _qop;
        private string _cnonce;
        private DateTime _cnonceDate;
        private int _nc;

        private string _url;
        private Uri _uri;
        private string _dir;

        private bool _isPOST;

        public delegate void RequestDoneDelegate(RequestState requestState);
        public event RequestDoneDelegate RequestDoneEvent = null;

        // Store one failed alert notificationrequest for to try it again after re-authentication
        private RequestState FailedRequest
        {
            get;
            set;
        }

        /*
         * Nokia Notifications API service handler
         * */
        public NotifServerApi()
        {
            /*
             * With Silverlight, you can specify whether the browser or the client provides HTTP handling 
             * for your Silverlight-based applications. 
             * By default, HTTP handling is performed by the browser and you must opt-in to client HTTP handling as follows
             * */
            WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
        }

        /*
         * Returns engine data for saving state
         * */
        public HttpEngineModel EngineData()
        {
            HttpEngineModel data = new HttpEngineModel();
            data.Cnonce = _cnonce;
            data.CnonceDate = _cnonceDate;
            data.Nc = _nc;
            data.Nonce = _nonce;
            data.Qop = _qop;
            data.Realm = _realm;
            return data;
        }

        public void RestoreEngineData(HttpEngineModel restoredData)
        {
            System.Diagnostics.Debug.WriteLine("RestoreEngineData");
            _cnonce = restoredData.Cnonce;
            _cnonceDate = restoredData.CnonceDate;
            _nc = restoredData.Nc;
            _nonce = restoredData.Nonce;
            _qop = restoredData.Qop;
            _realm = restoredData.Realm;
        }

        /*
         * Returns MD5 hash from the imput string
         * */
        private string CalculateMd5Hash(string input)
        {
            UTF8Encoding enc = new UTF8Encoding();
            var inputBytes = enc.GetBytes(input);
            var hash = MD5Core.GetHashString(inputBytes).ToLower();
            return hash.ToString();
        }

        /*
         * Parses values from http header
         * */
        private string GrabHeaderVar(string varName,string header)
        {
            var regHeader = new Regex(string.Format(@"{0}=""([^""]*)""", varName));
            var matchHeader = regHeader.Match(header);
            if (matchHeader.Success)
                return matchHeader.Groups[1].Value;
            else
                return "";
        }

        /*
         * Makes digest authentication header, read more from RFC 2617
         * */
        private string GetDigestHeader(string dir)
        {
            // http://tools.ietf.org/html/rfc2617

            _nc = _nc + 1;
            string HA1 = CalculateMd5Hash(string.Format("{0}:{1}:{2}", _user, _realm, _password));
            string HA2;
            if (_isPOST)
                HA2 = CalculateMd5Hash(string.Format("{0}:{1}", "POST", dir));
            else
                HA2 = CalculateMd5Hash(string.Format("{0}:{1}", "GET", dir));

            var response = CalculateMd5Hash(string.Format("{0}:{1}:{2:D8}:{3}:{4}:{5}", HA1, _nonce, _nc, _cnonce, _qop, HA2));
            
            string ret =  string.Format("Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", " +
                "algorithm=MD5, response=\"{4}\", qop={5}, nc={6}, cnonce=\"{7}\"",
                _user, _realm, _nonce, dir, response, _qop, _nc.ToString("D8"), _cnonce);

            return ret;
        }

        /*
         * Ping (GET post) the Notifications Server API for getting authenticated
         * */
        public void RequestPing(string serviceId, string secret, string dir)
        {
            RequestGET(HttpEngineState.EAuthenticate, serviceId, secret, dir);
        }

        /*
         * Send Notification into Notifications Service using Server API
         * */
        public void RequestSendNotification(string serviceId, string secret, string dir, string appName, string message)
        {
            string parameters = "";
            parameters += "toapp="+appName+"&";
            parameters += "payload="+message;
            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
            RequestPOST(HttpEngineState.ESendNotification,serviceId, secret, dir, byteArray);
        }

        /*
         * Make POST request
         * */
        private void RequestPOST(HttpEngineState state, string serviceId, string secret, string dir, byte[] parameters)
        {
            _user = serviceId;
            _password = secret;
            _dir = dir;
            _url = HOST_URI + dir;
            _uri = new Uri(_url);

            _isPOST = true;
            var request = (HttpWebRequest)WebRequest.Create(_uri);
            request.UseDefaultCredentials = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers[HttpRequestHeader.ContentLength] = parameters.Length.ToString();

            // If we've got a recent Auth header, re-use it
            if (!string.IsNullOrEmpty(_cnonce) && DateTime.Now.Subtract(_cnonceDate).TotalHours < 1.0)
            {
                request.Headers[HttpRequestHeader.Authorization] = GetDigestHeader(dir);
            }

            // Put the request into the state object so it can be passed around
            RequestState customRequestState = new RequestState();
            customRequestState.Request = request;
            customRequestState.PostByteArray = parameters;
            customRequestState.HttpEngineState = state;
            customRequestState.Dir = dir;
            customRequestState.Secret = secret;
            customRequestState.ServiceId = serviceId;
            request.BeginGetRequestStream(GetRequestStreamCallback, customRequestState);
        }

        /*
         * Send POST parameters
         * */
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            // Get the RequestState object from the async result.
            RequestState customRequestState = (RequestState)asynchronousResult.AsyncState;
            HttpWebRequest postRequest = (HttpWebRequest)customRequestState.Request;

            // End the stream request operation
            Stream postStream = postRequest.EndGetRequestStream(asynchronousResult);
            postStream.Write(customRequestState.PostByteArray, 0, customRequestState.PostByteArray.Length);
            postStream.Close();

            // Start/Continue the web POST request
            postRequest.BeginGetResponse(RequestCompleted, customRequestState);
        }

        /*
         * Send GET request 
         * */
        private void RequestGET(HttpEngineState state, string serviceId, string secret, string dir)
        {
            _user = serviceId;
            _password = secret;
            _dir = dir;
            _url = HOST_URI + dir;
            _uri = new Uri(_url);

            _isPOST = false;

            var request = (HttpWebRequest)WebRequest.Create(_uri);
            request.Method = "GET";
            request.UseDefaultCredentials = false;

            RequestState customRequestState = new RequestState();
            customRequestState.Request = request;
            customRequestState.HttpEngineState = state;

            // If we've got a recent Auth header, re-use it!
            if (!string.IsNullOrEmpty(_cnonce) && DateTime.Now.Subtract(_cnonceDate).TotalHours < 1.0)
            {
                request.Headers[HttpRequestHeader.Authorization] = GetDigestHeader(dir);
            }
            request.BeginGetResponse(RequestCompleted, customRequestState);
        }

        /*
         * Handle failed ALERT notification request cache
         * Store failed request and re-request if needed
         * */
        private void HandleFailedAlertRequestCache(RequestState currentRequest)
        {
            if (currentRequest.RequestPassed == false)
            {
                // Store failed ALERT notification request for requesting it again after authentication
                if (currentRequest.HttpEngineState == HttpEngineState.ESendNotification)
                {
                    System.Diagnostics.Debug.WriteLine("Store failed alert notification request");
                    FailedRequest = currentRequest;
                }
            }
            else
            {
                if (FailedRequest != null)
                {
                    if (FailedRequest.HttpEngineState != currentRequest.HttpEngineState)
                    {
                        // Try again, only one time
                        if (FailedRequest.RequestCount < 2)
                        {
                            System.Diagnostics.Debug.WriteLine("Request again failed alert notification request");
                            // Request again
                            FailedRequest.RequestCount += 1;
                            RequestPOST(FailedRequest.HttpEngineState, FailedRequest.ServiceId, FailedRequest.Secret, FailedRequest.Dir, FailedRequest.PostByteArray);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Clean cache");
                            // Clean cache
                            FailedRequest = null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Clean cache");
                        // Clean cache
                        FailedRequest = null;
                    }
                }
            }
        }

        /*
         * GET or POST request completed
         * */
        private void RequestCompleted(IAsyncResult result)
        {
            RequestState customRequestState = (RequestState)result.AsyncState;
            var request = customRequestState.Request;
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(result);

                if (response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted ||
                    response.StatusCode == HttpStatusCode.NoContent))
                {
                    customRequestState.RequestPassed = true;
                }
                
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed state was " + customRequestState.HttpEngineState);
                    if (RequestDoneEvent != null)
                        RequestDoneEvent(customRequestState);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // OK
                    System.Diagnostics.Debug.WriteLine(response.StatusCode + "State :" + customRequestState.HttpEngineState);
                    if (customRequestState.HttpEngineState == HttpEngineState.EAuthenticate)
                    {
                        // Authentication passed
                        System.Diagnostics.Debug.WriteLine("Autheticated");
                    }

                    if (RequestDoneEvent != null)
                        RequestDoneEvent(customRequestState);

                    HandleFailedAlertRequestCache(customRequestState);
                }
                else if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NoContent)
                {
                    // Accepted or NoContent
                    System.Diagnostics.Debug.WriteLine(response.StatusCode + "State :" + customRequestState.HttpEngineState);
                    if (RequestDoneEvent != null)
                        RequestDoneEvent(customRequestState);

                    HandleFailedAlertRequestCache(customRequestState);
                }
                else
                {
                    // Failed
                    System.Diagnostics.Debug.WriteLine(response.StatusCode + "State :" + customRequestState.HttpEngineState);
                    if (RequestDoneEvent != null)
                        RequestDoneEvent(customRequestState);

                    HandleFailedAlertRequestCache(customRequestState);
                }
            }
            catch (WebException e)
            {
                // Probably failed to authenticate

                // Try to fix a 401 exception by adding a Authorization header
                if (e.Response != null && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Store failed request for requesting it again after authentication
                    HandleFailedAlertRequestCache(customRequestState);
                    
                    // Read WWW-Authenticate header from the response
                    var wwwAuthenticateHeader = e.Response.Headers["WWW-Authenticate"];
                    _realm = GrabHeaderVar("realm", wwwAuthenticateHeader);
                    _nonce = GrabHeaderVar("nonce", wwwAuthenticateHeader);
                    _qop = GrabHeaderVar("qop", wwwAuthenticateHeader);

                    // Create authenticate header
                    _nc = 0;
                    _cnonce = GenerateNewCNonce();
                    _cnonceDate = DateTime.Now;

                    // Make authentication request
                    var authRequest = (HttpWebRequest)WebRequest.Create(new Uri(HOST_URI+PING_URI));
                    authRequest.Method = "GET";
                    _isPOST = false;
                    authRequest.UseDefaultCredentials = false;

                    RequestState customRequestStateForAuth = new RequestState();
                    customRequestStateForAuth.Request = authRequest;
                    customRequestStateForAuth.HttpEngineState = HttpEngineState.EAuthenticate;

                    // Set authenticate header
                    authRequest.Headers[HttpRequestHeader.Authorization] = GetDigestHeader(PING_URI);
                    // Request authentication
                    authRequest.BeginGetResponse(RequestCompleted, customRequestStateForAuth);
                }
                else
                {
                    // General Error
                    if (RequestDoneEvent != null)
                    {
                        if (response != null)
                            System.Diagnostics.Debug.WriteLine(response.StatusCode + "State :" + customRequestState.HttpEngineState);
                        if (RequestDoneEvent != null)
                            RequestDoneEvent(customRequestState);
                    }
                }
            }
        }

        /*
         * Generate new cnonce for authentication header
         * */
        private static string GenerateNewCNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

    } 
}
