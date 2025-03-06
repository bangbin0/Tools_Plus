using System;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace Tools.Controls
{
    public class UILogListView : UIPanel
    {
        private UIDataGridView dataGridView;

        public UILogListView()
        {
            InitializeComponent();
            this.ShowText = false;
            this.FillColor = Color.White;
            this.Padding = new Padding(0);
        }

        private void InitializeComponent()
        {
            dataGridView = new UIDataGridView();
            ((System.ComponentModel.ISupportInitialize)(dataGridView)).BeginInit();

            // 设置DataGridView的基本属性
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Font = new Font("宋体", 12F);
            dataGridView.GridColor = Color.FromArgb(80, 160, 255);
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowTemplate.Height = 29;
            dataGridView.ScrollBars = ScrollBars.Both;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(235, 243, 255)
            };
            dataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(155, 200, 255),
                SelectionForeColor = Color.Black
            };

            // 添加列
            var columnBefore = new DataGridViewTextBoxColumn
            {
                HeaderText = "原文件名",
                Name = "ColumnBefore",
                FillWeight = 40,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };

            var columnAfter = new DataGridViewTextBoxColumn
            {
                HeaderText = "新文件名",
                Name = "ColumnAfter",
                FillWeight = 40,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };

            var columnStatus = new DataGridViewTextBoxColumn
            {
                HeaderText = "状态",
                Name = "ColumnStatus",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };

            dataGridView.Columns.AddRange(new DataGridViewColumn[] {
                columnBefore,
                columnAfter,
                columnStatus
            });

            // 添加到控件
            this.Controls.Add(dataGridView);
            ((System.ComponentModel.ISupportInitialize)(dataGridView)).EndInit();
        }

        // 清空所有行
        public void ClearRows()
        {
            dataGridView.Rows.Clear();
        }

        // 添加新行并返回行索引
        public int AddRow(string originalName, string newName, string status = "处理中...", Color? backColor = null)
        {
            int rowIndex = dataGridView.Rows.Add(originalName, newName, status);
            if (backColor.HasValue)
            {
                dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = backColor.Value;
            }
            return rowIndex;
        }

        // 更新行状态
        public void UpdateRowStatus(int rowIndex, string status, Color backColor, string toolTip = "")
        {
            if (rowIndex >= 0 && rowIndex < dataGridView.Rows.Count)
            {
                var row = dataGridView.Rows[rowIndex];
                row.Cells[2].Value = status;
                row.DefaultCellStyle.BackColor = backColor;
                if (!string.IsNullOrEmpty(toolTip))
                {
                    row.Cells[2].ToolTipText = toolTip;
                }
            }
        }

        // 获取指定行
        public DataGridViewRow GetRow(int index)
        {
            if (index >= 0 && index < dataGridView.Rows.Count)
            {
                return dataGridView.Rows[index];
            }
            return null;
        }
    }
} 