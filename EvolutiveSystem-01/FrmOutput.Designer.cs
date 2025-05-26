namespace EvolutiveSystem_01
{
    partial class FrmOutput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmOutput));
            this.pnlCmd = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.rctOutput = new System.Windows.Forms.RichTextBox();
            this.pnlCmd.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCmd
            // 
            this.pnlCmd.Controls.Add(this.btnDelete);
            this.pnlCmd.Controls.Add(this.btnSave);
            this.pnlCmd.Controls.Add(this.btnOpen);
            this.pnlCmd.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCmd.Location = new System.Drawing.Point(0, 0);
            this.pnlCmd.Name = "pnlCmd";
            this.pnlCmd.Size = new System.Drawing.Size(1076, 46);
            this.pnlCmd.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.trash_empty_recycle_delete_delete_9752;
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnDelete.Location = new System.Drawing.Point(102, 0);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(51, 46);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSave.Image = global::EvolutiveSystem_01.Properties.Resources.icons8_save_close_481;
            this.btnSave.Location = new System.Drawing.Point(51, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(51, 46);
            this.btnSave.TabIndex = 1;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnOpen
            // 
            this.btnOpen.BackgroundImage = global::EvolutiveSystem_01.Properties.Resources.filesystems_thefolder_653;
            this.btnOpen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOpen.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnOpen.Location = new System.Drawing.Point(0, 0);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(51, 46);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.UseVisualStyleBackColor = true;
            // 
            // rctOutput
            // 
            this.rctOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rctOutput.Location = new System.Drawing.Point(0, 46);
            this.rctOutput.Name = "rctOutput";
            this.rctOutput.Size = new System.Drawing.Size(1076, 728);
            this.rctOutput.TabIndex = 1;
            this.rctOutput.Text = "";
            // 
            // FrmOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 774);
            this.Controls.Add(this.rctOutput);
            this.Controls.Add(this.pnlCmd);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmOutput";
            this.Text = "FrmOutput";
            this.Load += new System.EventHandler(this.FrmOutput_Load);
            this.pnlCmd.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCmd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.RichTextBox rctOutput;
    }
}