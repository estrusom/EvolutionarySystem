namespace MIUtopologyApp
{
    partial class Graph3DForm
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
            webViewBrowser = new Microsoft.Web.WebView2.WinForms.WebView2();
            panel1 = new Panel();
            btnSource = new Button();
            ((System.ComponentModel.ISupportInitialize)webViewBrowser).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // webViewBrowser
            // 
            webViewBrowser.AllowExternalDrop = true;
            webViewBrowser.CreationProperties = null;
            webViewBrowser.DefaultBackgroundColor = Color.White;
            webViewBrowser.Dock = DockStyle.Fill;
            webViewBrowser.Location = new Point(0, 0);
            webViewBrowser.Name = "webViewBrowser";
            webViewBrowser.Size = new Size(800, 450);
            webViewBrowser.TabIndex = 0;
            webViewBrowser.ZoomFactor = 1D;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSource);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 428);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 22);
            panel1.TabIndex = 1;
            // 
            // btnSource
            // 
            btnSource.Dock = DockStyle.Left;
            btnSource.Location = new Point(0, 0);
            btnSource.Name = "btnSource";
            btnSource.Size = new Size(75, 22);
            btnSource.TabIndex = 0;
            btnSource.Text = "Source";
            btnSource.UseVisualStyleBackColor = true;
            btnSource.Click += btnSource_Click;
            // 
            // Graph3DForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(webViewBrowser);
            Name = "Graph3DForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Graph3DForm";
            ((System.ComponentModel.ISupportInitialize)webViewBrowser).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewBrowser;
        private Panel panel1;
        private Button btnSource;
    }
}