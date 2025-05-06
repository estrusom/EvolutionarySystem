namespace EvolutiveSystem
{
    partial class FrmAddTable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAddTable));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.txtTblName = new System.Windows.Forms.TextBox();
            this.lbTblName = new System.Windows.Forms.Label();
            this.lblDenDboff = new System.Windows.Forms.Label();
            this.lblDb = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(290, 102);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(108, 54);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Annulla";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(133, 102);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(108, 54);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtTblName
            // 
            this.txtTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTblName.Location = new System.Drawing.Point(133, 44);
            this.txtTblName.Name = "txtTblName";
            this.txtTblName.Size = new System.Drawing.Size(265, 30);
            this.txtTblName.TabIndex = 5;
            // 
            // lbTblName
            // 
            this.lbTblName.AutoSize = true;
            this.lbTblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTblName.Location = new System.Drawing.Point(5, 44);
            this.lbTblName.Name = "lbTblName";
            this.lbTblName.Size = new System.Drawing.Size(127, 25);
            this.lbTblName.TabIndex = 4;
            this.lbTblName.Text = "Tablename:";
            // 
            // lblDenDboff
            // 
            this.lblDenDboff.AutoSize = true;
            this.lblDenDboff.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDboff.Location = new System.Drawing.Point(12, 173);
            this.lblDenDboff.Name = "lblDenDboff";
            this.lblDenDboff.Size = new System.Drawing.Size(78, 16);
            this.lblDenDboff.TabIndex = 8;
            this.lblDenDboff.Text = "DB select:";
            // 
            // lblDb
            // 
            this.lblDb.AutoSize = true;
            this.lblDb.Location = new System.Drawing.Point(92, 173);
            this.lblDb.Name = "lblDb";
            this.lblDb.Size = new System.Drawing.Size(44, 16);
            this.lblDb.TabIndex = 9;
            this.lblDb.Text = "label1";
            // 
            // FrmAddTable
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(421, 198);
            this.Controls.Add(this.lblDb);
            this.Controls.Add(this.lblDenDboff);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtTblName);
            this.Controls.Add(this.lbTblName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAddTable";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Imposta table name";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmAddTable_FormClosing);
            this.Load += new System.EventHandler(this.FrmAddTable_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txtTblName;
        private System.Windows.Forms.Label lbTblName;
        private System.Windows.Forms.Label lblDenDboff;
        private System.Windows.Forms.Label lblDb;
    }
}