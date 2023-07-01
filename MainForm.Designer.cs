using System.Text;
using System.Windows.Forms;

namespace WinRAR垃圾清理
{
    partial class MainForm
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
        /// 
        private void copyButton_Click(object sender, EventArgs e)
        {
            // 检查是否有选定的项
            if (resultListBox.SelectedItems.Count > 0)
            {
                // 创建一个 StringBuilder 来存储选定项的文本
                StringBuilder sb = new StringBuilder();

                // 遍历选定的每一项，并将其文本添加到 StringBuilder 中
                foreach (var selectedItem in resultListBox.SelectedItems)
                {
                    sb.AppendLine(selectedItem.ToString());
                }

                // 将 StringBuilder 的内容复制到剪贴板
                Clipboard.SetText(sb.ToString());
                MessageBox.Show("复制成功。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("什么也没选择。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            startButton = new Button();
            cancelButton = new Button();
            statusLabel = new Label();
            driveListBox = new CheckedListBox();
            selectAllButton = new Button();
            deselectAllButton = new Button();
            deleteFoldersCheckBox = new CheckBox();
            resultListBox = new ListBox();
            copyButton = new Button();
            SuspendLayout();
            // 
            // startButton
            // 
            resources.ApplyResources(startButton, "startButton");
            startButton.Name = "startButton";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // cancelButton
            // 
            resources.ApplyResources(cancelButton, "cancelButton");
            cancelButton.Name = "cancelButton";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // statusLabel
            // 
            resources.ApplyResources(statusLabel, "statusLabel");
            statusLabel.Name = "statusLabel";
            // 
            // driveListBox
            // 
            resources.ApplyResources(driveListBox, "driveListBox");
            driveListBox.BorderStyle = BorderStyle.FixedSingle;
            driveListBox.FormattingEnabled = true;
            driveListBox.Name = "driveListBox";
            // 
            // selectAllButton
            // 
            resources.ApplyResources(selectAllButton, "selectAllButton");
            selectAllButton.Name = "selectAllButton";
            selectAllButton.UseVisualStyleBackColor = true;
            // 
            // deselectAllButton
            // 
            resources.ApplyResources(deselectAllButton, "deselectAllButton");
            deselectAllButton.Name = "deselectAllButton";
            deselectAllButton.UseVisualStyleBackColor = true;
            // 
            // deleteFoldersCheckBox
            // 
            resources.ApplyResources(deleteFoldersCheckBox, "deleteFoldersCheckBox");
            deleteFoldersCheckBox.Name = "deleteFoldersCheckBox";
            deleteFoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // resultListBox
            // 
            resources.ApplyResources(resultListBox, "resultListBox");
            resultListBox.BorderStyle = BorderStyle.FixedSingle;
            resultListBox.FormattingEnabled = true;
            resultListBox.Name = "resultListBox";
            resultListBox.SelectionMode = SelectionMode.MultiExtended;
            // 
            // copyButton
            // 
            resources.ApplyResources(copyButton, "copyButton");
            copyButton.Name = "copyButton";
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += copyButton_Click;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(copyButton);
            Controls.Add(resultListBox);
            Controls.Add(deleteFoldersCheckBox);
            Controls.Add(deselectAllButton);
            Controls.Add(selectAllButton);
            Controls.Add(driveListBox);
            Controls.Add(statusLabel);
            Controls.Add(cancelButton);
            Controls.Add(startButton);
            Name = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button startButton;
        private Button cancelButton;
        private Label statusLabel;
        private CheckedListBox driveListBox;
        private Button selectAllButton;
        private Button deselectAllButton;
        private CheckBox deleteFoldersCheckBox;
        public ListBox resultListBox;
        private Button copyButton;
    }
}