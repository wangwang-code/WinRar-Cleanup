using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WinRAR垃圾清理
{
    public partial class MainForm : Form
    {
        // 正则表达式用于匹配WinRAR创建的临时文件夹
        private string pattern = "^Rar\\$[A-Za-z0-9]+";
        private List<DriveInfo> drives;
        private BackgroundWorker backgroundWorker;
        private List<string> output;

        public class WorkerArguments
        {
            public List<string> SelectedDrives { get; set; }
            public bool DeleteFolders { get; set; }

            public WorkerArguments(List<string> selectedDrives, bool deleteFolders)
            {
                SelectedDrives = selectedDrives;
                DeleteFolders = deleteFolders;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
            notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Resize += MainForm_Resize;
            notifyIcon1.Click += notifyIcon1_DoubleClick;

            drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).ToList();

            driveListBox.Items.AddRange(drives.Select(d => d.Name).ToArray());

            selectAllButton.Click += (s, ev) => SetAllItemsChecked(true);

            deselectAllButton.Click += (s, ev) => SetAllItemsChecked(false);

            about.Click += (s, ev) =>
            {
                string message = "扫描所有驱动器并删除（非抹除，可恢复）正则匹配 “^Rar\\$[A-Za-z0-9]+”的文件夹 \n点���将打开源码链接";
                string caption = "关于";

                DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                if (result == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://github.com/wangwang-code/WinRar-Cleanup") { UseShellExecute = true });
                }
            };
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            resultListBox.Items.Clear();

            if (!backgroundWorker.IsBusy)
            {
                driveListBox.Enabled = false;
                selectAllButton.Enabled = false;
                deselectAllButton.Enabled = false;

                output = new List<string>();
                startButton.Enabled = false;
                cancelButton.Enabled = true;

                var selectedDrives = driveListBox.CheckedItems.Cast<string>().ToList();

                if (selectedDrives.Count == 0)
                {
                    MessageBox.Show("请选择至少一个驱动器。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    driveListBox.Enabled = true;
                    selectAllButton.Enabled = true;
                    deselectAllButton.Enabled = true;
                    startButton.Enabled = true;
                    cancelButton.Enabled = false;
                    return;
                }

                backgroundWorker.RunWorkerAsync(new WorkerArguments(selectedDrives, !deleteFoldersCheckBox.Checked));
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy && !backgroundWorker.CancellationPending)
            {
                backgroundWorker.CancelAsync();
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as WorkerArguments;
            foreach (var driveName in arguments.SelectedDrives)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var driveInfo = drives.First(d => d.Name == driveName);
                var rootPath = driveInfo.RootDirectory.FullName;

                var message = "正在扫描驱动器" + driveName;
                statusLabel.Invoke((MethodInvoker)delegate {
                    statusLabel.Text = message; // 在UI线程上更新statusLabel
                });

                worker.ReportProgress(0);
                Thread.Sleep(1500);

                var folders = Directory.GetDirectories(rootPath);

                foreach (var folderPath in folders)
                {
                    var folderName = Path.GetFileName(folderPath);
                    if (Regex.IsMatch(folderName, pattern, RegexOptions.IgnoreCase))
                    {
                        worker.ReportProgress(0, folderPath);
                        if (arguments.DeleteFolders)
                        {
                            try
                            {
                                Directory.Delete(folderPath, true);
                                output.Add($"删除！文件夹： {folderPath}");
                            }
                            catch (Exception ex)
                            {
                                // 记录错误日志
                                File.AppendAllText("error.log", ex.ToString());
                                // 更简洁明了的错误消息
                                MessageBox.Show($"无法删除文件夹：{folderPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            output.Add($"跳过删除文件夹：{folderPath}");
                        }
                    }
                }
            }
            worker.ReportProgress(0);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                var folderPath = e.UserState.ToString();

                resultListBox.Invoke((MethodInvoker)delegate {
                    // 在UI线程上更新resultListBox
                    resultListBox.Items.Add($"找到：{folderPath}");
                });
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                statusLabel.Text = "操作已取消";
            }
            else
            {
                statusLabel.Text = output.Count == 0 ? "没有垃圾文件夹" : "垃圾文件夹——>";

                foreach (var item in output)
                {
                    resultListBox.Items.Add(item); // 在此处添加output列表中的所有删除文件夹的消息
                }
            }

            // 启用开始按钮和取消按钮
            startButton.Enabled = true;
            cancelButton.Enabled = false;
            driveListBox.Enabled = true;
            selectAllButton.Enabled = true;
            deselectAllButton.Enabled = true;
        }

        private void SetAllItemsChecked(bool isChecked)
        {
            for (int i = 0; i < driveListBox.Items.Count; i++)
            {
                driveListBox.SetItemChecked(i, isChecked);
            }
        }
    }
}