namespace SerialTest
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.portComboBox = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.TXcomboBox = new System.Windows.Forms.ComboBox();
            this.resultTextBox = new System.Windows.Forms.TextBox();
            this.LEDTextBox = new System.Windows.Forms.TextBox();
            this.LEDSendButton = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.scanStopButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(90, 60);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "接続";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // portComboBox
            // 
            this.portComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.portComboBox.FormattingEnabled = true;
            this.portComboBox.Location = new System.Drawing.Point(201, 62);
            this.portComboBox.Name = "portComboBox";
            this.portComboBox.Size = new System.Drawing.Size(121, 20);
            this.portComboBox.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(90, 112);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "送信";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(201, 159);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(587, 137);
            this.textBox1.TabIndex = 4;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(90, 415);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "閉じる";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // TXcomboBox
            // 
            this.TXcomboBox.FormattingEnabled = true;
            this.TXcomboBox.Location = new System.Drawing.Point(201, 114);
            this.TXcomboBox.Name = "TXcomboBox";
            this.TXcomboBox.Size = new System.Drawing.Size(360, 20);
            this.TXcomboBox.TabIndex = 6;
            // 
            // resultTextBox
            // 
            this.resultTextBox.Location = new System.Drawing.Point(201, 303);
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.Size = new System.Drawing.Size(360, 19);
            this.resultTextBox.TabIndex = 7;
            // 
            // LEDTextBox
            // 
            this.LEDTextBox.Location = new System.Drawing.Point(201, 329);
            this.LEDTextBox.Name = "LEDTextBox";
            this.LEDTextBox.Size = new System.Drawing.Size(100, 19);
            this.LEDTextBox.TabIndex = 8;
            // 
            // LEDSendButton
            // 
            this.LEDSendButton.Location = new System.Drawing.Point(90, 327);
            this.LEDSendButton.Name = "LEDSendButton";
            this.LEDSendButton.Size = new System.Drawing.Size(75, 23);
            this.LEDSendButton.TabIndex = 9;
            this.LEDSendButton.Text = "LED送信";
            this.LEDSendButton.UseVisualStyleBackColor = true;
            this.LEDSendButton.Click += new System.EventHandler(this.LEDSendButton_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(90, 159);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 9;
            this.button5.Text = "連続スキャン";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.scanButtonClicked);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // scanStopButton
            // 
            this.scanStopButton.Location = new System.Drawing.Point(90, 189);
            this.scanStopButton.Name = "scanStopButton";
            this.scanStopButton.Size = new System.Drawing.Size(75, 23);
            this.scanStopButton.TabIndex = 10;
            this.scanStopButton.Text = "スキャン停止";
            this.scanStopButton.UseVisualStyleBackColor = true;
            this.scanStopButton.Click += new System.EventHandler(this.scanStopButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.scanStopButton);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.LEDSendButton);
            this.Controls.Add(this.LEDTextBox);
            this.Controls.Add(this.resultTextBox);
            this.Controls.Add(this.TXcomboBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.portComboBox);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox portComboBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox TXcomboBox;
        private System.Windows.Forms.TextBox resultTextBox;
        private System.Windows.Forms.TextBox LEDTextBox;
        private System.Windows.Forms.Button LEDSendButton;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button scanStopButton;
    }
}

