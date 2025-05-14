namespace MediumIntegrityClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._connect = new System.Windows.Forms.Button();
            this._text = new System.Windows.Forms.TextBox();
            this._close = new System.Windows.Forms.Button();
            this._getAnotherString = new System.Windows.Forms.Button();
            this._startServer = new System.Windows.Forms.Button();
            this._hiddenServer = new System.Windows.Forms.CheckBox();
            this._clear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _connect
            // 
            this._connect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._connect.Enabled = false;
            this._connect.Location = new System.Drawing.Point(24, 98);
            this._connect.Name = "_connect";
            this._connect.Size = new System.Drawing.Size(655, 50);
            this._connect.TabIndex = 2;
            this._connect.Text = "Connect";
            this._connect.UseVisualStyleBackColor = true;
            this._connect.Click += new System.EventHandler(this._connect_Click);
            // 
            // _text
            // 
            this._text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._text.Location = new System.Drawing.Point(24, 154);
            this._text.Multiline = true;
            this._text.Name = "_text";
            this._text.ReadOnly = true;
            this._text.Size = new System.Drawing.Size(655, 359);
            this._text.TabIndex = 3;
            // 
            // _close
            // 
            this._close.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._close.Enabled = false;
            this._close.Location = new System.Drawing.Point(24, 631);
            this._close.Name = "_close";
            this._close.Size = new System.Drawing.Size(655, 50);
            this._close.TabIndex = 6;
            this._close.Text = "Close";
            this._close.UseVisualStyleBackColor = true;
            this._close.Click += new System.EventHandler(this._close_Click);
            // 
            // _getAnotherString
            // 
            this._getAnotherString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._getAnotherString.Enabled = false;
            this._getAnotherString.Location = new System.Drawing.Point(24, 575);
            this._getAnotherString.Name = "_getAnotherString";
            this._getAnotherString.Size = new System.Drawing.Size(655, 50);
            this._getAnotherString.TabIndex = 5;
            this._getAnotherString.Text = "Get Another String";
            this._getAnotherString.UseVisualStyleBackColor = true;
            this._getAnotherString.Click += new System.EventHandler(this._getAnotherString_Click);
            // 
            // _startServer
            // 
            this._startServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._startServer.Location = new System.Drawing.Point(24, 12);
            this._startServer.Name = "_startServer";
            this._startServer.Size = new System.Drawing.Size(655, 50);
            this._startServer.TabIndex = 0;
            this._startServer.Text = "Start Server";
            this._startServer.UseVisualStyleBackColor = true;
            this._startServer.Click += new System.EventHandler(this._startServer_Click);
            // 
            // _hiddenServer
            // 
            this._hiddenServer.AutoSize = true;
            this._hiddenServer.Location = new System.Drawing.Point(24, 68);
            this._hiddenServer.Name = "_hiddenServer";
            this._hiddenServer.Size = new System.Drawing.Size(185, 24);
            this._hiddenServer.TabIndex = 1;
            this._hiddenServer.Text = "Create hidden Server";
            this._hiddenServer.UseVisualStyleBackColor = true;
            // 
            // _clear
            // 
            this._clear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._clear.Location = new System.Drawing.Point(24, 519);
            this._clear.Name = "_clear";
            this._clear.Size = new System.Drawing.Size(655, 50);
            this._clear.TabIndex = 4;
            this._clear.Text = "Clear Text";
            this._clear.UseVisualStyleBackColor = true;
            this._clear.Click += new System.EventHandler(this._clear_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 693);
            this.Controls.Add(this._hiddenServer);
            this.Controls.Add(this._text);
            this.Controls.Add(this._startServer);
            this.Controls.Add(this._clear);
            this.Controls.Add(this._getAnotherString);
            this.Controls.Add(this._close);
            this.Controls.Add(this._connect);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _connect;
        private System.Windows.Forms.TextBox _text;
        private System.Windows.Forms.Button _close;
        private System.Windows.Forms.Button _getAnotherString;
        private System.Windows.Forms.Button _startServer;
        private System.Windows.Forms.CheckBox _hiddenServer;
        private System.Windows.Forms.Button _clear;
    }
}

