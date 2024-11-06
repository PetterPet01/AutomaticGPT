using AutomaticGPTPupTest1.FreeGPT.Models.Web;
using Cyotek.Collections.Generic;
using PetterPet.FreeGPT.API;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using VoskAutomationTest1.VoskAutomation;

namespace AutomaticGPTPupTest1
{
    enum ModeState
    {
        Uninitialized,
        Standby,
        ListeningCommand,
        ListeningPrompt,
        Processing
    }

    enum ChatMode
    {
        Continue,
        NewConversation
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //public class LifespanHandler : ILifeSpanHandler
        //{
        //    bool ILifeSpanHandler.DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        //    {
        //        Debug.WriteLine("LifespanHandler: Closing");

        //        return true;
        //    }

        //    bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl,
        //         IBrowser browser, IFrame frame, string targetUrl,
        //         string targetFrameName, WindowOpenDisposition targetDisposition,
        //         bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
        //         IBrowserSettings browserSettings, ref bool noJavascriptAccess,
        //         out IWebBrowser newBrowser)
        //    {

        //        Debug.WriteLine("LifespanHandler: POPUP!");

        //        //stop open popup
        //        newBrowser = null;
        //        return true;
        //    }

        //    void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        //    {
        //        Debug.WriteLine("LifespanHandler: OnBeforeClose");
        //    }

        //    void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        //    {
        //        Debug.WriteLine("LifespanHandler: OnAfterCreated");
        //    }
        //}


        static readonly string cnnvadModelPath = @"D:\-Projects-\CNNVAD-Sharp\frozen_without_dropout.pb";
        static readonly string voskModelPath = @"D:\-Projects-\AutomaticTranscriber\vosk-model-en-us-0.22-lgraph";
        static readonly float secsOfSilence = 0.2f;

        static readonly string wakePhrase = "wake up";
        static readonly Dictionary<string, ChatMode> chatModeLookup = new Dictionary<string, ChatMode>
        {
            { "continue", ChatMode.Continue },
            { "new conversation", ChatMode.NewConversation }
        };


        AutomaticVosk automaticVosk;
        CircularBuffer<bool> arbitraryPredicionsBuffer = new CircularBuffer<bool>((int)(secsOfSilence / 0.0125f));

        BrowserManager manager;
        List<ConversationsQuery> conversations;
        string? currentConvoId;
        string? lastIdInConvo;

        //FaceShakeDetection fsd;
        ModeState state;
        ChatMode chatMode;

        // default is false, set 1 for true.
        private int _threadSafeBoolBackValue = 0;

        public bool IsProcessing
        {
            get { return (Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1); }
            set
            {
                if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);
                else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
            }
        }

        void OnSpeechDetected(object? sender, bool e)
        {
            button1.BeginInvoke(() =>
            {
                button1.Text = e.ToString();
                button1.BackColor = e ? Color.Green : Color.Red;
            });
        }

        void OnSpeechRecognized(object? sender, string e)
        {
            Debug.WriteLine(e);
            if (state == ModeState.ListeningPrompt)
                textBox1.BeginInvoke(() =>
                {
                    textBox1.Text = e;
                    textBox1.BackColor = Color.LightGoldenrodYellow;
                });
        }

        void OnSpeechEnded(object? sender, string e)
        {
            Debug.WriteLine("<---------ENDED-------->");
            Debug.WriteLine(e);
            Debug.WriteLine("<---------ENDED-------->");
            if (state == ModeState.ListeningPrompt)
                textBox1.BeginInvoke(() =>
                {
                    textBox1.Text = "";
                    textBox1.BackColor = Color.White;
                });

            ProcessSpeech(e);
        }

        bool headShook = false;

        async Task ProcessSpeech(string speech)
        {
            Debug.WriteLine(IsProcessing);
            if (IsProcessing)
                return;
            IsProcessing = true;
            Debug.WriteLine(state);
            try
            {
                switch (state)
                {
                    case ModeState.Standby:
                        {
                            if (manager.BrowserState == BrowserState.Initialized && speech.ToLower() == wakePhrase)
                            {
                                if (conversations.Count == 0)
                                {
                                    await FeedMore();
                                    ChangeConversation(conversations[0].items[0].id);
                                }
                                Debug.WriteLine($"Wake command received, service availability: {manager.BrowserState}");
                                if (state != ModeState.Processing)
                                    state = ModeState.ListeningPrompt;
                            }
                            break;
                        }
                    //case ModeState.ListeningCommand:
                    //    {
                    //        if (chatModeLookup.TryGetValue(speech.ToLower(), out chatMode))
                    //            state = ModeState.ListeningPrompt;
                    //        break;
                    //    }
                    case ModeState.ListeningPrompt:
                        {
                            //await Task.Delay(5000);
                            //if (headShook)
                            //{
                            //    headShook = false;
                            //    return;
                            //}
                            if (!chatModeLookup.TryGetValue(speech.ToLower(), out chatMode))
                                chatMode = ChatMode.Continue;
                            else if (chatMode == ChatMode.NewConversation)
                            {
                                ClearConversationsUI();
                                break;
                            }

                            state = ModeState.Processing;
                            string mes = await SendMessage(chatMode == ChatMode.NewConversation, speech);
                            IsProcessing = false;
                            Task.Factory.StartNew(mes.Speak);

                            state = ModeState.Standby;
                            break;
                        }
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        async Task<string> ProcessChat(ISourceBlock<APIResponseMessage<MessageResponse>> source)
        {
            string fullText = "";
            while (await source.OutputAvailableAsync())
            {
                APIResponseMessage<MessageResponse> response = await source.ReceiveAsync();
                if (response.Response == null)
                    throw new Exception("Cannot process a null MessageResponse");
                await Task.Delay(10);
                chatControl1.BeginInvoke(() =>
                {
                    fullText = "";
                    foreach (string text in response.Response.message.content.parts)
                        fullText += text.Trim().Trim('\n');
                    chatControl1[chatControl1.Count - 1].Text = fullText;
                });
                if (response.Response.conversation_id == null)
                    throw new Exception("response.conversation_id is null");
                this.currentConvoId = response.Response.conversation_id;
                if (response.Response.message == null || response.Response.message.id == null)
                    throw new Exception("response.message or response.message.id is null");
                this.lastIdInConvo = response.Response.message.id;
            }

            return fullText;
        }

        async Task Initialize()
        {
            manager = await BrowserManager.Initialize();
            await manager.Load();
            //ProcessConversations(await );
            Debug.WriteLine("Hello");
            state = ModeState.Standby;
            statusUpdater.Start();
        }

        async void ChangeConversation(string id)
        {
            chatControl1.RemoveChatItems();

            APIResponseMessage<ConversationFullQuery> convo = await manager.LoadConversation(id);
            if (convo.Response == null)
                throw new Exception("Cannot change conversation");
            string firstId = convo.Response.mapping.Values.First().id;
            while (convo.Response.mapping[firstId].parent != null)
                firstId = convo.Response.mapping[firstId].parent;

            string currentId = firstId;
            for (int i = 0; i < 2; i++)
                currentId = convo.Response.mapping[currentId].children[0];
            while (true)
            {
                ConversationFullQuery.MessageData item = convo.Response.mapping[currentId];
                ChatItemType type = item.message.author.role == "user" ? ChatItemType.Sender : ChatItemType.Receiver;
                string message = string.Join(null, item.message.content.parts);
                chatControl1.BeginInvoke(() =>
                chatControl1.AddChatItem(message, type).MultiNode = convo.Response.mapping[currentId].children.Count > 1);

                if (convo.Response.mapping[currentId].children.Count == 0)
                    break;
                else
                    currentId = convo.Response.mapping[currentId].children[0];
            }

            this.currentConvoId = id;
            this.lastIdInConvo = currentId;
        }

        async Task FeedMore()
        {
            APIResponseMessage<ConversationsQuery> query;
            int offset = 0;
            int limit = 20;
            if (conversations.Count == 0)
            {
                Debug.WriteLine("FeedMore Pass 1");
                query = await manager.FeedConversations(0, 20);
                if (query.Response == null)
                    throw new Exception("Cannot feed more conversations");
            }
            else
            {
                Debug.WriteLine("FeedMore Pass 2");
                var convo = conversations.Last();
                offset = convo.offset;
                //limit = convo.limit;
                query = await manager.FeedConversations(convo.offset + convo.items.Count, /*convo.limit*/ limit);
                Debug.WriteLine("FeedMore Pass 3");

                if (query.Response == null)
                    throw new Exception("Cannot feed more conversations");

                bool needsFullRefresh = (query.Response.total != convo.total);
                if (!needsFullRefresh)
                    foreach (Item item1 in convo.items)
                    {
                        foreach (Item item2 in query.Response.items)
                            if (item1.id != item2.id)
                            {
                                needsFullRefresh = true;
                                break;
                            }
                        if (needsFullRefresh)
                            break;
                    }
                Debug.WriteLine("FeedMore Pass 4");

                if (needsFullRefresh)
                {
                    conversations.Clear();
                    for (int i = 0; i < query.Response.offset; i += limit)
                    {
                        APIResponseMessage<ConversationsQuery> tmpQuery =
                            await manager.FeedConversations(i, limit);
                        if (tmpQuery.Response == null)
                            throw new Exception("Cannot feed more conversations");
                        conversations.Add(tmpQuery.Response);
                    }
                }
                Debug.WriteLine("FeedMore Pass 5");
            }
            conversations.Add(query.Response);
            Debug.WriteLine($"Fed conversations at offset {offset}, limit {limit}");
            Debug.WriteLine("FeedMore Pass 6");
        }

        void ClearConversationsUI()
        {
            if (chatControl1.InvokeRequired)
                chatControl1.BeginInvoke(() => chatControl1.RemoveChatItems());
        }

        async Task<string> SendMessage(bool newConversation, string message)
        {
            BufferBlock<APIResponseMessage<MessageResponse>> bufferBlock;

            chatControl1.BeginInvoke(() =>
            {
                chatControl1.AddChatItem(message, ChatItemType.Sender);
                if (chatControl1[chatControl1.Count - 1].Text != "")
                    chatControl1.AddChatItem("", ChatItemType.Receiver);
            });

            if (newConversation)
                bufferBlock = manager.SendMessage(message, null, null);
            else
                bufferBlock = manager.SendMessage(message, currentConvoId, lastIdInConvo);

            return await ProcessChat(bufferBlock);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            automaticVosk = new AutomaticVosk(cnnvadModelPath, voskModelPath, secsOfSilence);
            automaticVosk.SpeechDetected += OnSpeechDetected;
            automaticVosk.SpeechRecognized += OnSpeechRecognized;
            automaticVosk.SpeechEnded += OnSpeechEnded;

            conversations = new List<ConversationsQuery>();

            Initialize();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            automaticVosk.Dispose();
        }

        bool isBrowser = false;
        //private async void execute()
        //{
        //    await chromiumWebBrowser1.GetMainFrame().EvaluateScriptAsync("document.body.querySelectorAll('input')[1].value = 'hominhquan2006@gmail.com'");
        //}
        private void button3_Click(object sender, EventArgs e)
        {
            //chromiumWebBrowser1.EvaluateScriptAsync(richTextBox1.Text);
            //isBrowser = !isBrowser;
            //if (isBrowser)
            //    chromiumWebBrowser1.BringToFront();
            //else
            //    chromiumWebBrowser1.SendToBack();
            //manager.ExecuteScript("document.body.querySelectorAll('input')[1].value = 'hominhquan2006@gmail.com'");
            //chromiumWebBrowser1.ExecuteScriptAsync("alert(1)");
            //JavascriptResponse result = chromiumWebBrowser1.EvaluateScriptAsync("alert(1)")
            //    .Result;
            //Debug.WriteLine(result.Message);

            SystemSpeechExtension.Stop();
        }

        private void statusUpdater_Tick(object sender, EventArgs e)
        {
            statusBrowserState.Text = manager.BrowserState.ToString();
            statusGPTState.Text = manager.GPTState.ToString();
            statusState.Text = state.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendMessage(false, textBox1.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //FFTSManager.LoadAppropriateDll();
            //        fsd = new FaceShakeDetection(@"RFB-320.bin",
            //@"RFB-320.param",
            //@"shape_predictor_68_face_landmarks.dat");
            //        fsd.HeadShakeDetected += (object? sender, EventArgs e) =>
            //        {
            //            Debug.WriteLine("SHOOK");
            //            headShook = true;
            //        };
            //chromiumWebBrowser1.LoadUrl("https://chat.openai.com/auth/login");
            //chromiumWebBrowser1.LifeSpanHandler = new LifespanHandler();
        }

        //async Task execute2()
        //{
        //    try
        //    {
        //        ChromiumWebBrowser old = chromiumWebBrowser1;
        //        chromiumWebBrowser1 = new ChromiumWebBrowser("https://chat.openai.com/auth/login");
        //        await chromiumWebBrowser1.WaitForInitialLoadAsync();
        //        old.Dispose();
        //        Debug.WriteLine("Initialized");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("Browser identifier: " + chromiumWebBrowser1.GetBrowser().Identifier);
            //execute2().Wait();
        }
    }
}