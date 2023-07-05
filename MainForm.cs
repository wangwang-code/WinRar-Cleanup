using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WinRAR垃圾清理
{
    public partial class MainForm : Form
    {
        private string pattern = "^Rar\\$[A-Za-z0-9]+";
        private List<DriveInfo> drives;
        private BackgroundWorker backgroundWorker;
        private List<string> output;


        // 在 MainForm 类的顶部定义 WorkerArguments 类
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

            // 初始化后台任务
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true; // 添加此行
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 获取所有驱动器
            drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).ToList();

            // 显示驱动器列表
            driveListBox.Items.AddRange(drives.Select(d => d.Name).ToArray());

            // 全选按钮点击事件处理
            selectAllButton.Click += (s, ev) =>
            {
                for (int i = 0; i < driveListBox.Items.Count; i++)
                {
                    driveListBox.SetItemChecked(i, true);
                }
            };

            // 取消选择按钮点击事件处理
            deselectAllButton.Click += (s, ev) =>
            {
                for (int i = 0; i < driveListBox.Items.Count; i++)
                {
                    driveListBox.SetItemChecked(i, false);
                }
            };
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            // 清空结果列表框
            resultListBox.Items.Clear();

            if (!backgroundWorker.IsBusy)
            {
                // 禁用驱动器列表框
                driveListBox.Enabled = false;
                selectAllButton.Enabled = false;
                deselectAllButton.Enabled = false;

                // 清空输出
                output = new List<string>();
                // 禁用开始按钮和取消按钮
                startButton.Enabled = false;
                cancelButton.Enabled = true;

                var selectedDrives = driveListBox.CheckedItems.Cast<string>().ToList();

                if (selectedDrives.Count == 0)
                {
                    MessageBox.Show("请选择至少一个驱动器。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // 启用驱动器列表框
                    driveListBox.Enabled = true;
                    selectAllButton.Enabled = true;
                    deselectAllButton.Enabled = true;
                    startButton.Enabled = true;
                    cancelButton.Enabled = false;
                    return;
                }

                // 启动后台任务
                backgroundWorker.RunWorkerAsync(new WorkerArguments(selectedDrives, !deleteFoldersCheckBox.Checked));
            }
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy && !backgroundWorker.CancellationPending)
            {
                // 取消后台任务
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

                // 添加输出语句
                var message = "正在扫描驱动器" + driveName;
                statusLabel.Invoke(new Action(() => statusLabel.Text = message));

                worker.ReportProgress(0);
                Thread.Sleep(1500);

                var folders = Directory.GetDirectories(rootPath);

                foreach (var folderPath in folders)
                {
                    var folderName = Path.GetFileName(folderPath);

                    if (Regex.IsMatch(folderName, pattern, RegexOptions.IgnoreCase)) // 使用正则表达式匹配
                    {
                        // 将找到的文件夹路径传递给ProgressChanged事件
                        worker.ReportProgress(0, folderPath);
                        // 判断是否删除文件夹
                        if (arguments.DeleteFolders)
                        {
                            try
                            {
                                Directory.Delete(folderPath, true);
                                output.Add($"删除！文件夹： {folderPath}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
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
                // 在UI线程上更新resultListBox
                resultListBox.Items.Add($"找到：{folderPath}");
                // 添加以下代码，以显示删除文件夹的消息
                if (output.Count > 0)
                {
                    var lastMessage = output.Last();
                    resultListBox.Items.Add(lastMessage);
                }
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
                statusLabel.Text = output.Count == 0 ? "未找到匹配的文件夹。" : "文件夹列表:";

                foreach (var item in output)
                {
                    resultListBox.Items.Add(item);
                }
            }

            // 启用开始按钮和取消按钮
            startButton.Enabled = true;
            cancelButton.Enabled = false;

            // 启用驱动器列表框
            driveListBox.Enabled = true;
            selectAllButton.Enabled = true;
            deselectAllButton.Enabled = true;
        }
    }
}