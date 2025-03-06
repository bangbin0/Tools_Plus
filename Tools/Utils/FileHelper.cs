using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class FileHelper
    {
        // 字典映射文件类型到扩展名
        private static readonly Dictionary<string, string[]> fileExtensions = new Dictionary<string, string[]>
    {
        { "DOC", new[] { "*.doc" } },
        { "DOCX", new[] { "*.docx" } },
        { "WPS", new[] { "*.wps" } },
        { "PDF", new[] { "*.pdf" } },
        { "XLS", new[] { "*.xls" } },
        { "XLSX", new[] { "*.xlsx" } },
        { "TXT", new[] { "*.txt" } },
        { "BAT", new[] { "*.bat" } },
        { "PNG", new[] { "*.png" } },
        { "JPEG", new[] { "*.jpeg", "*.jpg" } },
        { "LRMX", new[] { "*.lrmx" } }
    };

        /// <summary>
        /// 查找指定类型的文件
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        /// <param name="fileTypes">需要查找的文件类型</param>
        /// <returns>符合条件的文件路径列表</returns>
        public static List<string> FindFiles(string directoryPath, List<string> fileTypes, bool includeSubdirectories)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                return new List<string>();
            }

            List<string> foundFiles = new List<string>();

            try
            {
                var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                // 获取目录下的所有文件
                var allFiles = Directory.GetFiles(directoryPath, "*.*", searchOption);

                // 遍历所有文件，检查是否匹配所选类型
                foreach (var file in allFiles)
                {
                    string fileExtension = Path.GetExtension(file).ToLower();
                    
                    // 检查文件是否匹配任何选中的类型
                    foreach (var fileType in fileTypes)
                    {
                        if (fileExtensions.ContainsKey(fileType))
                        {
                            var validExtensions = fileExtensions[fileType]
                                .Select(ext => ext.Substring(1).ToLower()) // 去掉*号并转换为小写
                                .ToList();

                            if (validExtensions.Contains(fileExtension))
                            {
                                foundFiles.Add(file);
                                break; // 找到匹配就跳出内层循环
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 发生异常时返回空列表
                return new List<string>();
            }

            return foundFiles;
        }


        /// <summary>
        /// 复制文件到目标目录
        /// </summary>
        /// <param name="sourceFiles">源文件列表</param>
        /// <param name="destinationDirectory">目标目录</param>
        public static void CopyFiles(List<string> sourceFiles, string destinationDirectory)
        {
            if (string.IsNullOrEmpty(destinationDirectory) || !Directory.Exists(destinationDirectory))
            {
                throw new ArgumentException("无效的目标目录", nameof(destinationDirectory));
            }

            foreach (var file in sourceFiles)
            {
                string destFile = Path.Combine(destinationDirectory, Path.GetFileName(file));

                // 如果目标文件已经存在，重命名它
                if (File.Exists(destFile))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    string extension = Path.GetExtension(file);
                    int counter = 1;

                    // 生成一个新的文件名，避免覆盖
                    while (File.Exists(destFile))
                    {
                        destFile = Path.Combine(destinationDirectory, $"{fileNameWithoutExtension}({counter++}){extension}");
                    }
                }

                // 复制文件
                File.Copy(file, destFile);
            }
        }

        /// <summary>
        /// 删除指定路径的文件
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        public static void DeleteFiles(List<string> filePaths)
        {
            foreach (var file in filePaths)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// 获取文件扩展名列表
        /// </summary>
        /// <param name="fileTypes">文件类型列表</param>
        /// <returns>对应的扩展名列表</returns>
        public static List<string> GetExtensions(List<string> fileTypes)
        {
            List<string> extensions = new List<string>();

            foreach (var fileType in fileTypes)
            {
                if (fileExtensions.ContainsKey(fileType))
                {
                    extensions.AddRange(fileExtensions[fileType]);
                }
            }

            return extensions;
        }
    }

}
