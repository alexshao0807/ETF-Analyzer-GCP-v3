namespace ETF_Uploader
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCompare = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtYesterday = new System.Windows.Forms.TextBox();
            this.txtToday = new System.Windows.Forms.TextBox();
            this.btnBrowseYesterday = new System.Windows.Forms.Button();
            this.btnBrowseToday = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCompare
            // 
            this.btnCompare.Location = new System.Drawing.Point(589, 326);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(117, 77);
            this.btnCompare.TabIndex = 0;
            this.btnCompare.Text = "比較";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(153, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "昨日";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(543, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "今日";
            // 
            // txtYesterday
            // 
            this.txtYesterday.AllowDrop = true;
            this.txtYesterday.Location = new System.Drawing.Point(155, 101);
            this.txtYesterday.Multiline = true;
            this.txtYesterday.Name = "txtYesterday";
            this.txtYesterday.ReadOnly = true;
            this.txtYesterday.Size = new System.Drawing.Size(216, 63);
            this.txtYesterday.TabIndex = 3;
            // 
            // txtToday
            // 
            this.txtToday.AllowDrop = true;
            this.txtToday.Location = new System.Drawing.Point(545, 101);
            this.txtToday.Multiline = true;
            this.txtToday.Name = "txtToday";
            this.txtToday.ReadOnly = true;
            this.txtToday.Size = new System.Drawing.Size(216, 63);
            this.txtToday.TabIndex = 4;
            // 
            // btnBrowseYesterday
            // 
            this.btnBrowseYesterday.Location = new System.Drawing.Point(313, 177);
            this.btnBrowseYesterday.Name = "btnBrowseYesterday";
            this.btnBrowseYesterday.Size = new System.Drawing.Size(122, 34);
            this.btnBrowseYesterday.TabIndex = 5;
            this.btnBrowseYesterday.Text = "瀏覽檔案路徑位置";
            this.btnBrowseYesterday.UseVisualStyleBackColor = true;
            this.btnBrowseYesterday.Click += new System.EventHandler(this.btnBrowseYesterday_Click);
            // 
            // btnBrowseToday
            // 
            this.btnBrowseToday.Location = new System.Drawing.Point(666, 177);
            this.btnBrowseToday.Name = "btnBrowseToday";
            this.btnBrowseToday.Size = new System.Drawing.Size(122, 34);
            this.btnBrowseToday.TabIndex = 6;
            this.btnBrowseToday.Text = "瀏覽檔案路徑位置";
            this.btnBrowseToday.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnBrowseToday);
            this.Controls.Add(this.btnBrowseYesterday);
            this.Controls.Add(this.txtToday);
            this.Controls.Add(this.txtYesterday);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCompare);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtYesterday;
        private System.Windows.Forms.TextBox txtToday;
        private System.Windows.Forms.Button btnBrowseYesterday;
        private System.Windows.Forms.Button btnBrowseToday;
    }
}

