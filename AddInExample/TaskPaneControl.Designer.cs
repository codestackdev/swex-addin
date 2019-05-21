namespace CodeStack.SwEx.AddIn.Example
{
    partial class TaskPaneControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.trlMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblText = new System.Windows.Forms.Label();
            this.txtText = new System.Windows.Forms.TextBox();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.trlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // trlMain
            // 
            this.trlMain.ColumnCount = 2;
            this.trlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.trlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.trlMain.Controls.Add(this.lblText, 0, 0);
            this.trlMain.Controls.Add(this.txtText, 1, 0);
            this.trlMain.Controls.Add(this.btnSendMessage, 0, 1);
            this.trlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trlMain.Location = new System.Drawing.Point(0, 0);
            this.trlMain.Name = "trlMain";
            this.trlMain.RowCount = 2;
            this.trlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.trlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.trlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.trlMain.Size = new System.Drawing.Size(260, 283);
            this.trlMain.TabIndex = 0;
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(3, 124);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(39, 17);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "Text:";
            // 
            // txtText
            // 
            this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtText.Location = new System.Drawing.Point(48, 116);
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(209, 22);
            this.txtText.TabIndex = 1;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.trlMain.SetColumnSpan(this.btnSendMessage, 2);
            this.btnSendMessage.Location = new System.Drawing.Point(58, 144);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(143, 37);
            this.btnSendMessage.TabIndex = 2;
            this.btnSendMessage.Text = "Send Message";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.OnSendMessage);
            // 
            // TaskPaneControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trlMain);
            this.Name = "TaskPaneControl";
            this.Size = new System.Drawing.Size(260, 283);
            this.trlMain.ResumeLayout(false);
            this.trlMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel trlMain;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Button btnSendMessage;
    }
}
