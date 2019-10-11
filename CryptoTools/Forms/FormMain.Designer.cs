namespace CryptoTools
{
    partial class FormMain
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.logicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exchangeGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exchangeTransferToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aPIKeysAndSecretsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblExchanges = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblPairs = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblCoins = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.withdrawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logicsToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1196, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // logicsToolStripMenuItem
            // 
            this.logicsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exchangeGridToolStripMenuItem,
            this.bookDataToolStripMenuItem,
            this.exchangeTransferToolStripMenuItem});
            this.logicsToolStripMenuItem.Name = "logicsToolStripMenuItem";
            this.logicsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.logicsToolStripMenuItem.Text = "View";
            // 
            // exchangeGridToolStripMenuItem
            // 
            this.exchangeGridToolStripMenuItem.Name = "exchangeGridToolStripMenuItem";
            this.exchangeGridToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.exchangeGridToolStripMenuItem.Text = "Ticker grid";
            this.exchangeGridToolStripMenuItem.Click += new System.EventHandler(this.ExchangeGridToolStripMenuItem_Click);
            // 
            // bookDataToolStripMenuItem
            // 
            this.bookDataToolStripMenuItem.Name = "bookDataToolStripMenuItem";
            this.bookDataToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.bookDataToolStripMenuItem.Text = "OrderBooks";
            this.bookDataToolStripMenuItem.Click += new System.EventHandler(this.BookDataToolStripMenuItem_Click);
            // 
            // exchangeTransferToolStripMenuItem
            // 
            this.exchangeTransferToolStripMenuItem.Name = "exchangeTransferToolStripMenuItem";
            this.exchangeTransferToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.exchangeTransferToolStripMenuItem.Text = "Transfer deals";
            this.exchangeTransferToolStripMenuItem.Click += new System.EventHandler(this.ExchangeTransferToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aPIKeysAndSecretsToolStripMenuItem,
            this.withdrawToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // aPIKeysAndSecretsToolStripMenuItem
            // 
            this.aPIKeysAndSecretsToolStripMenuItem.Name = "aPIKeysAndSecretsToolStripMenuItem";
            this.aPIKeysAndSecretsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aPIKeysAndSecretsToolStripMenuItem.Text = "API keys and secrets";
            this.aPIKeysAndSecretsToolStripMenuItem.Click += new System.EventHandler(this.APIKeysAndSecretsToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblExchanges,
            this.lblPairs,
            this.lblCoins,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 680);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1196, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblExchanges
            // 
            this.lblExchanges.AutoSize = false;
            this.lblExchanges.Name = "lblExchanges";
            this.lblExchanges.Size = new System.Drawing.Size(100, 17);
            this.lblExchanges.Text = "Exchanges: ";
            this.lblExchanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPairs
            // 
            this.lblPairs.AutoSize = false;
            this.lblPairs.Name = "lblPairs";
            this.lblPairs.Size = new System.Drawing.Size(100, 17);
            this.lblPairs.Text = "Pairs: ";
            this.lblPairs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCoins
            // 
            this.lblCoins.AutoSize = false;
            this.lblCoins.Name = "lblCoins";
            this.lblCoins.Size = new System.Drawing.Size(100, 17);
            this.lblCoins.Text = "Coins: ";
            this.lblCoins.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(90, 17);
            this.lblStatus.Text = "Status: Sleeping";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // withdrawToolStripMenuItem
            // 
            this.withdrawToolStripMenuItem.Name = "withdrawToolStripMenuItem";
            this.withdrawToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.withdrawToolStripMenuItem.Text = "withdraw";
            this.withdrawToolStripMenuItem.Click += new System.EventHandler(this.WithdrawToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 702);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Crypto tools";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem logicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aPIKeysAndSecretsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exchangeGridToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblExchanges;
        private System.Windows.Forms.ToolStripStatusLabel lblPairs;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripMenuItem bookDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exchangeTransferToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel lblCoins;
        private System.Windows.Forms.ToolStripMenuItem withdrawToolStripMenuItem;
    }
}
