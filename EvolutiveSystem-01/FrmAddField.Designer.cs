namespace EvolutiveSystem
{
    partial class FrmAddField
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAddField));
            this.lblDenRegistry = new System.Windows.Forms.Label();
            this.lblDenEcrypt = new System.Windows.Forms.Label();
            this.lblDenValue = new System.Windows.Forms.Label();
            this.lblDenDataType = new System.Windows.Forms.Label();
            this.lblDenFieldName = new System.Windows.Forms.Label();
            this.lblDenKey = new System.Windows.Forms.Label();
            this.lblDenTabName = new System.Windows.Forms.Label();
            this.lblDenId = new System.Windows.Forms.Label();
            this.txtId = new System.Windows.Forms.TextBox();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.txtFieldName = new System.Windows.Forms.TextBox();
            this.cmbDataType = new System.Windows.Forms.ComboBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.cmbEncrypSel = new System.Windows.Forms.ComboBox();
            this.txtRegistry = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCance = new System.Windows.Forms.Button();
            this.cmbPrymaryKey = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblDenRegistry
            // 
            this.lblDenRegistry.AutoSize = true;
            this.lblDenRegistry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenRegistry.ForeColor = System.Drawing.Color.Blue;
            this.lblDenRegistry.Location = new System.Drawing.Point(77, 248);
            this.lblDenRegistry.Name = "lblDenRegistry";
            this.lblDenRegistry.Size = new System.Drawing.Size(77, 18);
            this.lblDenRegistry.TabIndex = 7;
            this.lblDenRegistry.Text = "Registro:";
            // 
            // lblDenEcrypt
            // 
            this.lblDenEcrypt.AutoSize = true;
            this.lblDenEcrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenEcrypt.ForeColor = System.Drawing.Color.Blue;
            this.lblDenEcrypt.Location = new System.Drawing.Point(34, 214);
            this.lblDenEcrypt.Name = "lblDenEcrypt";
            this.lblDenEcrypt.Size = new System.Drawing.Size(120, 18);
            this.lblDenEcrypt.TabIndex = 6;
            this.lblDenEcrypt.Text = "Campo cifrato:";
            // 
            // lblDenValue
            // 
            this.lblDenValue.AutoSize = true;
            this.lblDenValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenValue.ForeColor = System.Drawing.Color.Blue;
            this.lblDenValue.Location = new System.Drawing.Point(100, 182);
            this.lblDenValue.Name = "lblDenValue";
            this.lblDenValue.Size = new System.Drawing.Size(54, 18);
            this.lblDenValue.TabIndex = 5;
            this.lblDenValue.Text = "Value:";
            // 
            // lblDenDataType
            // 
            this.lblDenDataType.AutoSize = true;
            this.lblDenDataType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenDataType.ForeColor = System.Drawing.Color.Blue;
            this.lblDenDataType.Location = new System.Drawing.Point(76, 148);
            this.lblDenDataType.Name = "lblDenDataType";
            this.lblDenDataType.Size = new System.Drawing.Size(78, 18);
            this.lblDenDataType.TabIndex = 4;
            this.lblDenDataType.Text = "Tipo dati:";
            // 
            // lblDenFieldName
            // 
            this.lblDenFieldName.AutoSize = true;
            this.lblDenFieldName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenFieldName.ForeColor = System.Drawing.Color.Blue;
            this.lblDenFieldName.Location = new System.Drawing.Point(40, 116);
            this.lblDenFieldName.Name = "lblDenFieldName";
            this.lblDenFieldName.Size = new System.Drawing.Size(114, 18);
            this.lblDenFieldName.TabIndex = 7;
            this.lblDenFieldName.Text = "Nome campo:";
            // 
            // lblDenKey
            // 
            this.lblDenKey.AutoSize = true;
            this.lblDenKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenKey.ForeColor = System.Drawing.Color.Blue;
            this.lblDenKey.Location = new System.Drawing.Point(109, 82);
            this.lblDenKey.Name = "lblDenKey";
            this.lblDenKey.Size = new System.Drawing.Size(45, 18);
            this.lblDenKey.TabIndex = 6;
            this.lblDenKey.Text = "KEY:";
            // 
            // lblDenTabName
            // 
            this.lblDenTabName.AutoSize = true;
            this.lblDenTabName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenTabName.ForeColor = System.Drawing.Color.Blue;
            this.lblDenTabName.Location = new System.Drawing.Point(42, 50);
            this.lblDenTabName.Name = "lblDenTabName";
            this.lblDenTabName.Size = new System.Drawing.Size(112, 18);
            this.lblDenTabName.TabIndex = 5;
            this.lblDenTabName.Text = "Nome tabella:";
            // 
            // lblDenId
            // 
            this.lblDenId.AutoSize = true;
            this.lblDenId.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDenId.ForeColor = System.Drawing.Color.Blue;
            this.lblDenId.Location = new System.Drawing.Point(125, 18);
            this.lblDenId.Name = "lblDenId";
            this.lblDenId.Size = new System.Drawing.Size(29, 18);
            this.lblDenId.TabIndex = 4;
            this.lblDenId.Text = "ID:";
            // 
            // txtId
            // 
            this.txtId.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtId.Location = new System.Drawing.Point(161, 18);
            this.txtId.Name = "txtId";
            this.txtId.Size = new System.Drawing.Size(100, 24);
            this.txtId.TabIndex = 8;
            this.txtId.Tag = "Id";
            // 
            // txtTableName
            // 
            this.txtTableName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTableName.Location = new System.Drawing.Point(161, 50);
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.Size = new System.Drawing.Size(198, 24);
            this.txtTableName.TabIndex = 9;
            this.txtTableName.Tag = "TableName";
            // 
            // txtFieldName
            // 
            this.txtFieldName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFieldName.Location = new System.Drawing.Point(161, 116);
            this.txtFieldName.Name = "txtFieldName";
            this.txtFieldName.Size = new System.Drawing.Size(177, 24);
            this.txtFieldName.TabIndex = 11;
            this.txtFieldName.Tag = "FieldName";
            // 
            // cmbDataType
            // 
            this.cmbDataType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDataType.FormattingEnabled = true;
            this.cmbDataType.Location = new System.Drawing.Point(161, 148);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new System.Drawing.Size(237, 26);
            this.cmbDataType.TabIndex = 12;
            this.cmbDataType.Tag = "DataType";
            // 
            // txtValue
            // 
            this.txtValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue.Location = new System.Drawing.Point(161, 182);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(126, 24);
            this.txtValue.TabIndex = 13;
            this.txtValue.Tag = "Value";
            // 
            // cmbEncrypSel
            // 
            this.cmbEncrypSel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEncrypSel.FormattingEnabled = true;
            this.cmbEncrypSel.Items.AddRange(new object[] {
            "true",
            "false"});
            this.cmbEncrypSel.Location = new System.Drawing.Point(161, 214);
            this.cmbEncrypSel.Name = "cmbEncrypSel";
            this.cmbEncrypSel.Size = new System.Drawing.Size(150, 26);
            this.cmbEncrypSel.TabIndex = 14;
            this.cmbEncrypSel.Tag = "EncryptedField";
            // 
            // txtRegistry
            // 
            this.txtRegistry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRegistry.Location = new System.Drawing.Point(161, 248);
            this.txtRegistry.Name = "txtRegistry";
            this.txtRegistry.Size = new System.Drawing.Size(258, 24);
            this.txtRegistry.TabIndex = 15;
            this.txtRegistry.Tag = "Registry";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(161, 298);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 47);
            this.btnOk.TabIndex = 16;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCance
            // 
            this.btnCance.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCance.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCance.Location = new System.Drawing.Point(319, 298);
            this.btnCance.Name = "btnCance";
            this.btnCance.Size = new System.Drawing.Size(100, 47);
            this.btnCance.TabIndex = 17;
            this.btnCance.Text = "Annulla";
            this.btnCance.UseVisualStyleBackColor = true;
            // 
            // cmbPrymaryKey
            // 
            this.cmbPrymaryKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPrymaryKey.FormattingEnabled = true;
            this.cmbPrymaryKey.Items.AddRange(new object[] {
            "true",
            "false"});
            this.cmbPrymaryKey.Location = new System.Drawing.Point(161, 82);
            this.cmbPrymaryKey.Name = "cmbPrymaryKey";
            this.cmbPrymaryKey.Size = new System.Drawing.Size(150, 26);
            this.cmbPrymaryKey.TabIndex = 18;
            this.cmbPrymaryKey.Tag = "EncryptedField";
            // 
            // FrmAddField
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCance;
            this.ClientSize = new System.Drawing.Size(502, 368);
            this.Controls.Add(this.cmbPrymaryKey);
            this.Controls.Add(this.btnCance);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtRegistry);
            this.Controls.Add(this.cmbEncrypSel);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.cmbDataType);
            this.Controls.Add(this.txtFieldName);
            this.Controls.Add(this.txtTableName);
            this.Controls.Add(this.txtId);
            this.Controls.Add(this.lblDenFieldName);
            this.Controls.Add(this.lblDenKey);
            this.Controls.Add(this.lblDenTabName);
            this.Controls.Add(this.lblDenId);
            this.Controls.Add(this.lblDenEcrypt);
            this.Controls.Add(this.lblDenDataType);
            this.Controls.Add(this.lblDenRegistry);
            this.Controls.Add(this.lblDenValue);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAddField";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aggiungi campi";
            this.Load += new System.EventHandler(this.FrmAddField_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDenRegistry;
        private System.Windows.Forms.Label lblDenEcrypt;
        private System.Windows.Forms.Label lblDenValue;
        private System.Windows.Forms.Label lblDenDataType;
        private System.Windows.Forms.Label lblDenFieldName;
        private System.Windows.Forms.Label lblDenKey;
        private System.Windows.Forms.Label lblDenTabName;
        private System.Windows.Forms.Label lblDenId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.TextBox txtFieldName;
        private System.Windows.Forms.ComboBox cmbDataType;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.ComboBox cmbEncrypSel;
        private System.Windows.Forms.TextBox txtRegistry;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCance;
        private System.Windows.Forms.ComboBox cmbPrymaryKey;
    }
}