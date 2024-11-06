namespace AutomaticGPTPupTest1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            button1 = new Button();
            button2 = new Button();
            chatControl1 = new ChatControl();
            button3 = new Button();
            statusUpdater = new System.Windows.Forms.Timer(components);
            statusStrip1 = new StatusStrip();
            statusBrowserState = new ToolStripStatusLabel();
            statusGPTState = new ToolStripStatusLabel();
            statusState = new ToolStripStatusLabel();
            textBox1 = new TextBox();
            button4 = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(524, 414);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.Location = new Point(624, 414);
            button2.Name = "button2";
            button2.Size = new Size(94, 29);
            button2.TabIndex = 1;
            button2.Text = "Stop";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // chatControl1
            // 
            chatControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chatControl1.AutoScroll = true;
            chatControl1.AutoScrollMinSize = new Size(1, 0);
            chatControl1.Location = new Point(12, 12);
            chatControl1.Name = "chatControl1";
            chatControl1.Size = new Size(829, 396);
            chatControl1.TabIndex = 3;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button3.Location = new Point(724, 414);
            button3.Name = "button3";
            button3.Size = new Size(117, 29);
            button3.TabIndex = 5;
            button3.Text = "Stop Speech";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // statusUpdater
            // 
            statusUpdater.Tick += statusUpdater_Tick;
            // 
            // statusStrip1
            // 
            statusStrip1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            statusStrip1.Dock = DockStyle.None;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusBrowserState, statusGPTState, statusState });
            statusStrip1.Location = new Point(9, 417);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(166, 26);
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusBrowserState
            // 
            statusBrowserState.Name = "statusBrowserState";
            statusBrowserState.Size = new Size(49, 20);
            statusBrowserState.Text = "Status";
            // 
            // statusGPTState
            // 
            statusGPTState.Name = "statusGPTState";
            statusGPTState.Size = new Size(49, 20);
            statusGPTState.Text = "Status";
            // 
            // statusState
            // 
            statusState.Name = "statusState";
            statusState.Size = new Size(49, 20);
            statusState.Text = "Status";
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.BackColor = Color.White;
            textBox1.Location = new Point(352, 415);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(166, 27);
            textBox1.TabIndex = 7;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button4.Location = new Point(252, 417);
            button4.Name = "button4";
            button4.Size = new Size(94, 29);
            button4.TabIndex = 8;
            button4.Text = "Send";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(853, 445);
            Controls.Add(button4);
            Controls.Add(textBox1);
            Controls.Add(statusStrip1);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(chatControl1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private ChatControl chatControl1;
        private Button button3;
        private System.Windows.Forms.Timer statusUpdater;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusBrowserState;
        private ToolStripStatusLabel statusGPTState;
        private ToolStripStatusLabel statusState;
        private TextBox textBox1;
        private Button button4;
    }
}