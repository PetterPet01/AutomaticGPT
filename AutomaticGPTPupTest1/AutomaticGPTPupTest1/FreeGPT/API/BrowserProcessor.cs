using AutomaticGPTPupTest1.FreeGPT.Models.Exceptions;
using AutomaticGPTPupTest1.FreeGPT.Models.Local;
using AutomaticGPTPupTest1.FreeGPT.Models.Web;
using Newtonsoft.Json.Linq;
using PetterPet.FreeGPT.Helper;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using static AutomaticGPTPupTest1.EventHandlerExtensions;

namespace PetterPet.FreeGPT.API
{
    public enum BrowserState
    {
        Uninitialized,
        Initializing,
        Initialized
    }

    public enum GPTState
    {
        IdleNewChat,
        IdleInChat,
        Generating
    }

    public enum BrowserVerificationStatus
    {
        OK,
        Cancelled,
        Timeout,
        UnhandledException
    }

    public enum GPTError
    {
        ConnectionFailed,
    }

    //public sealed class MessageResponseFilter : IResponseFilter
    //{
    //    private Stream responseStream;
    //    public ActionBlock<MessageResponseRaw> processMessage;
    //    private string id;

    //    /// <summary>
    //    /// StreamResponseFilter constructor (The reuse of stream may be buggy in a multithreaded environment)
    //    /// </summary>
    //    /// <param name="stream">a writable stream</param>
    //    public MessageResponseFilter(Stream stream, ActionBlock<MessageResponseRaw> processMessage, string id)
    //    {
    //        responseStream = stream;
    //        this.processMessage = processMessage;
    //        this.id = id;
    //    }

    //    bool IResponseFilter.InitFilter()
    //    {
    //        return responseStream != null && responseStream.CanWrite;
    //    }

    //    FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
    //    {
    //        if (dataIn == null)
    //        {
    //            dataInRead = 0;
    //            dataOutWritten = 0;

    //            //Debug.WriteLine("FILTER DONE");

    //            return FilterStatus.Done;
    //        }

    //        //Calculate how much data we can read, in some instances dataIn.Length is
    //        //greater than dataOut.Length
    //        dataInRead = Math.Min(dataIn.Length, dataOut.Length);
    //        dataOutWritten = dataInRead;

    //        var readBytes = new byte[dataInRead];
    //        dataIn.Read(readBytes, 0, readBytes.Length);
    //        dataOut.Write(readBytes, 0, readBytes.Length);

    //        //Write buffer to the memory stream
    //        responseStream.Write(readBytes, 0, readBytes.Length);

    //        string data = System.Text.Encoding.UTF8.GetString(readBytes);
    //        //Debug.WriteLine(data);
    //        processMessage.Post(new MessageResponseRaw { Data = data, Id = id });

    //        //If we read less than the total amount available then we need
    //        //return FilterStatus.NeedMoreData so we can then write the rest
    //        if (dataInRead < dataIn.Length)
    //        {
    //            return FilterStatus.NeedMoreData;
    //        }

    //        //Debug.WriteLine(dataIn == null ? 0 : dataIn.Length);
    //        //Debug.WriteLine(data);
    //        //Debug.WriteLine("FILTER DONE");
    //        return FilterStatus.Done;
    //    }

    //    /// <inheritdoc/>
    //    public void Dispose()
    //    {
    //        responseStream = null;
    //    }
    //}

    //public sealed class GenericStreamResponseFilter : IResponseFilter
    //{
    //    private MemoryStream memoryStream;

    //    /// <summary>
    //    /// StreamResponseFilter constructor
    //    /// </summary>
    //    /// <param name="stream">a writable stream</param>
    //    public GenericStreamResponseFilter()
    //    {
    //        memoryStream = new MemoryStream();
    //    }

    //    bool IResponseFilter.InitFilter()
    //    {
    //        return memoryStream != null && memoryStream.CanWrite;
    //    }

    //    FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
    //    {
    //        if (dataIn == null)
    //        {
    //            dataInRead = 0;
    //            dataOutWritten = 0;

    //            return FilterStatus.Done;
    //        }

    //        //Calculate how much data we can read, in some instances dataIn.Length is
    //        //greater than dataOut.Length
    //        dataInRead = Math.Min(dataIn.Length, dataOut.Length);
    //        dataOutWritten = dataInRead;

    //        var readBytes = new byte[dataInRead];
    //        dataIn.Read(readBytes, 0, readBytes.Length);
    //        dataOut.Write(readBytes, 0, readBytes.Length);

    //        //Write buffer to the memory stream
    //        memoryStream.Write(readBytes, 0, readBytes.Length);

    //        //If we read less than the total amount available then we need
    //        //return FilterStatus.NeedMoreData so we can then write the rest
    //        if (dataInRead < dataIn.Length)
    //        {
    //            return FilterStatus.NeedMoreData;
    //        }

    //        return FilterStatus.Done;
    //    }

    //    public byte[] Data
    //    {
    //        get { return memoryStream.ToArray(); }
    //    }

    //    public void Dispose()
    //    {
    //        memoryStream.Dispose();
    //        memoryStream = null;
    //    }
    //}

    //public class GenericResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    //{
    //    ConcurrentDictionary<ulong, MemoryStream> responseDictionary;
    //    private Action<IRequest, byte[]>? processor;
    //    private ConcurrentDictionary<string, TaskCompletionSource<Response>?>? responseManager;
    //    private Action<string>? removeId;
    //    public IEnumerable<Func<IRequest, (bool, string?)>>? conditions;

    //    public GenericResourceRequestHandler(Action<IRequest, byte[]> processor)
    //    {
    //        responseDictionary = new ConcurrentDictionary<ulong, MemoryStream>();
    //        this.processor = processor;
    //    }

    //    public GenericResourceRequestHandler(ConcurrentDictionary<string, TaskCompletionSource<Response>?> responseManager,
    //        IEnumerable<Func<IRequest, (bool, string?)>> conditions, Action<string> removeId)
    //    {
    //        responseDictionary = new ConcurrentDictionary<ulong, MemoryStream>();
    //        this.responseManager = responseManager;
    //        this.conditions = conditions;
    //        this.removeId = removeId;
    //    }

    //    protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
    //    {
    //        Debug.WriteLine($"Requested ID: {request.Identifier}");
    //        MemoryStream memoryStream = new MemoryStream();
    //        Debug.WriteLine($"Added successfully: {responseDictionary.TryAdd(request.Identifier, memoryStream)}");
    //        return new StreamResponseFilter(memoryStream);
    //    }

    //    void ClearMemoryStream(MemoryStream ms)
    //    {
    //        ms.SetLength(0);
    //    }

    //    protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
    //    {
    //        Debug.WriteLine($"LOADED URL: {request.Url}");
    //        Debug.WriteLine($"Received ID: {request.Identifier}");
    //        bool success = responseDictionary.TryRemove(request.Identifier, out MemoryStream? memoryStream);
    //        Debug.WriteLine($"Removed succesfully: {success}");
    //        if (success)
    //        {
    //            if (memoryStream != null)
    //            {
    //                //string text = Encoding.UTF8.GetString(memoryStream.ToArray());
    //                byte[] data = memoryStream.ToArray();
    //                if (processor != null)
    //                    processor(request, data);

    //                string? id = null;

    //                GetFixedId(request, ref id);
    //                if (id == null)
    //                    GetManualId(request, ref id);

    //                if (id != null && responseManager != null)
    //                {
    //                    Debug.WriteLine("TASK COMPLETING");
    //                    if (responseManager.TryRemove(id, out TaskCompletionSource<Response>? val))
    //                    {
    //                        if (removeId != null)
    //                            removeId(id);
    //                        if (val != null)
    //                            val.SetResult(new Response(request, response, data));
    //                    }
    //                }

    //                //ClearMemoryStream(memoryStream);
    //            }
    //        }
    //    }

    //    void GetFixedId(IRequest request, ref string? id)
    //    {
    //        if (conditions != null)
    //            foreach (Func<IRequest, (bool, string?)> condition in conditions)
    //            {
    //                (bool valid, string? fixedId) = condition(request);
    //                if (valid && fixedId != null)
    //                {
    //                    id = fixedId;
    //                    return;
    //                }
    //            }

    //        id = null;
    //    }

    //    void GetManualId(IRequest request, ref string? id)
    //    {
    //        string[] split = request.Url.Split('#');
    //        if (split.Length == 1)
    //        {
    //            id = null;
    //            return;
    //        }
    //        id = request.Url.Split('#').Last();
    //    }

    //    //        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
    //    //        {
    //    //            var headers = request.Headers;
    //    //            if (headers["Origin"] == "about:blank")
    //    //                return CefReturnValue.Cancel;
    //    //            else
    //    //                return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
    //    //        }

    //    //        protected override bool OnResourceResponse(
    //    //    IWebBrowser chromiumWebBrowser,
    //    //    IBrowser browser,
    //    //    IFrame frame,
    //    //    IRequest request,
    //    //    IResponse response
    //    //)
    //    //        {
    //    //            if (request.Url.Contains('#') && request.Headers["Origin"] != "about:blank")
    //    //            {
    //    //                request.SetHeaderByName("Origin", "about:blank", true);
    //    //                return true;
    //    //            }
    //    //            return false;
    //    //        }
    //}

    //public class MessageHandler : CefSharp.Handler.ResourceRequestHandler
    //{
    //    private readonly MemoryStream memoryStream;
    //    private ActionBlock<MessageResponseRaw> processMessage;

    //    public MessageHandler(ActionBlock<MessageResponseRaw> processMessage)
    //    {
    //        memoryStream = new MemoryStream();
    //        this.processMessage = processMessage;
    //    }

    //    void ClearMemoryStream(MemoryStream ms)
    //    {
    //        ms.SetLength(0);
    //    }

    //    protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
    //    {
    //        //We rightfully assume the request to have POST data
    //        JObject messageRequest = JObject.Parse(
    //            Encoding.UTF8.GetString(request.PostData.Elements[0].Bytes));

    //        if (messageRequest == null)
    //            throw new Exception("Deserialized MessageRequest is null");
    //        JToken? token = messageRequest["conversation_id"];
    //        string? conversationId = token == null ? "" : token.Value<string>();
    //        if (conversationId == null)
    //            throw new Exception("Deserialized conversationId is null");

    //        return new MessageResponseFilter(memoryStream, processMessage, conversationId);
    //    }

    //    //protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
    //    //{
    //    //    if (request.Method == "POST")
    //    //    {
    //    //        string requestJSON = Encoding.UTF8.GetString(request.PostData.Elements[0].Bytes);
    //    //    }
    //    //    return CefReturnValue.Continue;
    //    //}

    //    protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
    //    {
    //        //You can now get the data from the stream
    //        //var bytes = memoryStream.ToArray();
    //        //Debug.WriteLine(System.Text.Encoding.UTF8.GetString(bytes));
    //        //dataNotifier(System.Text.Encoding.UTF8.GetString(bytes));

    //        //memoryStream.SetLength(0);
    //        //Debug.WriteLine(response.StatusCode);
    //        ClearMemoryStream(memoryStream);
    //    }
    //}

    //public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    //{
    //    internal IEnumerable<(Func<IRequest, bool>, IResourceRequestHandler)> specializedHandlers;
    //    internal GenericResourceRequestHandler? genericHandler;

    //    public CustomRequestHandler(IEnumerable<(Func<IRequest, bool>, IResourceRequestHandler)> specializedHandlers,
    //        GenericResourceRequestHandler? genericHandler = null)
    //    {
    //        this.specializedHandlers = specializedHandlers;
    //        this.genericHandler = genericHandler;
    //    }
    //    protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    //    {
    //        if (specializedHandlers != null)
    //            foreach ((Func<IRequest, bool> condition, IResourceRequestHandler handler) in specializedHandlers)
    //                if (condition(request))
    //                    return handler;

    //        if (genericHandler != null && genericHandler.conditions != null)
    //            foreach (Func<IRequest, (bool, string?)> condition in genericHandler.conditions)
    //            {
    //                (bool valid, _) = condition(request);
    //                if (valid)
    //                    return genericHandler;
    //            }

    //        return null;
    //    }
    //}

    internal static class BrowserProcessor
    {
        static string sDecodeHexUnicode = @"var j;
    var hexes = '{0}'.match(/.{1,4}/g) || [];
    var decodedStr = '';
    for(j = 0; j<hexes.length; j++) {
        decodedStr += String.fromCharCode(parseInt(hexes[j], 16));
    };";

        static string sCreateIFrame = @"var elemDiv = document.createElement('iframe');
elemDiv.className = 'code-executor-{0}';
document.head.appendChild(elemDiv);";

        static string sRemoveIFrame = @"var frames = document.getElementsByClassName('code-executor-{0}');
frames[0].parentNode.removeChild(frames[0]);";

        static string sIsSendRequestFunctionImplemented = @"typeof callRequestReceived === ""function""";

        static string sImplementSendMessageFunction = @"var SSE = function (id, url, options) {
  if (!(this instanceof SSE)) {
    return new SSE(id, url, options);
  }

  this.INITIALIZING = -1;
  this.CONNECTING = 0;
  this.OPEN = 1;
  this.CLOSED = 2;

  this.idNotify = id;
  this.urll = url;

  options = options || {};
  this.headers = options.headers || {};
  this.payload = options.payload !== undefined ? options.payload : '';
  this.method = options.method || (this.payload && 'POST' || 'GET');
  this.withCredentials = !!options.withCredentials;

  this.FIELD_SEPARATOR = ':';
  this.listeners = {};

  this.xhr = null;
  this.readyState = this.INITIALIZING;
  this.progress = 0;
  this.chunk = '';

  this.addEventListener = function(type, listener) {
    if (this.listeners[type] === undefined) {
      this.listeners[type] = [];
    }

    if (this.listeners[type].indexOf(listener) === -1) {
      this.listeners[type].push(listener);
    }
  };

  this.removeEventListener = function(type, listener) {
    if (this.listeners[type] === undefined) {
      return;
    }

    var filtered = [];
    this.listeners[type].forEach(function(element) {
      if (element !== listener) {
        filtered.push(element);
      }
    });
    if (filtered.length === 0) {
      delete this.listeners[type];
    } else {
      this.listeners[type] = filtered;
    }
  };

  this.removeAllEventListeners = function(type) {
    if (this.listeners[type] === undefined) {
      return;
    }
    
    delete this.listeners[type];
  }

  this.dispatchEvent = function(e) {
    if (!e) {
      return true;
    }

    e.source = this;

    var onHandler = 'on' + e.type;
    if (this.hasOwnProperty(onHandler)) {
      this[onHandler].call(this, e);
      if (e.defaultPrevented) {
        return false;
      }
    }

    if (this.listeners[e.type]) {
      return this.listeners[e.type].every(function(callback) {
        callback(e);
        return !e.defaultPrevented;
      });
    }

    return true;
  };

  this._setReadyState = function(state) {
    var event = new CustomEvent('readystatechange');
    event.readyState = state;
    this.readyState = state;
    this.dispatchEvent(event);
  };

  this._onStreamFailure = function(e) {
    window.callMessageUpdate(idNotify);
    var event = new CustomEvent('error');
    event.data = e.currentTarget.response;
    this.dispatchEvent(event);
    this.close();
  }

  this._onStreamAbort = function(e) {
    this.dispatchEvent(new CustomEvent('abort'));
    this.close();
  }

  this._onStreamProgress = function(e) {
    if (!this.xhr) {
      return;
    }

    if (this.xhr.status !== 200) {
      this._onStreamFailure(e);
      return;
    }

    if (this.readyState == this.CONNECTING) {
      this.dispatchEvent(new CustomEvent('open'));
      this._setReadyState(this.OPEN);
    }

    var data = this.xhr.responseText.substring(this.progress);
    this.progress += data.length;
    data.split(/(\r\n|\r|\n){2}/g).forEach(function(part) {
      if (part.trim().length === 0) {
        this.dispatchEvent(this._parseEventChunk(this.chunk.trim()));
        this.chunk = '';
      } else {
        this.chunk += part;
      }
    }.bind(this));
  };

  this._onStreamLoaded = function(e) {
    this._onStreamProgress(e);

    // Parse the last chunk.
    this.dispatchEvent(this._parseEventChunk(this.chunk));
    this.chunk = '';
  };

  /**
   * Parse a received SSE event chunk into a constructed event object.
   */
  this._parseEventChunk = function(chunk) {
    if (!chunk || chunk.length === 0) {
      return null;
    }

    var e = {'id': null, 'retry': null, 'data': '', 'event': 'message'};
    chunk.split(/\n|\r\n|\r/).forEach(function(line) {
      line = line.trimRight();
      var index = line.indexOf(this.FIELD_SEPARATOR);
      if (index <= 0) {
        // Line was either empty, or started with a separator and is a comment.
        // Either way, ignore.
        return;
      }

      var field = line.substring(0, index);
      if (!(field in e)) {
        return;
      }

      var value = line.substring(index + 1).trimLeft();
      if (field === 'data') {
        e[field] += value;
      } else {
        e[field] = value;
      }
    }.bind(this));

    var event = new CustomEvent(e.event);
    event.data = e.data;
    event.id = e.id;
    return event;
  };

  this._checkStreamClosed = function() {
    if (!this.xhr) {
      return;
    }

    if (this.xhr.readyState === XMLHttpRequest.DONE) {
      this._setReadyState(this.CLOSED);
    }
  };

  this.stream = function() {
    this._setReadyState(this.CONNECTING);

    this.xhr = new XMLHttpRequest();
    this.xhr.addEventListener('progress', this._onStreamProgress.bind(this));
    this.xhr.addEventListener('load', this._onStreamLoaded.bind(this));
    this.xhr.addEventListener('readystatechange', this._checkStreamClosed.bind(this));
    this.xhr.addEventListener('error', this._onStreamFailure.bind(this));
    this.xhr.addEventListener('abort', this._onStreamAbort.bind(this));
    this.xhr.open(this.method, this.urll);
    for (var header in this.headers) {
      this.xhr.setRequestHeader(header, this.headers[header]);
    }
    this.xhr.withCredentials = this.withCredentials;
    this.xhr.send(this.payload);
  };

  this.close = function() {
    console.log('CLOSE');
    if (this.readyState === this.CLOSED) {
      return;
    }

    this.xhr.abort();
    this.xhr = null;
    this._setReadyState(this.CLOSED);
  };
};

function sendMessage(authorizationToken, customPayload, id)
{
  let sourcee = new SSE(id, 'https://chat.openai.com/backend-api/conversation', {headers: {
    'Authorization': 'Bearer ' + authorizationToken,
    'Content-Type': 'application/json',
    'accept': 'text/event-stream',
    }, payload: customPayload});
    sourcee.addEventListener('message', function(e) {
      // Assuming we receive JSON-encoded data payloads:
      // var payload = JSON.parse(e.data);
      window.callMessageUpdate(id, e.data);
      if (e.data == '[DONE]') {
        sourcee.removeAllEventListeners('message');
      }
    });
    sourcee.stream();
};";

        static string sExecuteIfSendMessageFunctionExists = @"var isImplemented = typeof sendMessage === ""function"";
if (!isImplemented)
{{0}};
isImplemented;";

        private static string sSafeImplementSendMessageFunction =
            sExecuteIfSendMessageFunctionExists.Replace("{0}", sImplementSendMessageFunction);

        static string sIsVerifyingBrowser = @"document.getElementById('cf-please-wait') != null";
        //static string sIsCloudflareBlocked = @"document.getElementById('cf-stage') != null";
        static string sIsCloudflareBlocked = @"var ele = document.getElementById('challenge-stage');
if (ele == null)
    false;
else
    getComputedStyle(document.getElementById('challenge-stage')).display === 'block'";
        static string sIsSignInRequired = @"var arr = document.getElementsByClassName('btn relative btn-primary');
(arr[0] != null && arr[1] != null && arr[0].innerText == 'Log in' && arr[1].innerText == 'Sign up')";
        static string sIsServiceLoaded = @"document.querySelector('main>div>div>div>div>div>div') != null;";
        static string sIsNextBtnDisclaimerShown = @"document.querySelector('button.btn.relative.btn-neutral.ml-auto') != null";
        static string sIsDoneBtnDisclaimerShown = @"document.querySelector('button.btn.relative.btn-primary.ml-auto') != null";
        static string sClickNextOnDisclaimer = @"document.querySelector('button.btn.relative.btn-neutral.ml-auto').click()";
        static string sClickDoneOnDisclaimer = @"document.querySelector('button.btn.relative.btn-primary.ml-auto').click()";
        static string sIsInMinimizedState = @"var eles = document.querySelectorAll('[class*=""text-sm""]');
var isMinimized = eles[0].className.includes('flex-col');";
        static string sCreateNewChat = $@"{sIsInMinimizedState}
if (isMinimized) document.getElementsByTagName('button')[1].click(); else eles[0].click();";
        static string sIsWindowCompressed = @"document.querySelectorAll('[class*=""text-sm""]').length == 1";
        static string sClickExpandWindow = @"document.getElementsByTagName('button')[0].click()";
        static string sGetConversationsHolder = $@"{sIsInMinimizedState}
eles[isMinimized?2:1]";
        static string sSelectChat = sGetConversationsHolder + @".childNodes[{0}].click()";
        static string sTypeMessage = sDecodeHexUnicode + @"
document.getElementsByTagName('textarea')[0].value = decodedStr;
document.getElementsByTagName('textarea')[0].dispatchEvent(new Event('input', { bubbles: true }));";
        static string sIsMessageTyped = sDecodeHexUnicode + @"
document.getElementsByTagName('textArea')[0].textContent == decodedStr";
        //static string sSendMessage = @"document.querySelector('textArea').nextElementSibling.click()";
        static string sIsMessageSent = sDecodeHexUnicode + @"
var eles = document.querySelectorAll('[class*=""text-sm""]');
var childs = eles[eles.length - 1].childNodes;
document.getElementsByTagName('textArea').length > 0 && document.getElementsByTagName('textArea')[0].textContent == '' && childs[childs.length - 3].innerText == decodedStr;";
        static string sIsGenerating = @"document.querySelector('form').querySelector('button').innerText == 'Stop generating'";
        static string sIsAbleToRegenerate = $@"{sIsInMinimizedState}
var btns = document.querySelector('form').querySelectorAll('button');
(!isMinimized && btns[0].innerText == 'Regenerate response') || (isMinimized && !btns[1].disabled);";
        static string sClickGenerationBtn = $@"{sIsInMinimizedState}
var btns = document.querySelector('form').querySelectorAll('button');
if (isMinimized) btns[1].click(); else btns[0].click();";
        static string sGetLastCompletedResponse = sGetConversationsHolder + @".click()";
        static string sGetLastResponse = @"Array.from(Array.from(document.querySelectorAll('div[class*=""markdown""]')).pop().querySelectorAll('p')).map(p => p.innerHTML);";
        static string sIsShowMoreBtnShown = sGetConversationsHolder + @".lastChild.className.includes('btn')";
        static string sClickShowMore = sGetConversationsHolder + @".lastChild.click()";
        static string sSendRequest = sDecodeHexUnicode + @"
fetch(""{1}"", {
  method: ""{2}"",
  headers: {{3}}, 
  body: {4},
  credentials: 'include',
  redirect: 'manual'
}).then((response) => response.text())
    .then((text) => {
      window.callRequestReceived('{5}', text);
    });";

        private static string sSendMessage = sDecodeHexUnicode + @"
sendMessage('{1}', decodedStr, '{2}');";

        private static readonly UnicodeEncoding bigEndianUnicode = new UnicodeEncoding(true, true);

        private static string HexEncode(string? stringValue)
        {
            if (stringValue == null)
                return "";

            var ba = bigEndianUnicode.GetBytes(stringValue);
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static string HexDecode(string hexString)
        {
            if (hexString == null || (hexString.Length & 1) == 1) return "";
            var numberChars = hexString.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return bigEndianUnicode.GetString(bytes);
        }

        public static async Task<bool> IsSendRequestFunctionImplemented(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsSendRequestFunctionImplemented);
        }

        public static async Task<bool> SafeImplementSendMessageFunction(this IPage page)
        {
            Debug.WriteLine(sSafeImplementSendMessageFunction);
            return await page.EvaluateExpressionAsync<bool>(sSafeImplementSendMessageFunction);
        }

        public static async Task<bool> IsVerifyingBrowser(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsVerifyingBrowser);
        }

        public static async Task<bool> IsCloudflareBlocked(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsCloudflareBlocked);
        }

        public static async Task<bool> IsSignInRequired(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsSignInRequired);
        }

        public static async Task<bool> IsNextBtnDisclaimerShown(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsNextBtnDisclaimerShown);
        }

        public static async Task<bool> IsDoneBtnDisclaimerShown(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsDoneBtnDisclaimerShown);
        }

        public static async Task<bool> IsServiceLoaded(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsServiceLoaded);
        }

        public static async Task ClickNextOnDisclaimer(this IPage page)
        {
            await page.EvaluateExpressionAsync(sClickNextOnDisclaimer);
        }

        public static async Task ClickDoneOnDisclaimer(this IPage page)
        {
            await page.EvaluateExpressionAsync(sClickDoneOnDisclaimer);
        }

        public static async Task CreateNewChat(this IPage page)
        {
            await page.EvaluateExpressionAsync(sCreateNewChat);
        }

        public static async Task SelectChat(this IPage page, int index)
        {
            await page.EvaluateExpressionAsync(string.Format(sSelectChat, index));
        }

        public static async Task TypeMessage(this IPage page, string message)
        {
            string script = sTypeMessage.Replace("{0}", HexEncode(message));
            //Debug.WriteLine(script);
            await page.EvaluateExpressionAsync(script);
        }

        public static async Task<bool> IsMessageTyped(this IPage page, string message)
        {
            string script = sIsMessageTyped.Replace("{0}", HexEncode(message));
            return await page.EvaluateExpressionAsync<bool>(script);
        }

        //public static async Task SendMessage(this IPage page)
        //{
        //    await page.EvaluateExpressionAsync(sSendMessage);
        //}

        public static async Task<bool> IsMessageSent(this IPage page, string message)
        {
            string script = sIsMessageSent.Replace("{0}", HexEncode(message));
            return await page.EvaluateExpressionAsync<bool>(script);
        }

        public static async Task<bool> IsGenerating(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsGenerating);
        }

        public static async Task<bool> IsAbleToRegenerate(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsAbleToRegenerate);
        }

        public static async Task ClickGenerationBtn(this IPage page)
        {
            await page.EvaluateExpressionAsync(sClickGenerationBtn);
        }

        public static async Task<List<string>> GetLastCompletedResponse(this IPage page)
        {
            return await page.EvaluateExpressionAsync<List<string>>(sGetLastCompletedResponse);
        }

        public static async Task<List<string>> GetLastResponse(this IPage page)
        {
            return await page.EvaluateExpressionAsync<List<string>>(sGetLastResponse);
        }

        public static async Task<bool> IsWindowCompressed(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsWindowCompressed);
        }

        public static async Task ClickExpandWindow(this IPage page)
        {
            await page.EvaluateExpressionAsync(sClickExpandWindow);
        }

        public static async Task<bool> IsShowMoreBtnShown(this IPage page)
        {
            return await page.EvaluateExpressionAsync<bool>(sIsShowMoreBtnShown);
        }

        public static async Task ClickShowMore(this IPage page)
        {
            await page.EvaluateExpressionAsync(sClickShowMore);
        }

        public static async Task<string> SendRequest(this IPage page, string id, string url, string method, string headers, string? body)
        {
            string script = sSendRequest
                .Replace("{0}", HexEncode(body))
                .Replace("{1}", url)
                .Replace("{2}", method)
                .Replace("{3}", headers)
                .Replace("{4}", body == null ? "null" : "decodedStr")
                .Replace("{5}", id);
            Debug.WriteLine(script);
            return await page.EvaluateExpressionAsync<string>(script);
        }

        public static async Task SendMessage(this IPage page, string authorizationToken, string payload, string id)
        {
            await page.EvaluateExpressionAsync(sSendMessage
                .Replace("{0}", HexEncode(payload))
                .Replace("{1}", authorizationToken)
                .Replace("{2}", id));
        }
    }

    class ConversationsLoadEventArgs : EventArgs
    {
        public ConcurrentDictionary<int, ConversationsQuery> Conversations { get; set; }
    }

    //public class Response
    //{
    //    public IResponse response;
    //    public byte[] data;
    //    //public TaskCompletionSource<(IRequest, string)> responseSource;

    //    public Response(IResponse response, byte[] data)
    //    {
    //        this.response = response;
    //        this.data = data;
    //        //this.responseSource = responseSource;
    //    }
    //}

    internal class NetworkManager
    {
        private Func<IPage> pageGetter;
        private IFrame? frame;
        private Func<bool> isAvailable;

        private IEnumerable<Func<IRequest, (bool, string?)>> genericConditions;
        private IEnumerable<(Func<IRequest, bool>, Action<IRequest, string>)> specializedHandlers;

        private CancellationTokenSource ctsAllRequestManagers;
        //Generic response managers (using manual ids)
        private ConcurrentDictionary<string, TaskCompletionSource<IResponse>?> manualIdGenericResponseManager;
        //Generic request / response managers
        private BufferBlock<(string?, ManualRequest, TaskCompletionSource<string>?)> requestManager;
        private ConcurrentDictionary<string, TaskCompletionSource<string>?> responseManager;

        //Message request / response managers
        private BufferBlock<(string, MessageRequestRaw, BufferBlock<APIResponseMessage<MessageResponse>>)> requestMessagesManager;
        private ConcurrentDictionary<string, InternalMessageResponse> responseMessagesManager;

        public event EventHandler<GPTErrorReceivedEventArgs> GPTErrorReceived = delegate { };

        public NetworkManager(Func<IPage> pageGetter, IEnumerable<Func<IRequest, (bool, string?)>> genericConditions,
            IEnumerable<(Func<IRequest, bool>, Action<IRequest, string>)> specializedHandlers, Func<bool> isAvailable)
        {
            this.pageGetter = pageGetter;
            this.genericConditions = genericConditions;
            this.specializedHandlers = specializedHandlers;

            manualIdGenericResponseManager = new ConcurrentDictionary<string, TaskCompletionSource<IResponse>?>();
            requestManager = new BufferBlock<(string?, ManualRequest, TaskCompletionSource<string>?)>();
            responseManager = new ConcurrentDictionary<string, TaskCompletionSource<string>?>();

            requestMessagesManager =
                new BufferBlock<(string, MessageRequestRaw, BufferBlock<APIResponseMessage<MessageResponse>>)>();
            responseMessagesManager = new ConcurrentDictionary<string, InternalMessageResponse>();

            pageGetter().Response += ProcessResponseAsync;
            //browser.RequestHandler = new CustomRequestHandler(
            //    specializedHandlers,
            //    new GenericResourceRequestHandler(responseManager, conditions, RemoveIFrame)
            //    );
            this.isAvailable = isAvailable;

            ctsAllRequestManagers = new CancellationTokenSource();
            ProcessRequestAsync(ctsAllRequestManagers.Token);
            ProcessRequestMessagesAsync(ctsAllRequestManagers.Token);
        }

        async void ProcessRequestAsync(CancellationToken ct)
        {
            while (await requestManager.OutputAvailableAsync())
            {
                if (!isAvailable())
                {
                    await Task.Delay(1500);
                    continue;
                }

                await SafeImplementSendRequestHelper();

                (string? id, ManualRequest request, TaskCompletionSource<string>? tcs) = await requestManager.ReceiveAsync();

                HookResponse(id, tcs);
                //var frame = await browser.GetBrowser().ImplementIFrame(id);
                //frame.LoadRequest(request);
                //pageGetter().EvaluateExpressionAsync()

                List<string> pairs = new List<string>();

                foreach (var kvp in request.Headers)
                    pairs.Add($"'{kvp.Key}': '{kvp.Value}'");
                string header = string.Join(',', pairs);

                while (true)
                {
                    try
                    {
                        await pageGetter().SendRequest(id, request.Url, request.Method, header, request.PostData);
                        break;
                    }
                    catch (Exception e)
                    {
                        GPTErrorReceived.Raise(this,
                            new GPTErrorReceivedEventArgs(GPTError.ConnectionFailed, e.Message));
                        await Task.Delay(5000);
                    }

                    if (ct.IsCancellationRequested)
                        return;
                }
            }
        }

        async void ProcessRequestMessagesAsync(CancellationToken ct)
        {
            while (await requestMessagesManager.OutputAvailableAsync())
            {
                if (!isAvailable())
                {
                    Debug.WriteLine("Waiting for availability...");
                    await Task.Delay(1500);
                    continue;
                }

                await SafeImplementSendMessageHelper();

                (string? id, MessageRequestRaw request, BufferBlock<APIResponseMessage<MessageResponse>> bb) =
                    await requestMessagesManager.ReceiveAsync();

                //HookResponse(id, tcs);
                //var frame = await browser.GetBrowser().ImplementIFrame(id);
                //frame.LoadRequest(request);
                //pageGetter().EvaluateExpressionAsync()

                //List<string> pairs = new List<string>();

                //foreach (var kvp in request.Headers)
                //    pairs.Add($"'{kvp.Key}': '{kvp.Value}'");
                //string header = string.Join(',', pairs);

                while (responseMessagesManager.Count >= 1)
                    await Task.Delay(5000);
                responseMessagesManager[id] = new InternalMessageResponse(bb);

                try
                {
                    Debug.WriteLine("Sending message...");
                    await pageGetter().SendMessage(request.AuthToken, request.Payload, id);
                    Debug.WriteLine("Message successfully sent");
                }
                catch (Exception e)
                {
                    //    GPTErrorReceived.Raise(this,
                    //        new GPTErrorReceivedEventArgs(GPTError.ConnectionFailed, e.Message));
                    //    await Task.Delay(5000);
                }

                if (ct.IsCancellationRequested)
                    return;
            }
        }

        private async void ProcessResponseAsync(object? sender, ResponseCreatedEventArgs e)
        {
            Debug.WriteLine($"RECEIVED RESPONSE, code: {(int)e.Response.Status}");
            if (!e.Response.Ok)
            {
                Debug.WriteLine(e.Response.Url);
                try
                {
                    if (e.Response.Request.PostData != null)
                        Debug.WriteLine(e.Response.Request.PostData.ToString());
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                return;
            }
            string data;
            try
            {
                data = await e.Response.TextAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
            foreach ((Func<IRequest, bool> validate, Action<IRequest, string> process) t in specializedHandlers)
                if (t.validate(e.Response.Request))
                {
                    t.process(e.Response.Request, data);
                    return;
                }

            string? id = null;

            GetFixedId(e.Response.Request, ref id);
            //if (id == null)
            //    GetManualId(e.Response.Request, ref id);

            Debug.WriteLine("Processed Id");

            if (id != null /*&& responseManager != null*/)
            {
                Debug.WriteLine("TASK COMPLETING");
                if (manualIdGenericResponseManager.TryRemove(id, out TaskCompletionSource<IResponse>? val))
                {
                    if (val != null)
                        val.SetResult(e.Response);
                }
            }
        }

        private bool CallRequestReceived(string id, string data)
        {
            //Debug.WriteLine($"RECEIVED RESPONSE, code: {(int)e.Response.Status}");
            //if (!e.Response.Ok)
            //    return;
            //byte[] data;
            //try
            //{
            //    data = await e.Response.BufferAsync();
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception.Message);
            //    return;
            //}
            //foreach ((Func<IRequest, bool> validate, Action<IRequest, string > process) t in specializedHandlers)
            //    if (t.validate(e.Response.Request))
            //    {
            //        t.process(e.Response.Request, data);
            //        return;
            //    }

            //string? id = null;

            //GetFixedId(e.Response.Request, ref id);
            //if (id == null)
            //    GetManualId(e.Response.Request, ref id);

            Debug.WriteLine("CallRequestReceived: Processed Id");

            //if (id != null /*&& responseManager != null*/)
            //{
            Debug.WriteLine("TASK COMPLETING");
            if (responseManager.TryRemove(id, out TaskCompletionSource<string>? val))
            {
                if (val != null)
                    val.SetResult(data);
            }
            //}

            return true;
        }

        public void HookResponse(string id, TaskCompletionSource<IResponse>? tcs)
        {
            manualIdGenericResponseManager[id] = tcs;
        }

        public void HookResponse(string id, TaskCompletionSource<string>? tcs)
        {
            responseManager[id] = tcs;
        }

        // tcs is null IF AND ONLY IF withResponse = false
        public string EnqueueRequest(ManualRequest request, TaskCompletionSource<string>? tcs = null)
        {
            string id;
            do
            {
                id = MiscHelper.GetRandomId();
            } while (responseManager.ContainsKey(id));

            request.Url = $"{request.Url}#{id}";
            if (!requestManager.Post((id, request, tcs)))
                throw new Exception("Failed to enqueue request");

            return id;
        }

        public string EnqueueMessage(string authorizationToken, string payload, BufferBlock<APIResponseMessage<MessageResponse>> bb)
        {
            string id;
            do
            {
                id = MiscHelper.GetRandomId();
            } while (responseMessagesManager.ContainsKey(id));

            if (!requestMessagesManager.Post((id,
                    new MessageRequestRaw
                    {
                        AuthToken = authorizationToken,
                        //Id = id,
                        Payload = payload,
                    }, bb)))
                throw new Exception("Failed to enqueue message");
            Debug.WriteLine("Message enqueued");

            return id;
        }

        async Task SafeImplementSendRequestHelper()
        {
            bool isImplemented = await pageGetter().IsSendRequestFunctionImplemented();
            if (!isImplemented)
                await pageGetter().ExposeFunctionAsync<string, string, bool>("callRequestReceived", CallRequestReceived);
        }

        async Task SafeImplementSendMessageHelper()
        {
            bool isImplemented = await pageGetter().SafeImplementSendMessageFunction();
            if (!isImplemented)
            {
                await pageGetter().ExposeFunctionAsync<string, bool>("callMessageFailure", callMessageFailure);
                await pageGetter().ExposeFunctionAsync<string, string, bool>("callMessageUpdate", callMessageUpdate);
            }
        }

        bool callMessageFailure(string id)
        {
            if (!responseMessagesManager.ContainsKey(id))
                return false;

            if (responseMessagesManager.TryRemove(id,
                    out InternalMessageResponse? response))
            {
                response.BufferBlock.Post(new APIResponseMessage<MessageResponse>(
                    true,
                    new ResponseError
                    {
                        Detail = "Connection abruptly closed"
                    }, null));
                response.BufferBlock.Complete();
                response.Cache = "";
            }

            return true;
        }

        bool callMessageUpdate(string id, string data)
        {
            //Debug.WriteLine("Received message: " + data);
            ProcessMessage(new MessageResponseRaw
            {
                Id = id,
                Data = data,
            });
            return true;
        }

        void ProcessMessage(MessageResponseRaw rawMessage)
        {
            if (!responseMessagesManager.ContainsKey(rawMessage.Id))
                return;

            string[] datas = (responseMessagesManager[rawMessage.Id].Cache + rawMessage.Data)
                .Replace("\r\n\r\n", "\n\n")
                .Split("\n\n");
            for (int i = 0; i < datas.Length; i++)
            {
                string dataPackage = datas[i];

                //Signal that ChatGPT's response is completed
                if (dataPackage == "data: [DONE]")
                {
                    //GPTState = GPTState.IdleInChat;
                    //if (responseMessagesManager.TryRemove(rawMessage.Id, out BufferBlock<MessageResponse>? bb))
                    //    bb.Complete();
                    responseMessagesManager[rawMessage.Id].Cache = "";
                    return;
                }

                string cleanData;
                if (dataPackage.StartsWith("data: "))
                    cleanData = dataPackage.Substring(6);
                else
                    cleanData = dataPackage;

                if (cleanData == "\n" || cleanData == "")
                    continue;
                if (cleanData.IndexOf('{') == -1)
                {
                    responseMessagesManager[rawMessage.Id].Cache = "";
                    continue;
                }
                cleanData = cleanData.Substring(cleanData.IndexOf('{'));
                //Debug.WriteLine(Uri.EscapeUriString(cleanData) + Environment.NewLine + "---------------------------------");
                if (JsonHelper.TryParseJson(cleanData, out MessageStatusResponse messageStatus))
                {
                    Debug.WriteLine("Message Status Reported");
                    continue;
                }
                if (!JsonHelper.ParseJsonWithException(cleanData, out MessageResponse? message, out Exception ex))
                //Data is incomplete
                {
                    if (JsonHelper.TryParseJson(cleanData, out ResponseError? error))
                    {
                        if (responseMessagesManager.TryRemove(rawMessage.Id,
                                out InternalMessageResponse? response))
                        {
                            response.BufferBlock.Complete();
                            response.Cache = "";
                        }

                        responseMessagesManager[rawMessage.Id].BufferBlock.Post(new APIResponseMessage<MessageResponse>(
                            true,
                            new ResponseError
                            {
                                Detail = $"OpenAI API error: {error.Detail}"
                            }, null));

                        Debug.WriteLine(error.Detail);
                        return;
                    }
                    if (ex != null)
                    {
                        Debug.WriteLine(cleanData);
                        Debug.WriteLine(ex.Message);
                    }

                    //Skips irrelevant / faulty data package
                    //if (i < datas.Length - 1)
                    //    return;

                    //Debug.WriteLine("Deserialization failed");

                    if (responseMessagesManager[rawMessage.Id].Cache != "")
                        Debug.WriteLine("responseMessageCache was faulty twice or more");
                    if (i == datas.Length - 1)
                        responseMessagesManager[rawMessage.Id].Cache = datas[datas.Length - 1];

                    return;
                }
                else
                    responseMessagesManager[rawMessage.Id].Cache = "";

                if (message == null)
                    throw new InternalAPIException("Parsed MessageResponse is not supposed to be null");
                if (message.message == null)
                    throw new InternalAPIException("Parsed MessageResponse.Message is not supposed to be null");

                //Check for message end of turn
                if (message.message.end_turn != null && message.message.end_turn == true
                    && message.message.metadata.finish_details != null)
                {
                    Debug.WriteLine("FINISHED");
                    //GPTState = GPTState.IdleInChat;
                    if (!responseMessagesManager.TryRemove(rawMessage.Id,
                            out InternalMessageResponse? response))
                        if (!responseMessagesManager.TryRemove("", out response))
                            throw new InternalAPIException("Failed to signal completion to a message");

                    message.message.metadata.finish_details = null;
                    response.BufferBlock.Post(new APIResponseMessage<MessageResponse>(false, null,
                        JsonHelper.ParseJson<MessageResponse>(JsonHelper.SerializeObject(message))!));
                    response.BufferBlock.Complete();

                    response.Cache = "";
                    return;

                }

                //GPTState = GPTState.Generating;
                //if (!responseMessagesManager.TryGetValue(rawMessage.Id, out InternalMessageResponse? responseGet))
                //{
                //    responseGet = responseMessagesManager[""];
                //    //if (CurrentConversationId == "")
                //    //    CurrentConversationId = message.conversation_id;
                //}
                responseMessagesManager[rawMessage.Id].BufferBlock.Post(
                    new APIResponseMessage<MessageResponse>(false, null, message));
            }
        }

        public void ClearQueues()
        {
            requestManager.Complete();
            requestMessagesManager.Complete();

            ctsAllRequestManagers.Cancel();

            foreach (string key in manualIdGenericResponseManager.Keys)
                if (manualIdGenericResponseManager.Remove(key, out TaskCompletionSource<IResponse>? response))
                    if (response != null)
                        response.SetCanceled();
            foreach (string key in responseManager.Keys)
                if (responseManager.Remove(key, out TaskCompletionSource<string>? response))
                    if (response != null)
                        response.SetCanceled();
            foreach (string key in responseMessagesManager.Keys)
                if (responseMessagesManager.Remove(key, out InternalMessageResponse response))
                {
                    response.BufferBlock.Complete();
                    response.Cache = "";
                }

            requestManager = new BufferBlock<(string, ManualRequest, TaskCompletionSource<string>)>();
            requestMessagesManager =
                new BufferBlock<(string, MessageRequestRaw, BufferBlock<APIResponseMessage<MessageResponse>>)>();
            ctsAllRequestManagers = new CancellationTokenSource();
            manualIdGenericResponseManager = new ConcurrentDictionary<string, TaskCompletionSource<IResponse>?>();
            responseManager = new ConcurrentDictionary<string, TaskCompletionSource<string>?>();
            responseMessagesManager = new ConcurrentDictionary<string, InternalMessageResponse>();
        }

        void GetFixedId(IRequest request, ref string? id)
        {
            if (genericConditions != null)
                foreach (Func<IRequest, (bool, string?)> condition in genericConditions)
                {
                    (bool valid, string? fixedId) = condition(request);
                    if (valid && fixedId != null)
                    {
                        id = fixedId;
                        return;
                    }
                }

            id = null;
        }

        void GetManualId(IRequest request, ref string? id)
        {
            string[] split = request.Url.Split('#');
            if (split.Length == 1)
            {
                id = null;
                return;
            }
            id = request.Url.Split('#').Last();
        }

    }

    internal class InternalMessageResponse
    {
        public BufferBlock<APIResponseMessage<MessageResponse>> BufferBlock { get; }
        public string Cache { get; set; }

        public InternalMessageResponse(BufferBlock<APIResponseMessage<MessageResponse>> bufferBlock)
        {
            BufferBlock = bufferBlock;
            Cache = "";
        }
    }

    internal class BrowserManager
    {
        IBrowser browser;
        private IPage page;
        //Action displayBrowser;
        //Action hideBrowser;

        //MessageHandler messageHandler;
        //GenericResourceRequestHandler initializationHandler;

        //ActionBlock<MessageResponseRaw> processMessage;

        private long _browserState;
        public BrowserState BrowserState
        {
            get => (BrowserState)Interlocked.Read(ref _browserState);
            set { Interlocked.Exchange(ref _browserState, (long)value); }
        }

        private long _gptState;
        public GPTState GPTState
        {
            get => (GPTState)Interlocked.Read(ref _gptState);
            set { Interlocked.Exchange(ref _gptState, (long)value); }
        }

        public string CurrentConversationId { get; private set; }

        //public ConcurrentDictionary<int, ConversationsQuery> Conversations { get; private set; }
        //public event EventHandler<ConversationsLoadEventArgs>? ConversationsLoaded;
        //private ConcurrentDictionary<string, TaskCompletionSource<ConversationFullQuery>> conversationFullQueryManager;
        //private ConcurrentDictionary<string, InternalMessageResponse> responseMessagesManager;
        //private ConcurrentDictionary<string, TaskCompletionSource<Response>> responseManager;
        //private ConcurrentDictionary<string, (IRequest, TaskCompletionSource<Response>)> requestManager;
        private NetworkManager networkManager;
        private ConcurrentBag<CancellationTokenSource> initializationTokens;

        string authorizationToken;

        static readonly string RESERVED_AUTH_FIXEDID = "auth";
        static string[] dynamicHeaders = new string[]
    {
            "sec-ch-ua",
            "sec-ch-ua-mobile",
            "User-Agent",
            "sec-ch-ua-platform",
            "Accept-Language"
    };

        Dictionary<string, string> universalHeaders = new Dictionary<string, string>
        {
            {"Connection", "keep-alive" },
            {"sec-ch-ua", "" },
            {"Content-Type", "application/json" },
            {"sec-ch-ua-mobile", "" },
            {"Authorization", "" },
            {"User-Agent", "" },
            {"sec-ch-ua-platform", "" },
            {"Origin", "" },
            //"Accept" header is customized
            {"Accept", "" },
            {"Sec-Fetch-Site", "same-origin" },
            {"Sec-Fetch-Mode", "cors" },
            {"Sec-Fetch-Dest", "empty" },
            //{"Referer", "https://chat.openai.com/chat" },
            {"Accept-Encoding", "gzip, deflate, br" },
            {"Accept-Language", "" }
        };
        //static readonly string chatSpecificReferer = "https://chat.openai.com/chat/c/{0}";
        //static readonly string chatReferer = "https://chat.openai.com/chat/";
        static readonly string conversationsFeedUrl = "https://chat.openai.com/backend-api/conversations?offset={0}&limit={1}";
        static readonly string conversationLoadUrl = "https://chat.openai.com/backend-api/conversation/{0}";
        static readonly string conversationSendMessageUrl = "https://chat.openai.com/backend-api/conversation";
        static readonly Dictionary<string, string> conversationsFeedHeaders = new Dictionary<string, string>
        {
            {"Accept", "*/*" },
        };
        Dictionary<string, string> conversationLoadHeaders = new Dictionary<string, string>
        {
            {"Accept", "*/*" },
        };
        Dictionary<string, string> conversationMessageSendHeaders = new Dictionary<string, string>
        {
            {"Accept", "text/event-stream" },
            {"Origin", "https://chat.openai.com" },
        };

        private BrowserManager()
        {
            //processMessage = new ActionBlock<MessageResponseRaw>(ProcessMessage);
            CurrentConversationId = "";
            //responseMessagesManager =
            //    new ConcurrentDictionary<string, InternalMessageResponse>();
            initializationTokens = new ConcurrentBag<CancellationTokenSource>();
        }

        public static async Task<BrowserManager> Initialize()
        {
            BrowserManager manager = new BrowserManager();

            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            //this.displayBrowser = displayBrowser;
            //this.hideBrowser = hideBrowser;
            //manager.processMessage = new ActionBlock<MessageResponseRaw>(manager.ProcessMessage);

            //messageHandler = new MessageHandler(processMessage);
            //initializationHandler = new GenericResourceRequestHandler(InitializeOnLoad);
            //this.conversationFullQueryManager = new ConcurrentDictionary<string, TaskCompletionSource<ConversationFullQuery>>();
            //manager.responseMessagesManager = new ConcurrentDictionary<string, BufferBlock<MessageResponse>>();
            //this.responseManager = new ConcurrentDictionary<string, TaskCompletionSource<Response>>();
            //this.requestManager = new ConcurrentDictionary<string, (IRequest, TaskCompletionSource<Response>)>();
            //manager.initializationTokens = new ConcurrentBag<CancellationTokenSource>();

            //this.Conversations = new ConcurrentDictionary<int, ConversationsQuery>();
            //this.ConversationIdsOrdered = new ConcurrentDictionary<int, string>();
            //manager.CurrentConversationId = "";

            // Initialization plugin builder
            var extra = new PuppeteerExtra();

            // Use stealth plugin
            extra.Use(new StealthPlugin());

            // Launch the puppeteer browser with plugins
            manager.browser = await extra.LaunchAsync(new LaunchOptions()
            {
                Headless = false,
                UserDataDir = AppDomain.CurrentDomain.BaseDirectory + "\\ChromeUserData",
                DefaultViewport = null,
            });

            manager.page = await manager.browser.NewPageAsync();

            manager.networkManager = new NetworkManager(() => manager.page,
                //Generic handlers
                new List<Func<IRequest, (bool, string?)>> {
                    manager.ConversationsLoadInterceptCondition, manager.ConversationChangeInterceptCondition,
                    manager.AuthorizationInterceptCondition },
                //Specialized handlers
                new List<(Func<IRequest, bool>, Action<IRequest, string>)> {
                //(MessageInterceptCondition, messageHandler),
                (manager.InitializationInterceptCondition, manager.InitializeOnLoad)},
                manager.IsServiceAvailable);

            return manager;
        }

        public async Task Reinitialize(bool resetPage = true, bool clearQueues = true)
        {
            if (resetPage)
            {
                BrowserState = BrowserState.Uninitialized;
                GPTState = GPTState.IdleInChat;

                foreach (CancellationTokenSource cts in initializationTokens)
                    cts.Cancel();
                initializationTokens = new ConcurrentBag<CancellationTokenSource>();

                CurrentConversationId = "";
                //authorizationToken = "";

                await page.DisposeAsync();
                page = await browser.NewPageAsync();
            }

            if (clearQueues)
            {
                //foreach (var key in responseMessagesManager.Keys)
                //    if (responseMessagesManager.Remove(key, out InternalMessageResponse response))
                //        response.BufferBlock.Complete();

                //responseMessagesManager =
                //    new ConcurrentDictionary<string, InternalMessageResponse>();
                networkManager.ClearQueues();
                networkManager = new NetworkManager(() => page,
                    //Generic handlers
                    new List<Func<IRequest, (bool, string?)>>
                    {
                        ConversationsLoadInterceptCondition, ConversationChangeInterceptCondition,
                        AuthorizationInterceptCondition
                    },
                    //Specialized handlers
                    new List<(Func<IRequest, bool>, Action<IRequest, string>)>
                    {
                        //(MessageInterceptCondition, messageHandler),
                        (InitializationInterceptCondition, InitializeOnLoad)
                    },
                    IsServiceAvailable);
            }

            if (resetPage)
                await Load();
        }

        static APIResponseMessage<T> ParseData<T>(string data)
        {
            T? result = default(T);
            ResponseError? error = null;

            if (!JsonHelper.ParseJsonWithException(data, out result, out Exception? ex1/*, MissingMemberHandling.Ignore*/))
            {
                Debug.WriteLine(data);
                if (!JsonHelper.ParseJsonWithException(data, out error,
                        out Exception? ex2 /*, MissingMemberHandling.Ignore*/))
                {

                    throw new Exception(ex1!.Message + Environment.NewLine + ex2!.Message);
                }
                throw new Exception(error!.Detail);
            }

            APIResponseMessage<T> responseMessage = new APIResponseMessage<T>(error != null, error, result);

            return responseMessage;
        }

        bool IsServiceAvailable()
        {
            return BrowserState == BrowserState.Initialized;
        }

        //async Task ProcessAuthorization(IRequest request, string data)
        //{
        //    AuthorizationResponse? auth = JsonHelper.ParseJson<AuthorizationResponse>(data);
        //    if (auth == null)
        //        throw new Exception("AuthorizationResponse must not be null");
        //    accessToken = auth.accessToken;
        //    foreach (string header in dynamicHeaders)
        //    {
        //        if (request.Headers[header] == null)
        //            throw new Exception($"{header} is null!");
        //        conversationsFeedHeaders[header] = request.Headers[header];
        //    }
        //    await browser.CreateIFrame();
        //}

        //void ProcessMessage(MessageResponseRaw rawMessage)
        //{
        //    if (!responseMessagesManager.ContainsKey(rawMessage.Id))
        //        return;

        //    string[] datas = (responseMessagesManager[rawMessage.Id].Cache + rawMessage.Data)
        //        .Replace("\r\n\r\n", "\n\n")
        //        .Split("\n\n");
        //    for (int i = 0; i < datas.Length; i++)
        //    {
        //        string dataPackage = datas[i];

        //        //Signal that ChatGPT's response is completed
        //        if (dataPackage == "data: [DONE]")
        //        {
        //            //GPTState = GPTState.IdleInChat;
        //            //if (responseMessagesManager.TryRemove(rawMessage.Id, out BufferBlock<MessageResponse>? bb))
        //            //    bb.Complete();
        //            responseMessagesManager[rawMessage.Id].Cache = "";
        //            return;
        //        }

        //        string cleanData;
        //        if (dataPackage.StartsWith("data: "))
        //            cleanData = dataPackage.Substring(6);
        //        else
        //            cleanData = dataPackage;

        //        if (cleanData == "\n" || cleanData == "")
        //            continue;
        //        //Debug.WriteLine(Uri.EscapeUriString(cleanData) + Environment.NewLine + "---------------------------------");
        //        if (!JsonHelper.ParseJsonWithException(cleanData, out MessageResponse? message, out Exception ex))
        //        //Data is incomplete
        //        {
        //            if (JsonHelper.TryParseJson(cleanData, out ResponseError? error))
        //            {
        //                if (responseMessagesManager.TryRemove(rawMessage.Id,
        //                        out InternalMessageResponse? response))
        //                {
        //                    response.BufferBlock.Complete();
        //                    response.Cache = "";
        //                }
        //                Debug.WriteLine(error.detail);
        //                return;
        //            }
        //            if (ex != null)
        //            {
        //                Debug.WriteLine(cleanData);
        //                Debug.WriteLine(ex.Message);
        //            }

        //            //Skips irrelevant / faulty data package
        //            //if (i < datas.Length - 1)
        //            //    return;

        //            //Debug.WriteLine("Deserialization failed");

        //            if (responseMessagesManager[rawMessage.Id].Cache != "")
        //                Debug.WriteLine("responseMessageCache was faulty twice or more");
        //            if (i == datas.Length - 1)
        //                responseMessagesManager[rawMessage.Id].Cache = datas[datas.Length - 1];

        //            return;
        //        }
        //        else
        //            responseMessagesManager[rawMessage.Id].Cache = "";

        //        if (message == null)
        //            throw new Exception("Parsed MessageResponse is not supposed to be null");
        //        if (message.message == null)
        //            throw new Exception("Parsed MessageResponse.Message is not supposed to be null");

        //        //Check for message end of turn
        //        if (message.message.end_turn != null && message.message.end_turn == true
        //            && message.message.metadata.finish_details != null)
        //        {
        //            Debug.WriteLine("FINISHED");
        //            GPTState = GPTState.IdleInChat;
        //            if (!responseMessagesManager.TryRemove(message.conversation_id,
        //                    out InternalMessageResponse? response))
        //                if (!responseMessagesManager.TryRemove("", out response))
        //                    throw new Exception("Failed to signal completion to a message");

        //            message.message.metadata.finish_details = null;
        //            response.BufferBlock.Post(new APIResponseMessage<MessageResponse>(null,
        //                JsonHelper.ParseJson<MessageResponse>(JsonHelper.SerializeObject(message))!));
        //            response.BufferBlock.Complete();

        //            responseMessagesManager[rawMessage.Id].Cache = "";
        //            return;

        //        }

        //        GPTState = GPTState.Generating;
        //        if (!responseMessagesManager.TryGetValue(message.conversation_id, out InternalMessageResponse? responseGet))
        //        {
        //            responseGet = responseMessagesManager[""];
        //            if (CurrentConversationId == "")
        //                CurrentConversationId = message.conversation_id;
        //        }
        //        responseGet.BufferBlock.Post(new APIResponseMessage<MessageResponse>(null, message));
        //    }
        //}

        //void ProcessConversationsLoad(IRequest request, string data)
        //{
        //    Debug.WriteLine(data);

        //    ConversationsQuery? query = ParseData<ConversationsQuery>(data);

        //    if (query == null)
        //        throw new Exception("Parsed ConversationsQuery is not supposed to be null");

        //    //int i = 0;
        //    //foreach (Item item in query.items)
        //    //{
        //    //    if (!Conversations.TryAdd(item.id, item))
        //    //        if (DateTime.Compare(item.update_time, Conversations[item.id].update_time) > 0)
        //    //            Conversations[item.id] = item;

        //    //    ConversationIdsOrdered[i] = item.id;
        //    //    i++;
        //    //}

        //    int index = (int)Math.Ceiling((double)query.offset / (double)query.limit);
        //    Conversations[index] = query;
        //    OnConversationsLoaded(new ConversationsLoadEventArgs { Conversations = Conversations });
        //}

        //void ProcessConversationChange(IRequest request, string data)
        //{
        //    GPTState = GPTState.IdleInChat;
        //    ConversationFullQuery? query = ParseData<ConversationFullQuery>(data);

        //    if (query == null)
        //        throw new Exception("Parsed ConversationFullQuery is not supposed to be null");

        //    string conversationId = request.Url.Split('/').Last();
        //    CurrentConversationId = conversationId;

        //    if (conversationFullQueryManager.TryRemove(conversationId, out TaskCompletionSource<ConversationFullQuery>? tcs))
        //        tcs.SetResult(query);
        //}

        bool InitializationInterceptCondition(IRequest request)
        {
            //Debug.WriteLine(request.Url);
            //Debug.WriteLine(request.Url == "https://chat.openai.com/");
            bool valid = request.Url == "https://chat.openai.com/" ||
                (BrowserState != BrowserState.Initializing &&
                request.Url.StartsWith("https://chat.openai.com/auth/login"));
            return valid;
        }
        (bool, string?) AuthorizationInterceptCondition(IRequest request)
        {
            bool valid = request.Url == "https://chat.openai.com/api/auth/session";
            return (valid, RESERVED_AUTH_FIXEDID);
        }

        bool MessageInterceptCondition(IRequest request)
        {
            if (request.Url.Split('#')[0] == "https://chat.openai.com/backend-api/conversation")
                return true;
            return false;
        }

        (bool, string?) ConversationsLoadInterceptCondition(IRequest request)
        {
            bool valid = request.Url.StartsWith("https://chat.openai.com/backend-api/conversations") &&
                (new Uri(request.Url).Segments.Length == 3);
            return (valid, null);
        }

        (bool, string?) ConversationChangeInterceptCondition(IRequest request)
        {
            bool valid = request.Url.StartsWith("https://chat.openai.com/backend-api/conversation/") &&
                (new Uri(request.Url).Segments.Length == 4);
            return (valid, null);
        }

        //protected virtual void OnConversationsLoaded(ConversationsLoadEventArgs e)
        //{
        //    Debug.WriteLine("UPDATED CONVERSATIONS");
        //    ConversationsLoaded?.Invoke(this, e);
        //}

        async Task<bool> IsBrowserBlocked()
        {
            //Debug.WriteLine("On different address: " + !browser.Address.StartsWith("https://chat.openai.com/chat"));
            return await page.IsCloudflareBlocked() ||
                await page.IsSignInRequired()/* || !browser.Address.StartsWith("https://chat.openai.com/chat")*/;
        }

        void InitializeOnLoad(IRequest request, string data)
        {
            Debug.WriteLine("Initializing");
            foreach (CancellationTokenSource ts in initializationTokens)
                ts.Cancel();
            initializationTokens.Clear();
            CancellationTokenSource tsNew = new CancellationTokenSource();
            initializationTokens.Add(tsNew);
            InitializeInternal(tsNew.Token);
        }

        async Task InitializeInternal(CancellationToken ct, int milisecsPerLoading = 1500/*, TimeSpan? timeout = null*/)
        {
            BrowserState = BrowserState.Initializing;

            //In advance, prepare the response manager for the session authorization load

            //int mainframeLoadCount = 0;
            //EventHandler<FrameLoadEndEventArgs> MainframeLoaded = (sender, args) =>
            //{
            //    //Wait for the MainFrame to finish loading
            //    if (args.Frame.IsMain)
            //        mainframeLoadCount++;
            //};

            //browser.FrameLoadEnd += MainframeLoaded;

            int msWaited = 0;
            //int oldCount = mainframeLoadCount;

            //while (oldCount == mainframeLoadCount)
            //    await Task.Delay(milisecsPerLoading);

            Task<BrowserVerificationStatus> browserVerification = VerifyBrowser(ct, milisecsPerLoading/*, timeout*/);

            IResponse sessionAuthResult;
            while (true)
            {
                TaskCompletionSource<IResponse> tcs = new TaskCompletionSource<IResponse>();
                networkManager.HookResponse(RESERVED_AUTH_FIXEDID, tcs);
                Task<IResponse> t = tcs.Task.WaitAsync(ct);
                //t.ContinueWith(t => Console.WriteLine("TASK COMPLETED"));

                try
                {
                    sessionAuthResult = await t;
                    Debug.WriteLine("SUCCESSFULLY RECEIVED AUTH SESSION");
                    if (sessionAuthResult.Ok)
                        break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("CANCELLED");
                    return;
                }
            }

            Debug.WriteLine(await sessionAuthResult.TextAsync());
            //AuthorizationResponse? auth = JsonHelper.ParseJson<AuthorizationResponse>(
            //Encoding.UTF8.GetString(sessionAuthResult.data));
            JObject auth = JObject.Parse(await sessionAuthResult.TextAsync());

            if (auth == null)
                throw new Exception("AuthorizationResponse must not be null");
            authorizationToken = auth["accessToken"].Value<string>();
            //accessToken = auth.accessToken;
            foreach (string header in dynamicHeaders)
            {
                if (sessionAuthResult.Request.Headers[header] == null)
                    throw new Exception($"{header} is null!");
                universalHeaders[header] = sessionAuthResult.Request.Headers[header];
            }
            Debug.WriteLine(authorizationToken);

            if (await browserVerification == BrowserVerificationStatus.UnhandledException)
            {
                Debug.WriteLine("ERROR FOUND, reinitializing");
                IPage old = page;
                page = await browser.NewPageAsync();
                //await browser.WaitForInitialLoadAsync();
                old.Dispose();
                await Reinitialize(true, true);
                await Load();
                return;
            }

            Debug.WriteLine("Waiting for conversations update");
            ////Wait until Conversations populates
            ////while (Conversations.Count == 0)
            ////{
            ////    await Task.Delay(milisecsPerLoading);
            ////    msWaited += milisecsPerLoading;
            ////    if (timeout != null && msWaited > timeout.Value.TotalMilliseconds)
            ////        throw new Exception("Timeout. Could not initialize ChatGPT.");
            ////}
            //(IRequest request, string data) = await tcs.Task;
            //ConversationsQuery? query = ParseData<ConversationsQuery>(data);
            //if (query == null)
            //    throw new Exception("Cannot parse the first ConversationsQuery upon intialization");

            //Resolved
            BrowserState = BrowserState.Initialized;
            Debug.WriteLine("Done!");
            Debug.WriteLine("Total miliseconds taken: " + msWaited);

            //return query;
        }

        async Task<BrowserVerificationStatus> VerifyBrowser(CancellationToken cancellationToken, int milisecsPerLoading = 1500/*, TimeSpan? timeout = null*/)
        {
            while (true)
            {
                try
                {
                    IElementHandle asyncNav = await page.WaitForSelectorAsync("div")
.WaitAsync(cancellationToken);

                    //await page.EvaluateExpressionAsync("alert(1)");
                    await Task.Delay(milisecsPerLoading);
                    Debug.WriteLine("asyncNav: " + asyncNav != null);
                    //Debug.WriteLine(browser.CanExecuteJavascriptInMainFrame);

                    if (cancellationToken.IsCancellationRequested)
                        return BrowserVerificationStatus.Cancelled;
                    if (asyncNav != null)
                        break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            Debug.WriteLine("PASSED NAVIGATION CHECK");

            bool wasBlocked = false;
            //int msWaited = 0;
            try
            {
                while (!await page.IsServiceLoaded())
                {
                    Debug.WriteLine("CP 1");
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("VerifyBrowser canceled by token");
                        if (wasBlocked)
                            HideBrowser();
                        return BrowserVerificationStatus.Cancelled;
                    }
                    Debug.WriteLine("CP 2");

                    Debug.WriteLine("Waiting");
                    if (wasBlocked == false && await IsBrowserBlocked())
                    //The user will have to resolve the blockage themselves
                    {
                        wasBlocked = true;
                        DisplayBrowser();
                    }
                    Debug.WriteLine("CP 3");

                    Debug.WriteLine("CP 4");

                    Debug.WriteLine("CP 5");

                    //await Task.Delay(milisecsPerLoading);
                    //msWaited += milisecsPerLoading;
                    Debug.WriteLine("CP 6");
                    await Task.Delay(milisecsPerLoading);
                    //msWaited += milisecsPerLoading;
                    //if (timeout != null && msWaited > timeout.Value.TotalMilliseconds)
                    //{
                    //    Debug.WriteLine("TIMEOUT!");
                    //    return BrowserVerificationStatus.Timeout;
                    //}
                    Debug.WriteLine("CP 7");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("BrowserVerificationStatus.UnhandledException");
                return BrowserVerificationStatus.UnhandledException;
            }

            //Proceed the introduction / disclaimer popup when first logged in.
            while (await page.IsNextBtnDisclaimerShown())
            {
                Debug.WriteLine("Clicking next");
                await page.ClickNextOnDisclaimer();
                await Task.Delay(milisecsPerLoading);
            }
            await Task.Delay(milisecsPerLoading);
            if (await page.IsDoneBtnDisclaimerShown())
                await page.ClickDoneOnDisclaimer();

            Debug.WriteLine("CP 8");
            if (wasBlocked)
                HideBrowser();
            Debug.WriteLine("wasBlocked: " + wasBlocked);
            Debug.WriteLine("DONE VERIFYING");

            return BrowserVerificationStatus.OK;
        }

        void DisplayBrowser()
        {

        }

        void HideBrowser()
        {

        }

        public async Task Load()
        {
            await page.GoToAsync("https://chat.openai.com/");
            //await Task.Delay(6000);
            //await page.ScreenshotAsync(".\\screenshot.jpg");
        }

        ManualRequest CreateRequest(string method, string url, Dictionary<string, string> headerDict, string? postData = null)
        {
            ManualRequest request = new ManualRequest(method, url, new Dictionary<string, string>(), postData);

            foreach (var kvp in universalHeaders)
                request.Headers[kvp.Key] = kvp.Value;
            foreach (var kvp in headerDict)
                request.Headers[kvp.Key] = kvp.Value;
            //NameValueCollection headers = MiscHelper.GetHeaders(universalHeaders, headerDict);
            request.Headers["Authorization"] = "Bearer " + authorizationToken;

            if (postData != null)
                request.PostData = postData;

            return request;
        }

        Task<string> SendRequest(ManualRequest request)
        {
            //var frame = browser.GetMainFrame();
            //IRequest request = CreateRequest(frame, method, url, headerDict, data);
            TaskCompletionSource<string>? tcs = new TaskCompletionSource<string>();
            networkManager.EnqueueRequest(request, tcs);
            return tcs.Task;
        }

        void SendRequestWithoutResponse(ManualRequest request)
        {
            //var frame = browser.GetMainFrame();
            //IRequest request = CreateRequest(frame, method, url, headerDict, data);
            TaskCompletionSource<string>? tcs = null;
            networkManager.EnqueueRequest(request, tcs);
        }

        //Task<Response>? SendGetRequest(string url, Dictionary<string, string> headerDict, bool withResponse = true)
        //{
        //    //var frame = browser.GetMainFrame();
        //    //IRequest request = CreateRequest(frame, "GET", url, headerDict);
        //    TaskCompletionSource<Response>? tcs = null;
        //    if (withResponse)
        //        tcs = new TaskCompletionSource<Response>();
        //    networkManager.EnqueueRequest("GET", url, headerDict, null, tcs);
        //    return tcs == null ? null : tcs.Task;
        //}

        //Task<Response>? SendPostRequest(string url, Dictionary<string, string> headerDict, string data, bool withResponse = true)
        //{
        //    //var frame = browser.GetMainFrame();
        //    //IRequest request = CreateRequest(frame, "POST", url, headerDict);
        //    TaskCompletionSource<Response>? tcs = null;
        //    if (withResponse)
        //        tcs = new TaskCompletionSource<Response>();
        //    networkManager.EnqueueRequest("POST", url, headerDict, data, tcs);
        //    return tcs == null ? null : tcs.Task;
        //}


        public async Task<APIResponseMessage<ConversationsQuery>> FeedConversations(int offset, int limit)
        {
            Debug.WriteLine("FeedConversations Pass 1");
            string response = await SendRequest(CreateRequest(
                "GET",
                string.Format(conversationsFeedUrl, offset, limit),
                conversationsFeedHeaders));

            APIResponseMessage<ConversationsQuery> responseMessage = ParseData<ConversationsQuery>(response);
            Debug.WriteLine("FeedConversations Pass 2");
            //if (responseMessage.Error != null || responseMessage.Response == null)
            //    throw new Exception("ConversationsQuery is null when deserialized");

            return responseMessage;

            //if (await browser.IsWindowCompressed())
            //    await browser.ClickExpandWindow();

            //if (await browser.IsShowMoreBtnShown())
            //    await browser.ClickShowMore();
        }

        public async Task<APIResponseMessage<ConversationFullQuery>> LoadConversation(string conversationId)
        {
            string response = await SendRequest(CreateRequest(
                "GET",
            string.Format(conversationLoadUrl, conversationId),
                conversationLoadHeaders));

            APIResponseMessage<ConversationFullQuery> responseMessage = ParseData<ConversationFullQuery>(response);
            //if (query == null)
            //    throw new Exception("ConversationFullQuery is null when deserialized");

            return responseMessage;
        }

        MessageRequest CreateMessageRequest(string message, string? conversationId, string? parentMessageId, bool regenerate = false)
        {
            MessageRequest request = new MessageRequest
            {
                action = regenerate ? "variant" : "next",
                messages = new List<MessageRequest.Message> {
                    new MessageRequest.Message
                    {
                        author = new MessageRequest.Author { role = "user" },
                        content = new MessageRequest.Content
                        {
                            content_type = "text",
                            parts = new List<string> {message}
                        },
                        id = MiscHelper.GetRandomId(),
                    }
                },
                conversation_id = conversationId,
                parent_message_id = parentMessageId == null ? MiscHelper.GetRandomId() : parentMessageId,
                model = "text-davinci-002-render-sha",
                timezone_offset_min = -420,
                variant_purpose = "none"
            };
            return request;
        }

        public BufferBlock<APIResponseMessage<MessageResponse>> SendMessage(
            string message, string? conversationId, string? parentMessageId, bool regenerate = false)
        {
            BufferBlock<APIResponseMessage<MessageResponse>> bb = new BufferBlock<APIResponseMessage<MessageResponse>>();
            if (conversationId == null)
            {
                //Sending message in a new conversation
                GPTState = GPTState.IdleNewChat;
                //responseMessagesManager[""] = bb;
                CurrentConversationId = "";
            }
            //else
            //    //Sending message in an existing conversation
            //    responseMessagesManager[conversationId] = bb;

            MessageRequest request = CreateMessageRequest(message, conversationId,
                parentMessageId, regenerate);
            string requestJson = JsonHelper.SerializeObject(request);

            Debug.WriteLine(requestJson);

            //SendRequestWithoutResponse(new ManualRequest(
            //    "POST",
            //    conversationSendMessageUrl,
            //    conversationMessageSendHeaders,
            //    requestJson));

            networkManager.EnqueueMessage(authorizationToken, requestJson, bb);

            return bb;
        }

        //public async Task CreateNewChat()
        //{
        //    GPTState = GPTState.IdleNewChat;
        //    CurrentConversationId = "";
        //    await browser.CreateNewChat();
        //}

        //private async void ChangeConversationInternal(int index)
        //{
        //    if (await browser.IsWindowCompressed())
        //        await browser.ClickExpandWindow();
        //    await browser.SelectChat(index);
        //}

        //public Task<ConversationFullQuery> ChangeConversation(string conversationId)
        //{
        //    //long dateTime = Conversations[conversationId].create_time.Ticks;
        //    //int index = 0;
        //    //foreach (Item item in Conversations.Values)
        //    //    if (dateTime < item.create_time.Ticks)
        //    //        index++;

        //    int index = 0;
        //    foreach (ConversationsQuery query in Conversations.Values)
        //    {
        //        bool done = false;
        //        foreach (Item item in query.items)
        //            if (item.id == conversationId)
        //            {
        //                done = true;
        //                break;
        //            }
        //            else
        //                index++;
        //        if (done)
        //            break;
        //    }

        //    TaskCompletionSource<ConversationFullQuery> tcs = new TaskCompletionSource<ConversationFullQuery>();
        //    conversationFullQueryManager[conversationId] = tcs;

        //    ChangeConversationInternal(index);
        //    return tcs.Task;
        //}

        //private async Task<bool> SendMessageInternal(string message)
        //{
        //    await browser.TypeMessage(message);
        //    if (!await browser.IsMessageTyped(message))
        //        return false;
        //    Debug.WriteLine("TYPED MESSAGE");
        //    await Task.Delay(1500);
        //    await browser.SendMessage();
        //    if (!await browser.IsMessageSent(message))
        //        return false;
        //    Debug.WriteLine("SENT MESSAGE");
        //    return true;
        //}

        //public async Task<(bool sent, BufferBlock<MessageResponse>? bufferBlock)> SendMessage(string message)
        //{
        //    if (await SendMessageInternal(message))
        //    {
        //        BufferBlock<MessageResponse> bb = new BufferBlock<MessageResponse>();
        //        responseMessagesManager[CurrentConversationId] = bb;
        //        return (true, bb);
        //    }
        //    else
        //        return (false, null);
        //}

        //public async Task<(string messageId, string parentMessageId,
        //    BufferBlock<MessageResponse>? bufferBlock)> SendMessage(string message, string? conversationId = null)
        //{

        //}

        //public async Task StopGenerating()
        //{
        //    if (await browser.IsGenerating())
        //        await browser.ClickGenerationBtn();
        //}

        //public async Task<(bool sent, BufferBlock<MessageResponse>? bufferBlock)> Regenerate()
        //{
        //    if (await browser.IsAbleToRegenerate())
        //    {
        //        await browser.ClickGenerationBtn();
        //        BufferBlock<MessageResponse> bb = new BufferBlock<MessageResponse>();
        //        responseMessagesManager[CurrentConversationId] = bb;
        //        return (true, bb);
        //    }
        //    else
        //        return (false, null);
        //}
    }
}
