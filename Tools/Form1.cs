using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Aspose.Words;
using Aspose.Words.Saving;
using Microsoft.SqlServer.Server;
using Sunny.UI;
using Sunny.UI.Win32;
using Tools.Utils;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;

namespace Tools
{
    public partial class Form1 : UIForm
    {
        List<string> wordFilesList = new List<string>();
        List<string> ReNameFilesList = new List<string>();
        private const string REGEX_CONFIG_FILE = "regex_favorites.txt";
        
        public Form1()
        {
            InitializeComponent();
            LoadRegexFavorites();
            ZZCHeck.CheckedChanged += ZZCHeck_CheckedChanged;
        }

        // 加载收藏的正则表达式
        private void LoadRegexFavorites()
        {
            try
            {
                if (File.Exists(REGEX_CONFIG_FILE))
                {
                    var favorites = File.ReadAllLines(REGEX_CONFIG_FILE);
                    P2_FindText.Items.Clear();
                    P2_FindText.Items.AddRange(favorites);
                }
            }
            catch (Exception ex)
            {
                ShowCustomErrorTip($"加载正则表达式收藏失败: {ex.Message}");
            }
        }

        // 保存正则表达式到收藏
        private void SaveRegexToFavorites(string regex)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(regex))
                {
                    // 检查是否已经存在
                    var favorites = File.Exists(REGEX_CONFIG_FILE) 
                        ? new List<string>(File.ReadAllLines(REGEX_CONFIG_FILE)) 
                        : new List<string>();

                    if (!favorites.Contains(regex))
                    {
                        favorites.Add(regex);
                        File.WriteAllLines(REGEX_CONFIG_FILE, favorites);
                        
                        // 更新下拉框
                        if (!P2_FindText.Items.Contains(regex))
                        {
                            P2_FindText.Items.Add(regex);
                        }
                        
                        ShowCustomSuccessTip("正则表达式已收藏");
                    }
                    else
                    {
                        ShowCustomErrorTip("该正则表达式已在收藏中");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowCustomErrorTip($"保存正则表达式收藏失败: {ex.Message}");
            }
        }

        // 收藏按钮点击事件
        private void SaveRegexBtn_Click(object sender, EventArgs e)
        {
            if (ZZCHeck.Checked)
            {
                string regex = P2_FindText.Text;
                if (string.IsNullOrWhiteSpace(regex))
                {
                    ShowCustomErrorTip("请输入正则表达式");
                    return;
                }

                // 验证正则表达式是否有效
                if (!StringHelper.IsValidRegex(regex))
                {
                    ShowCustomErrorTip("无效的正则表达式");
                    return;
                }

                SaveRegexToFavorites(regex);
            }
            else
            {
                ShowCustomErrorTip("请先启用正则表达式模式");
            }
        }

        // 正则表达式模式切换事件
        private void ZZCHeck_CheckedChanged(object sender, EventArgs e)
        {
            SaveRegexBtn.Visible = ZZCHeck.Checked;
            if (ZZCHeck.Checked)
            {
                LoadRegexFavorites();
            }
        }

        // 添加错误提示方法
        private void ShowCustomErrorTip(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 添加成功提示方法
        private void ShowCustomSuccessTip(string message)
        {
            MessageBox.Show(message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #region word2pdf
        private void BtnSeachWJJ2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)

                {
                    string selectedPath = folderDialog.SelectedPath;
                    this.folderPath2.Text = selectedPath;
                    if (String.IsNullOrEmpty(targetPath.Text))
                    {

                        targetPath.Text = this.folderPath2.Text + "\\target";
                    }
                    AddFile2List();
                }
            }
        }

        private async void StartChangeBtn2_Click(object sender, EventArgs e)
        {
            bool error = false;
            if (string.IsNullOrEmpty(this.folderPath2.Text))
            {
                MessageBox.Show("请选择转换文件夹");
                error = true;
            }
            if (TypeBox.Items.Count == 0)
            {
                MessageBox.Show("请选择转换类型");
                error = true;
            }
            if (!error)
            {

                uiProcessBar1.Value = 0;
                // 确保输出目录存在
                if (!Directory.Exists(this.folderPath2.Text))
                {
                    Directory.CreateDirectory(this.folderPath2.Text);
                }
                object[] objects = FileList.SelectedItems.ToArray();
                List<string> wordFiles = objects.Cast<string>().ToList();

                List<string> failedFiles = new List<string>(); // 记录失败的文件
                int successCount = 0; // 成功转换的文件计数

                // 异步处理文件转换
                List<Task> tasks = new List<Task>();
                // 输出转换结果

                for (int i = 0; i < wordFiles.Count; i++)
                {
                    int index = i; // 必须在循环中保存索引变量
                    string wordFile = wordFiles[i];

                    // 创建一个任务
                    Task task = Task.Run(() =>
                    {
                        string pdfFilePath = "";
                        if (string.IsNullOrEmpty(this.targetPath.Text))
                        {
                            pdfFilePath = Path.Combine(this.folderPath2.Text + "\\target", Path.GetFileNameWithoutExtension(wordFile) + ".pdf");
                        }
                        else
                        {
                            pdfFilePath = Path.Combine(this.targetPath.Text, Path.GetFileNameWithoutExtension(wordFile) + ".pdf");
                        }

                        try
                        {
                            // 进行文件转换
                            Document doc = new Document(wordFile);
                            PdfSaveOptions saveOptions = new PdfSaveOptions();

                            // 设置PDF加密选项
                            PdfEncryptionDetails encryptionDetails = new PdfEncryptionDetails(string.Empty, "password", PdfEncryptionAlgorithm.RC4_128);
                            encryptionDetails.Permissions = PdfPermissions.AllowAll;
                            saveOptions.EncryptionDetails = encryptionDetails;

                            // 保存为 PDF 文件
                            doc.Save(pdfFilePath, saveOptions);

                            // 更新进度条（通过主线程更新 UI）
                            this.Invoke(new Action(() =>
                            {
                                uiProcessBar1.Value = index + 1; // 更新进度条
                            }));
                            // 成功计数
                            this.Invoke(new Action(() =>
                            {
                                successCount++;
                            }));
                        }
                        catch (Exception ex)
                        {
                            // 记录失败的文件
                            this.Invoke(new Action(() =>
                            {
                                failedFiles.Add(wordFile); // 添加到失败列表
                            }));
                            // 异常处理，记录错误
                            Console.WriteLine($"Error converting file {wordFile}: {ex.Message}");
                        }
                    });

                    // 添加任务到任务列表
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);

                // 弹出提示框，显示成功和失败数量
                MessageBox.Show($"转换完成！\n成功转换 {successCount} 个文件。\n失败 {failedFiles.Count} 个文件。",
                                "转换结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 输出失败的文件列表到目标文件夹
                if (failedFiles.Count > 0)
                {
                    string failedListFilePath = Path.Combine(this.targetPath.Text, "failed_files.txt");
                    System.IO.File.WriteAllLines(failedListFilePath, failedFiles);
                    MessageBox.Show($"失败的文件已保存到 {failedListFilePath}", "失败文件列表", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SeachtargetPathBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    this.targetPath.Text = selectedPath;
                }
            }
        }

        private void OpenTargetBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.targetPath.Text))
            {
                Process.Start("explorer.exe", this.targetPath.Text);
            }
        }


        private void AddFile2List()
        {
            if (!String.IsNullOrEmpty(this.folderPath2.Text))
            {
                FileList.Items.Clear();
                wordFilesList.Clear();

                object[] objects = TypeBox.SelectedItems.ToArray();
                List<string> TypeBoxList = objects.Cast<string>().ToList();
                wordFilesList = FileHelper.FindFiles(folderPath2.Text, TypeBoxList, P1Sun.Checked);
                // 将列表转换为数组
                string[] wordFiles = wordFilesList.ToArray();

                for (int i = 0; i < wordFiles.Length; i++)
                {
                    string wordFile = wordFiles[i];

                    FileList.Items.Add(wordFile);
                }

            }
        }



        private void selectAllBtn_Click(object sender, EventArgs e)
        {
            FileList.SelectAll();
        }
        private void FileList_ValueChanged(object sender, CheckBoxGroupEventArgs e)
        {
            uiProcessBar1.Maximum = FileList.SelectedItems.Count;

        }


        private void NoSeachBtn_Click(object sender, EventArgs e)
        {
            FileList.UnSelectAll();
        }

        private void TypeBox_ValueChanged(object sender, CheckBoxGroupEventArgs e)
        {
            AddFile2List();
        }

        private void P1Sun_CheckedChanged(object sender, EventArgs e)
        {
            AddFile2List();
        }

        #endregion
        #region 批量重名名文件
        private void SeachBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    this.ChangeText.Text = selectedPath;
                }
            }
        }

        private async void ReplaceBtn_Click(object sender, EventArgs e)
        {
            if (Replacetype.SelectedItems.Count != 0)
            {
                P2Pro.Value = 0;
                bool error = false;
                if (string.IsNullOrEmpty(this.ChangeText.Text))
                {
                    MessageBox.Show("请选择替换文件所在文件夹");
                    error = true;
                }
                if (string.IsNullOrEmpty(this.P2_FindText.Text))
                {
                    MessageBox.Show("请填写查找内容");
                    error = true;
                }
                ReNameFilesList.Clear();
                P2Logs.ClearRows();  // 使用新方法清空列表
                
                if (!error)
                {
                    object[] objects = Replacetype.SelectedItems.ToArray();
                    List<string> ReNameTypeList = objects.Cast<string>().ToList();
                    ReNameFilesList = FileHelper.FindFiles(ChangeText.Text, ReNameTypeList, P2Sun.Checked);
                    List<string> ReNameTempList = new List<string>();
                    
                    for (int i = 0; i < ReNameFilesList.Count; i++)
                    {
                        if (!P2_FindText.Text.IsNullOrEmpty())
                        {
                            if (ZZCHeck.Checked)
                            {
                                string customRegexPattern = P2_FindText.Text;
                                if (!Tools.StringHelper.IsValidRegex(customRegexPattern))
                                {
                                    MessageBox.Show("输入的正则表达式无效，请重新输入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                ReNameTempList = ReNameFilesList.Where(file =>
                                    Regex.IsMatch(Path.GetFileName(file), customRegexPattern)).ToList();
                            }
                            else
                            {
                                string wordFile = ReNameFilesList[i];
                                bool containsPattern = Path.GetFileName(wordFile).Contains(P2_FindText.Text);
                                if (containsPattern)
                                {
                                    ReNameTempList.Add(wordFile);
                                }
                            }
                        }
                    }
                    ReNameFilesList = ReNameTempList;
                    List<Task> tasks = new List<Task>();
                    P2Pro.Maximum = ReNameFilesList.Count;
                    
                    for (int i = 0; i < ReNameFilesList.Count; i++)
                    {
                        int index = i;
                        string wordFile = ReNameFilesList[i];
                        P2Pro.Value = i + 1;
                        
                        Task task = Task.Run(() =>
                        {
                            try
                            {
                                string directory = Path.GetDirectoryName(wordFile);
                                string fileName = Path.GetFileName(wordFile);
                                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                                string fileExtension = Path.GetExtension(fileName);
                                string newFileName = fileName;

                                if (ZZCHeck.Checked)
                                {
                                    newFileName = Regex.Replace(fileNameWithoutExtension, P2_FindText.Text, ThText.Text) + fileExtension;
                                }
                                else
                                {
                                    newFileName = fileNameWithoutExtension.Replace(P2_FindText.Text, ThText.Text) + fileExtension;
                                }

                                string newFilePath = Path.Combine(directory, newFileName);
                                
                                // 使用新方法添加行
                                this.Invoke(new Action(() =>
                                {
                                    int rowIndex = P2Logs.AddRow(fileName, newFileName, "处理中...", Color.LightYellow);
                                }));

                                if (System.IO.File.Exists(newFilePath))
                                {
                                    newFilePath = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(newFileName)}_{Guid.NewGuid()}{Path.GetExtension(newFileName)}");
                                }

                                System.IO.File.Move(wordFile, newFilePath);

                                // 使用新方法更新状态
                                this.Invoke(new Action(() =>
                                {
                                    P2Logs.UpdateRowStatus(index, "✓ 成功", Color.LightGreen);
                                }));
                            }
                            catch (Exception ex)
                            {
                                // 使用新方法更新失败状态
                                this.Invoke(new Action(() =>
                                {
                                    P2Logs.UpdateRowStatus(index, "× 失败", Color.LightPink, ex.Message);
                                }));
                            }
                        });

                        tasks.Add(task);
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }

        private void CollectFilesSeachFindPathBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)

                {
                    string selectedPath = folderDialog.SelectedPath;
                    this.CollectFilesFindfilePath.Text = selectedPath;
                    if (String.IsNullOrEmpty(CollectFilesSavePath.Text))
                    {

                        CollectFilesSavePath.Text = this.CollectFilesFindfilePath.Text + "\\target";
                    }
                }
            }
        }
        #endregion

        #region 收集文件
        private void CollectFilesOpenSaveBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CollectFilesSavePath.Text))
            {
                Process.Start("explorer.exe", this.CollectFilesSavePath.Text);
            }
        }

        private void CollectFilesSavePathBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    CollectFilesSavePath.Text = selectedPath;
                }
            }
        }

        private async void CollectFilesStartBtn_Click(object sender, EventArgs e)
        {
            P3Pro.Value = 0;
            // 确保目标目录存在
            if (!Directory.Exists(CollectFilesSavePath.Text))
            {
                Directory.CreateDirectory(CollectFilesSavePath.Text);
            }

            object[] objects = CollectfFilesType.SelectedItems.ToArray();
            List<string> CollectfFilesTypes = objects.Cast<string>().ToList();

            List<string> FindFileList = new List<string>();
            FindFileList = FileHelper.FindFiles(CollectFilesFindfilePath.Text, CollectfFilesTypes, CollectFilesFindSunFIles.Checked);
            List<Task> tasks = new List<Task>();
            P3Pro.Maximum = FindFileList.Count;
            for (int i = 0; i < FindFileList.Count; i++)
            {
                int index = i; // 必须在循环中保存索引变量
                string sFile = FindFileList[i];
                P3Pro.Value = i + 1;
                // 创建一个任务
                Task task = Task.Run(() =>
                {
                    try
                    {
                        // 如果目标文件已经存在，则可以根据需要决定如何处理，比如覆盖或者跳过
                        if (System.IO.File.Exists(sFile))
                        {
                            // 例如：跳过文件拷贝（你可以选择覆盖文件等）
                            CollectFilesLogs.Text += $"文件已存在，跳过拷贝: {sFile}\r\n";
                        }
                        else
                        {
                            // 执行文件拷贝操作
                            System.IO.File.Copy(sFile, CollectFilesSavePath.Text);
                            CollectFilesLogs.Text += $"文件拷贝成功: {sFile}\r\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        CollectFilesLogs.Text += $"文件替换失败: {sFile},{ex.Message}\r\n";
                    }
                });

                // 添加任务到任务列表
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }



        #endregion

        #region pdf文件操作
        private Task _currentTask;
        private void p4_SeachFileBtn_Click(object sender, EventArgs e)
        {
            // 创建 OpenFileDialog 实例
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置文件类型过滤器，只允许选择 PDF 文件
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";

            // 设置是否允许多选
            openFileDialog.Multiselect = false;  // 设置为 true 允许多选文件

            // 打开文件对话框并判断用户是否选择了文件
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取选中的文件路径
                string selectedPdfFile = openFileDialog.FileName;

                p4_PdfFilePath.Text = selectedPdfFile;
            }
        }
        private async void p4_StartBtn_Click(object sender, EventArgs e)
        {
            p4_waitbar.Visible = true;
            p4_StartBtn.Enabled = false;

            // 创建CancellationTokenSource

            try
            {
                // 调用异步方法并传递CancellationToken
                _currentTask = pdf2word.ConvertWordToImagesAsync(p4_PdfFilePath.Text);

                // 等待任务完成
                await _currentTask;
            }
            catch (OperationCanceledException)
            {
                // 任务被取消时的处理
                MessageBox.Show("任务被取消");
            }
            finally
            {
                p4_waitbar.Visible = false;
                p4_StartBtn.Enabled = true;
            }

        }
    
  
        #endregion

        #region svg2ico
        private void svg2ico_SeachSvgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "SVG Files (*.svg)|*.svg";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    svg2ico_SvgPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void svg2ico_SavePathBtn_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ICO Files (*.ico)|*.ico";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "ico";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    svg2ico_SavePath.Text = saveFileDialog.FileName;
                }
            }
        }

        private async void svg2ico_StartBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(svg2ico_SvgPath.Text) || string.IsNullOrEmpty(svg2ico_SavePath.Text))
            {
                ShowCustomErrorTip("请选择SVG文件和保存路径");
                return;
            }

            try
            {
                svg2ico_waitbar.Visible = true;
                svg2ico_StartBtn.Enabled = false;

                await Task.Run(() =>
                {
                    // 使用第三方库进行转换
                    var svgDocument = Svg.SvgDocument.Open(svg2ico_SvgPath.Text);
                    
                    // 创建不同尺寸的位图
                    var sizes = new[] { 16, 32, 48, 64, 128, 256 };
                    var images = new List<Bitmap>();
                    
                    foreach (var size in sizes)
                    {
                        var bitmap = new Bitmap(svgDocument.Draw(size, size));
                        images.Add(bitmap);
                    }

                    // 保存为ICO文件
                    using (var stream = new FileStream(svg2ico_SavePath.Text, FileMode.Create))
                    {
                        IconFactory.SavePngsAsIcon(images.ToArray(), stream);
                    }
                });

                ShowCustomSuccessTip("转换完成");
            }
            catch (Exception ex)
            {
                ShowCustomErrorTip($"转换失败: {ex.Message}");
            }
            finally
            {
                svg2ico_waitbar.Visible = false;
                svg2ico_StartBtn.Enabled = true;
            }
        }
        #endregion

    }
}
