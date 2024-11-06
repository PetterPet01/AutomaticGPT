namespace AutomaticGPTPupTest1
{
    public partial class ChatControl : UserControl
    {
        private List<ChatItem> chatItems = new List<ChatItem>();

        public int Count { get => chatItems.Count; }
        public ChatItem this[int i]
        {
            get { return chatItems[i]; }
            set { chatItems[i] = value; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }
        public ChatItem AddChatItem(string text, ChatItemType type)
        {
            ChatItem chatItem = new ChatItem();
            chatItem.Dock = DockStyle.Bottom;
            chatItem.AutoSize = true;
            chatItem.Text = text;
            chatItem.ItemType = type;
            chatItem.ResizeNormally(Width);
            chatItems.Add(chatItem);
            Controls.Add(chatItem);
            //Resize += ChatControl_Resize; // Add event handler for SizeChanged event
            if (this.AutoScrollMinSize.Width != this.ClientSize.Width)
                this.AutoScrollMinSize = new Size(this.ClientSize.Width, 0);
            //this.AutoScrollPosition = new Point(0, this.DisplayRectangle.Height);

            return chatItem;
        }

        public void HighlightItem(int index)
        {
            int count = chatItems.Count;

            for (int i = 0; i < count; i++)
                if (chatItems[i].LabelBackColor == Color.Orange)
                    chatItems[i].SetLabelColor(i % 2 == 0 ? Color.AliceBlue : Color.LawnGreen);

            chatItems[index].SetLabelColor(Color.Orange);
            this.ScrollToControl(chatItems[index]);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (this.AutoScrollMinSize.Width != this.ClientSize.Width)
                this.AutoScrollMinSize = new Size(1, 0);
            this.AutoScrollPosition = new Point(0, this.DisplayRectangle.Height);
        }

        protected override void OnResize(EventArgs e)
        {
            SuspendLayout();
            // Adjust width of ChatItem controls when ChatControl is resized
            foreach (var chatItem in chatItems)
            {
                chatItem.ResizeNormally(Width);
            }
            ResumeLayout();
            //if (this.AutoScrollMinSize.Width != this.ClientSize.Width)
            //    this.AutoScrollMinSize = new Size(this.ClientSize.Width, 0);
            //this.AutoScrollPosition = new Point(0, this.DisplayRectangle.Height);
        }

        public void RemoveChatItem(ChatItem item)
        {
            Controls.Remove(item);
            item.Dispose();
        }

        public void RemoveChatItems()
        {
            foreach (var chatItem in chatItems)
            {
                Controls.Remove(chatItem);
                chatItem.Dispose();
            }
            chatItems.Clear();
        }
    }

    public enum ChatItemType
    {
        Sender,
        Receiver
    }

    public class ChatItem : UserControl
    {
        //private const int MaxWidth = 300; // Maximum width of ChatItem

        private Label labelText;
        private Switcher button;

        public Color LabelBackColor { get => labelText.BackColor; }

        public bool MultiNode { get; set; } = false;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public ChatItem()
        {
            InitializeComponent();
            //SizeChanged += (object? sender, EventArgs e) => OnResize(e);
        }

        public void SetLabelColor(Color color)
        {
            labelText.BackColor = color;
        }

        public string Text
        {
            get { return labelText.Text; }
            set { labelText.Text = value; }
        }

        public ChatItemType ItemType { get; set; }

        private void InitializeComponent()
        {
            labelText = new Label();
            button = new Switcher();

            SuspendLayout();

            // labelText
            labelText.AutoSize = true;
            //labelText.MaximumSize = new Size(Parent.Width, 0);
            labelText.Margin = new Padding(5);
            labelText.Padding = new Padding(10);
            labelText.BackColor = Color.White;
            labelText.BorderStyle = BorderStyle.FixedSingle;
            labelText.Resize += (object? sender, EventArgs e) =>
            {
                button.Location = new Point(0, labelText.Height + 5);
            };

            // button
            button.Text = "Button";
            button.Margin = new Padding(5);
            button.Visible = false;
            button.Font = new Font("Arial", 8);
            button.Text = "Delete";
            //button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button.Location = new Point(0, labelText.Height + 5);

            // Add an event handler for the delete button

            // Add the delete button to the control
            this.Controls.Add(button);

            // Position button within the control
            button.Size = new Size(50, 20);

            // ChatItem
            AutoSize = true;
            Controls.Add(button);
            Controls.Add(labelText);

            ResumeLayout();
        }

        public void ResizeNormally(int width)
        {
            labelText.MaximumSize = new Size(width - 20, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Resize label width to match ChatItem width
            labelText.Width = Width - 20;

            // Show/hide button based on ChatItemType
            if (ItemType == ChatItemType.Sender)
            {
                //labelText.TextAlign = ContentAlignment.MiddleRight;
                labelText.BackColor = Color.AliceBlue;
            }
            else
            {
                //labelText.TextAlign = ContentAlignment.MiddleLeft;
                labelText.BackColor = Color.LawnGreen;
            }
            button.Visible = MultiNode;
        }
    }

    public class Switcher : UserControl
    {
        private Button btnLeft;
        private Label lblMiddle;
        private Button btnRight;

        public event EventHandler? LeftClick;
        public event EventHandler? RightClick;

        private int currentNumber = 0;
        public int CurrentNumber
        {
            get { return currentNumber; }
            set
            {
                currentNumber = value;
                lblMiddle.Text = $"{currentNumber} / {maximumNumber}";
            }
        }

        private int maximumNumber = 0;
        public int MaximumNumber
        {
            get { return currentNumber; }
            set
            {
                maximumNumber = value;
                lblMiddle.Text = $"{currentNumber} / {maximumNumber}";
            }
        }

        public Switcher()
        {
            // Initialize the left button
            btnLeft = new Button();
            btnLeft.Text = "<";
            btnLeft.Dock = DockStyle.Left;
            btnLeft.Click += (object? sender, EventArgs e) =>
            {
                if (LeftClick != null) LeftClick.Invoke(sender, e);
            };

            // Initialize the middle label
            lblMiddle = new Label();
            //lblMiddle.Dock = DockStyle.Fill;
            lblMiddle.TextAlign = ContentAlignment.MiddleCenter;
            lblMiddle.Font = new Font("Arial", 8);
            lblMiddle.Text = "0/0";

            // Initialize the right button
            btnRight = new Button();
            btnRight.Text = ">";
            btnRight.Dock = DockStyle.Right;
            btnRight.Click += (object? sender, EventArgs e) =>
            {
                if (RightClick != null) RightClick.Invoke(sender, e);
            };

            // Add the controls to the custom control
            this.Controls.Add(btnLeft);
            this.Controls.Add(lblMiddle);
            this.Controls.Add(btnRight);

            // Set up the control's properties
            this.Size = new Size(75, 20);
        }

        void ResizeToFitLabel(Control control, float percentage)
        {
            float height = control.Height * percentage;
            float width = control.Width * percentage;

            control.SuspendLayout();

            Font tryFont = control.Font;
            Size tempSize = TextRenderer.MeasureText(control.Text, tryFont);

            float heightRatio = height / tempSize.Height;
            float widthRatio = width / tempSize.Width;
            float size = tryFont.Size * Math.Min(widthRatio, heightRatio);
            if (size == 0)
                return;

            tryFont = new Font(tryFont.FontFamily, size, tryFont.Style);

            control.Font = tryFont;
            control.ResumeLayout();

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (btnLeft != null && btnRight != null && lblMiddle != null)
            {
                btnLeft.Size = new Size((int)(Width * 0.25f), Height);
                btnRight.Size = new Size((int)(Width * 0.25f), Height);
                lblMiddle.Size = new Size((int)(Width * 0.5f), Height);
                lblMiddle.Location = new Point(btnLeft.Width, 0);
            }
            foreach (Control control in Controls)
                ResizeToFitLabel(control, 0.8f);
        }

        private void BtnLeft_Click(object? sender, EventArgs e)
        {
            // Handle the left button click event
            if (CurrentNumber > 1)
                CurrentNumber -= 1;
        }

        private void BtnRight_Click(object? sender, EventArgs e)
        {
            // Handle the right button click event
            if (CurrentNumber > 1)
                CurrentNumber += 1;
        }
    }
}
