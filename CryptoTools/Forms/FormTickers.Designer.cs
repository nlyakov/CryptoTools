namespace CryptoTools.Forms
{
    partial class FormTickerData
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTickerData));
            this.dgvTicker = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnUpdate = new System.Windows.Forms.ToolStripButton();
            this.btnUpdateOrderBooks = new System.Windows.Forms.ToolStripButton();
            this.btnUpdateFullOrderBooks = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolbtnAllPairs = new System.Windows.Forms.ToolStripButton();
            this.lblFilter = new System.Windows.Forms.ToolStripLabel();
            this.edtFilter = new System.Windows.Forms.ToolStripTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblAllPairs = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDisplayedLabels = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastUpdated = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicker)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvTicker
            // 
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTicker.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvTicker.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTicker.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvTicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTicker.Location = new System.Drawing.Point(0, 0);
            this.dgvTicker.Name = "dgvTicker";
            this.dgvTicker.ReadOnly = true;
            this.dgvTicker.RowHeadersVisible = false;
            this.dgvTicker.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTicker.Size = new System.Drawing.Size(1330, 621);
            this.dgvTicker.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnUpdate,
            this.btnUpdateOrderBooks,
            this.btnUpdateFullOrderBooks,
            this.toolStripSeparator1,
            this.toolbtnAllPairs,
            this.lblFilter,
            this.edtFilter});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(582, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnUpdate
            // 
            this.btnUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnUpdate.Image = ((System.Drawing.Image)(resources.GetObject("btnUpdate.Image")));
            this.btnUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(89, 22);
            this.btnUpdate.Text = "Update Tickers";
            this.btnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);
            // 
            // btnUpdateOrderBooks
            // 
            this.btnUpdateOrderBooks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnUpdateOrderBooks.Image = ((System.Drawing.Image)(resources.GetObject("btnUpdateOrderBooks.Image")));
            this.btnUpdateOrderBooks.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdateOrderBooks.Name = "btnUpdateOrderBooks";
            this.btnUpdateOrderBooks.Size = new System.Drawing.Size(114, 22);
            this.btnUpdateOrderBooks.Text = "Update OrderBooks";
            this.btnUpdateOrderBooks.Click += new System.EventHandler(this.BtnUpdateOrderBooks_Click);
            // 
            // btnUpdateFullOrderBooks
            // 
            this.btnUpdateFullOrderBooks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnUpdateFullOrderBooks.Image = ((System.Drawing.Image)(resources.GetObject("btnUpdateFullOrderBooks.Image")));
            this.btnUpdateFullOrderBooks.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdateFullOrderBooks.Name = "btnUpdateFullOrderBooks";
            this.btnUpdateFullOrderBooks.Size = new System.Drawing.Size(136, 22);
            this.btnUpdateFullOrderBooks.Text = "Update Full OrderBooks";
            this.btnUpdateFullOrderBooks.Click += new System.EventHandler(this.BtnUpdateFullOrderBooks_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolbtnAllPairs
            // 
            this.toolbtnAllPairs.CheckOnClick = true;
            this.toolbtnAllPairs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolbtnAllPairs.Image = ((System.Drawing.Image)(resources.GetObject("toolbtnAllPairs.Image")));
            this.toolbtnAllPairs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolbtnAllPairs.Name = "toolbtnAllPairs";
            this.toolbtnAllPairs.Size = new System.Drawing.Size(53, 22);
            this.toolbtnAllPairs.Text = "All pairs";
            // 
            // lblFilter
            // 
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(39, 22);
            this.lblFilter.Text = "Filter: ";
            // 
            // edtFilter
            // 
            this.edtFilter.Name = "edtFilter";
            this.edtFilter.Size = new System.Drawing.Size(100, 25);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblAllPairs,
            this.lblDisplayedLabels,
            this.lblLastUpdated});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1330, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblAllPairs
            // 
            this.lblAllPairs.AutoSize = false;
            this.lblAllPairs.Name = "lblAllPairs";
            this.lblAllPairs.Size = new System.Drawing.Size(100, 17);
            this.lblAllPairs.Text = "All pairs:";
            this.lblAllPairs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDisplayedLabels
            // 
            this.lblDisplayedLabels.AutoSize = false;
            this.lblDisplayedLabels.Name = "lblDisplayedLabels";
            this.lblDisplayedLabels.Size = new System.Drawing.Size(140, 17);
            this.lblDisplayedLabels.Text = "Displayed pairs:";
            this.lblDisplayedLabels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLastUpdated
            // 
            this.lblLastUpdated.AutoSize = false;
            this.lblLastUpdated.Name = "lblLastUpdated";
            this.lblLastUpdated.Size = new System.Drawing.Size(300, 17);
            this.lblLastUpdated.Text = "Last updated: not updated";
            this.lblLastUpdated.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.dgvTicker);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1330, 621);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1330, 668);
            this.toolStripContainer1.TabIndex = 2;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // FormTickerData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1330, 668);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "FormTickerData";
            this.Text = "Ticker prices";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTickerData_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTicker)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTicker;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolbtnAllPairs;
        private System.Windows.Forms.ToolStripLabel lblFilter;
        private System.Windows.Forms.ToolStripTextBox edtFilter;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblAllPairs;
        private System.Windows.Forms.ToolStripStatusLabel lblDisplayedLabels;
        private System.Windows.Forms.ToolStripStatusLabel lblLastUpdated;
        private System.Windows.Forms.ToolStripButton btnUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnUpdateOrderBooks;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripButton btnUpdateFullOrderBooks;
    }
}