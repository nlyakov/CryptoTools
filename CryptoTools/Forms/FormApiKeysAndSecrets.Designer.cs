namespace CryptoTools.Forms
{
    partial class FormApiSettings
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
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btnClearSettings = new System.Windows.Forms.Button();
            this.lblPassword = new System.Windows.Forms.Label();
            this.edtPassword = new System.Windows.Forms.TextBox();
            this.btnLoadSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.pnlClient = new System.Windows.Forms.Panel();
            this.memSettings = new System.Windows.Forms.RichTextBox();
            this.pnlControls.SuspendLayout();
            this.pnlClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.btnClearSettings);
            this.pnlControls.Controls.Add(this.lblPassword);
            this.pnlControls.Controls.Add(this.edtPassword);
            this.pnlControls.Controls.Add(this.btnLoadSettings);
            this.pnlControls.Controls.Add(this.btnSaveSettings);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlControls.Location = new System.Drawing.Point(0, 418);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(800, 32);
            this.pnlControls.TabIndex = 2;
            // 
            // btnClearSettings
            // 
            this.btnClearSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearSettings.Location = new System.Drawing.Point(643, 4);
            this.btnClearSettings.Name = "btnClearSettings";
            this.btnClearSettings.Size = new System.Drawing.Size(152, 23);
            this.btnClearSettings.TabIndex = 4;
            this.btnClearSettings.Text = "Clear data from window";
            this.btnClearSettings.UseVisualStyleBackColor = true;
            this.btnClearSettings.Click += new System.EventHandler(this.BtnClearSettings_Click);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(5, 9);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password";
            // 
            // edtPassword
            // 
            this.edtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtPassword.Location = new System.Drawing.Point(64, 6);
            this.edtPassword.Name = "edtPassword";
            this.edtPassword.Size = new System.Drawing.Size(257, 20);
            this.edtPassword.TabIndex = 2;
            this.edtPassword.UseSystemPasswordChar = true;
            // 
            // btnLoadSettings
            // 
            this.btnLoadSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadSettings.Location = new System.Drawing.Point(327, 4);
            this.btnLoadSettings.Name = "btnLoadSettings";
            this.btnLoadSettings.Size = new System.Drawing.Size(152, 23);
            this.btnLoadSettings.TabIndex = 1;
            this.btnLoadSettings.Text = "Load && decode";
            this.btnLoadSettings.UseVisualStyleBackColor = true;
            this.btnLoadSettings.Click += new System.EventHandler(this.BtnLoadSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSettings.Location = new System.Drawing.Point(485, 4);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(152, 23);
            this.btnSaveSettings.TabIndex = 0;
            this.btnSaveSettings.Text = "Encrypt && save";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.BtnSaveSettings_Click);
            // 
            // pnlClient
            // 
            this.pnlClient.Controls.Add(this.memSettings);
            this.pnlClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlClient.Location = new System.Drawing.Point(0, 0);
            this.pnlClient.Name = "pnlClient";
            this.pnlClient.Size = new System.Drawing.Size(800, 418);
            this.pnlClient.TabIndex = 3;
            // 
            // memSettings
            // 
            this.memSettings.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.memSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memSettings.Location = new System.Drawing.Point(0, 0);
            this.memSettings.Name = "memSettings";
            this.memSettings.Size = new System.Drawing.Size(800, 418);
            this.memSettings.TabIndex = 2;
            this.memSettings.Text = "";
            // 
            // FormApiSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlClient);
            this.Controls.Add(this.pnlControls);
            this.Name = "FormApiSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "API Keys & Secret phrases";
            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            this.pnlClient.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btnClearSettings;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox edtPassword;
        private System.Windows.Forms.Button btnLoadSettings;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Panel pnlClient;
        private System.Windows.Forms.RichTextBox memSettings;
    }
}