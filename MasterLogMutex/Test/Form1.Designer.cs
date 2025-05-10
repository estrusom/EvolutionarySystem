namespace Test
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnPath = new System.Windows.Forms.Button();
            this.lblReturnedPath = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblReturnedFileName = new System.Windows.Forms.Label();
            this.txtBoxRead = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(41, 2);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(211, 20);
            this.txtPath.TabIndex = 1;
            // 
            // btnPath
            // 
            this.btnPath.Location = new System.Drawing.Point(205, 86);
            this.btnPath.Name = "btnPath";
            this.btnPath.Size = new System.Drawing.Size(75, 23);
            this.btnPath.TabIndex = 2;
            this.btnPath.Text = "Visualizza";
            this.btnPath.UseVisualStyleBackColor = true;
            this.btnPath.Click += new System.EventHandler(this.btnPath_Click);
            // 
            // lblReturnedPath
            // 
            this.lblReturnedPath.AutoSize = true;
            this.lblReturnedPath.Location = new System.Drawing.Point(6, 58);
            this.lblReturnedPath.Name = "lblReturnedPath";
            this.lblReturnedPath.Size = new System.Drawing.Size(35, 13);
            this.lblReturnedPath.TabIndex = 3;
            this.lblReturnedPath.Text = "label2";
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(63, 25);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(211, 20);
            this.txtFileName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "FileName";
            // 
            // lblReturnedFileName
            // 
            this.lblReturnedFileName.AutoSize = true;
            this.lblReturnedFileName.Location = new System.Drawing.Point(6, 86);
            this.lblReturnedFileName.Name = "lblReturnedFileName";
            this.lblReturnedFileName.Size = new System.Drawing.Size(35, 13);
            this.lblReturnedFileName.TabIndex = 6;
            this.lblReturnedFileName.Text = "label2";
            // 
            // txtBoxRead
            // 
            this.txtBoxRead.Location = new System.Drawing.Point(41, 102);
            this.txtBoxRead.Multiline = true;
            this.txtBoxRead.Name = "txtBoxRead";
            this.txtBoxRead.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxRead.Size = new System.Drawing.Size(142, 69);
            this.txtBoxRead.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 183);
            this.Controls.Add(this.txtBoxRead);
            this.Controls.Add(this.lblReturnedFileName);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblReturnedPath);
            this.Controls.Add(this.btnPath);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnPath;
        private System.Windows.Forms.Label lblReturnedPath;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblReturnedFileName;
        private System.Windows.Forms.TextBox txtBoxRead;
    }
}

