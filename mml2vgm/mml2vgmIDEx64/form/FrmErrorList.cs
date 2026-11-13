using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace mml2vgmIDEx64
{
    public partial class FrmErrorList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public Action parentUpdate = null;
        public Action<string, long, bool> parentJumpDocument = null;
        private ContextMenuStrip errorListContextMenu = null;
        private ToolStripMenuItem tsmiCopyRow = null;
        private ToolStripMenuItem tsmiCopyAll = null;
        private bool currentCellClearPending = false;
        private int activeRowIndex = -1;

        public FrmErrorList(Setting setting)
        {
            InitializeComponent();
            InitializeHeaderCells();
            ApplyBackgroundSetting(setting);
            this.dataGridView1.ForeColor = ResolveTextColor(setting.other.PanelTextFontSettings.ErrorList.Color, setting.ColorScheme.ErrorList_ForeColor);
            ApplyFontSetting(setting.other.PanelTextFontSettings.ErrorList);
            ApplyScrollBarTheme(setting);

            Common.SetDoubleBuffered(dataGridView1);
            InitializeContextMenu();
            dataGridView1.ColumnHeaderMouseClick += DataGridView1_ColumnHeaderMouseClick;
            dataGridView1.GotFocus += DataGridView1_GotFocus;
            dataGridView1.RowsAdded += DataGridView1_RowsChanged;
            dataGridView1.RowsRemoved += DataGridView1_RowsChanged;
            dataGridView1.TabStop = false;
            UpdateSortedHeaderStyle();
            ScheduleClearCurrentCellFocus();
            UpdateStatusSummary();
        }

        private void InitializeContextMenu()
        {
            errorListContextMenu = new ContextMenuStrip();
            errorListContextMenu.ShowImageMargin = false;
            errorListContextMenu.ShowCheckMargin = false;
            tsmiCopyRow = new ToolStripMenuItem("この行をコピー");
            tsmiCopyAll = new ToolStripMenuItem("全件コピー");

            tsmiCopyRow.Click += TsmiCopyRow_Click;
            tsmiCopyAll.Click += TsmiCopyAll_Click;

            errorListContextMenu.Items.AddRange(new ToolStripItem[]
            {
                tsmiCopyRow,
                tsmiCopyAll
            });

            dataGridView1.ContextMenuStrip = errorListContextMenu;
        }

        private void InitializeHeaderCells()
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                ColorSortGlyphHeaderCell headerCell = new ColorSortGlyphHeaderCell();
                headerCell.Value = column.HeaderText;
                headerCell.Style = column.HeaderCell.Style;
                column.HeaderCell = headerCell;
            }
        }

        public void ApplyFontSetting(Setting.PanelTextFontSettings.PanelTextFontSetting panelTextFontSetting)
        {
            Font font;
            try
            {
                font = new Font(
                    panelTextFontSetting.Name,
                    panelTextFontSetting.Size,
                    panelTextFontSetting.Style);
            }
            catch
            {
                font = new Font("Consolas", 9.0f, FontStyle.Regular);
            }

            dataGridView1.DefaultCellStyle.Font = font;
            dataGridView1.RowsDefaultCellStyle.Font = font;
            dataGridView1.RowTemplate.DefaultCellStyle.Font = font;
            Color textColor = ResolveTextColor(panelTextFontSetting.Color, Color.FromArgb(235, 235, 255).ToArgb());
            dataGridView1.ForeColor = textColor;
            dataGridView1.DefaultCellStyle.ForeColor = textColor;
            dataGridView1.RowsDefaultCellStyle.ForeColor = textColor;
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = textColor;
            statusStrip1.ForeColor = textColor;
            tsslErrorCount.ForeColor = textColor;
            tsslWarningCount.ForeColor = textColor;
            UpdateRowHighlight(activeRowIndex);
        }

        public void ApplyBackgroundSetting(Setting setting)
        {
            Color backColor = ResolvePanelBackColor(setting, setting.ColorScheme.ErrorList_BackColor);
            this.BackColor = backColor;
            dataGridView1.BackgroundColor = backColor;
            dataGridView1.DefaultCellStyle.BackColor = backColor;
            dataGridView1.RowsDefaultCellStyle.BackColor = backColor;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = backColor;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = backColor;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.BackColor = backColor;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = backColor;
                }
            }

            statusStrip1.BackColor = backColor;

            UpdateRowHighlight(activeRowIndex);
        }

        private static Color ResolveTextColor(int otherTextColor, int fallbackColor)
        {
            return otherTextColor == Color.Empty.ToArgb() || otherTextColor == 0
                ? Color.FromArgb(fallbackColor)
                : Color.FromArgb(otherTextColor);
        }

        private static Color ResolvePanelBackColor(Setting setting, int fallbackColor)
        {
            int commonBackColor = setting?.other?.CommonPanelBackColor ?? 0;
            return commonBackColor == Color.Empty.ToArgb() || commonBackColor == 0
                ? Color.FromArgb(fallbackColor)
                : Color.FromArgb(commonBackColor);
        }

        public void ApplyScrollBarTheme(Setting setting)
        {
            Common.ApplyScrollBarTheme(dataGridView1, setting.other.ScrollBarTheme);
        }
        protected override string GetPersistString()
        {
            return this.Name;
        }

        private void FrmErrorList_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            parentUpdate?.Invoke();

        }

        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            SetActiveRow(e.RowIndex);
            if (e.Button == MouseButtons.Right)
            {
                ScheduleClearCurrentCellFocus();
                return;
            }
            JumpDocument(e.RowIndex, false);
            ScheduleClearCurrentCellFocus();
        }

        private void DataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            SetActiveRow(e.RowIndex);
            JumpDocument(e.RowIndex, false);
            ScheduleClearCurrentCellFocus();
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count < 1 || dataGridView1.SelectedRows[0].Index < 0) return;
            int rowIndex = dataGridView1.SelectedRows[0].Index;
            if (activeRowIndex != rowIndex)
            {
                activeRowIndex = rowIndex;
                UpdateRowHighlight(activeRowIndex);
            }
            JumpDocument(rowIndex, false);
            ScheduleClearCurrentCellFocus();
        }

        private void JumpDocument(int rowIndex, bool wantFocus)
        {
            if (rowIndex < 0 || rowIndex > dataGridView1.Rows.Count - 1) return;
            string f = dataGridView1.Rows[rowIndex].Cells[2].Value.ToString();
            string l = dataGridView1.Rows[rowIndex].Cells[3].Value.ToString();
            long n = 0;

            if (long.TryParse(l, out n))
            {
                parentJumpDocument?.Invoke(f, n, wantFocus);
            }
            else
            {
                parentJumpDocument?.Invoke("", -1, wantFocus);
            }
        }

        public void AddErrorRow(string level, string fileName, string line, string message)
        {
            int sequence = dataGridView1.Rows.Count + 1;
            dataGridView1.Rows.Add(sequence.ToString(), level, fileName, line, message);
            ScheduleClearCurrentCellFocus();
            if (activeRowIndex >= dataGridView1.Rows.Count) activeRowIndex = -1;
            UpdateRowHighlight(activeRowIndex);
            UpdateStatusSummary();
        }

        public void ClearErrorRows()
        {
            dataGridView1.Rows.Clear();
            activeRowIndex = -1;
            UpdateStatusSummary();
        }

        private void FrmErrorList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void FrmErrorList_Load(object sender, EventArgs e)
        {

        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateSortedHeaderStyle();
            ScheduleClearCurrentCellFocus();
        }

        private void DataGridView1_GotFocus(object sender, EventArgs e)
        {
            ScheduleClearCurrentCellFocus();
        }

        private void TsmiCopyRow_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count < 1) return;
            string line = BuildRowText(dataGridView1.SelectedRows[0]);
            if (string.IsNullOrEmpty(line)) return;
            Clipboard.SetText(line);
        }

        private void TsmiCopyAll_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 1) return;

            StringBuilder sb = new StringBuilder();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string line = BuildRowText(row);
                if (string.IsNullOrEmpty(line)) continue;
                if (sb.Length > 0) sb.AppendLine();
                sb.Append(line);
            }

            if (sb.Length < 1) return;
            Clipboard.SetText(sb.ToString());
        }

        private string BuildRowText(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return "";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (i > 0) sb.Append('\t');
                object value = row.Cells[i]?.Value;
                sb.Append(value?.ToString() ?? "");
            }

            return sb.ToString();
        }

        private void UpdateSortedHeaderStyle()
        {
            DataGridViewColumn sortedColumn = dataGridView1.SortedColumn;
            bool hasSortedColumn = sortedColumn != null && dataGridView1.SortOrder != SortOrder.None;

            Color normalBackColor = dataGridView1.ColumnHeadersDefaultCellStyle.BackColor;
            Color normalForeColor = dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor;
            Color sortedBackColor = dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor;
            Color sortedForeColor = dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                bool isSortedColumn = hasSortedColumn && column == sortedColumn;
                column.HeaderCell.Style.BackColor = isSortedColumn ? sortedBackColor : normalBackColor;
                column.HeaderCell.Style.ForeColor = isSortedColumn ? sortedForeColor : normalForeColor;
            }

            dataGridView1.Invalidate();
        }

        private void SetActiveRow(int rowIndex)
        {
            activeRowIndex = rowIndex;
            dataGridView1.ClearSelection();
            if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows[rowIndex].Selected = true;
            }
            UpdateRowHighlight(activeRowIndex);
        }

        private void UpdateRowHighlight(int rowIndex)
        {
            Color normalBackColor = dataGridView1.DefaultCellStyle.BackColor;
            Color normalForeColor = dataGridView1.DefaultCellStyle.ForeColor;
            Color activeBackColor = GetActiveRowBackColor(normalBackColor);
            Color activeForeColor = GetReadableTextColor(activeBackColor);

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                bool isActive = i == rowIndex;
                Color backColor = isActive ? activeBackColor : normalBackColor;
                Color foreColor = isActive ? activeForeColor : normalForeColor;

                row.DefaultCellStyle.BackColor = backColor;
                row.DefaultCellStyle.ForeColor = foreColor;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = backColor;
                    cell.Style.ForeColor = foreColor;
                    cell.Style.SelectionBackColor = backColor;
                    cell.Style.SelectionForeColor = foreColor;
                }
            }

            dataGridView1.Invalidate();
        }

        private static Color GetActiveRowBackColor(Color baseColor)
        {
            const int delta = 28;
            bool brighten = baseColor.GetBrightness() <= 0.5f;
            int r = brighten ? Math.Min(255, baseColor.R + delta) : Math.Max(0, baseColor.R - delta);
            int g = brighten ? Math.Min(255, baseColor.G + delta) : Math.Max(0, baseColor.G - delta);
            int b = brighten ? Math.Min(255, baseColor.B + delta) : Math.Max(0, baseColor.B - delta);
            return Color.FromArgb(r, g, b);
        }

        private static Color GetReadableTextColor(Color backColor)
        {
            int luminance = (backColor.R * 299 + backColor.G * 587 + backColor.B * 114) / 1000;
            return luminance >= 128 ? Color.Black : Color.White;
        }

        private void ScheduleClearCurrentCellFocus()
        {
            if (currentCellClearPending || !IsHandleCreated) return;
            currentCellClearPending = true;

            BeginInvoke(new Action(() =>
            {
                currentCellClearPending = false;
                if (dataGridView1.IsDisposed) return;
                if (dataGridView1.CurrentCell != null)
                {
                    dataGridView1.CurrentCell = null;
                }
            }));
        }

        private void DataGridView1_RowsChanged(object sender, EventArgs e)
        {
            UpdateStatusSummary();
        }

        private void UpdateStatusSummary()
        {
            int errorCount = 0;
            int warningCount = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                string level = row.Cells[1]?.Value?.ToString() ?? "";
                if (level.StartsWith("Error", StringComparison.OrdinalIgnoreCase) || level.StartsWith("エラー", StringComparison.OrdinalIgnoreCase))
                {
                    errorCount++;
                    continue;
                }

                if (level.StartsWith("Warning", StringComparison.OrdinalIgnoreCase) || level.StartsWith("警告", StringComparison.OrdinalIgnoreCase))
                {
                    warningCount++;
                }
            }

            tsslErrorCount.Text = errorCount.ToString();
            tsslWarningCount.Text = warningCount.ToString();
        }

        private sealed class ColorSortGlyphHeaderCell : DataGridViewColumnHeaderCell
        {
            protected override void Paint(
                Graphics graphics,
                Rectangle clipBounds,
                Rectangle cellBounds,
                int rowIndex,
                DataGridViewElementStates dataGridViewElementState,
                object value,
                object formattedValue,
                string errorText,
                DataGridViewCellStyle cellStyle,
                DataGridViewAdvancedBorderStyle advancedBorderStyle,
                DataGridViewPaintParts paintParts)
            {
                base.Paint(
                    graphics,
                    clipBounds,
                    cellBounds,
                    rowIndex,
                    dataGridViewElementState,
                    value,
                    formattedValue,
                    errorText,
                    cellStyle,
                    advancedBorderStyle,
                    paintParts);

                if (SortGlyphDirection == SortOrder.None) return;

                const int glyphWidth = 8;
                const int glyphHeight = 5;
                const int margin = 6;

                int glyphX = DataGridView != null && DataGridView.RightToLeft == RightToLeft.Yes
                    ? cellBounds.Left + margin
                    : cellBounds.Right - glyphWidth - margin;
                int glyphY = cellBounds.Y + (cellBounds.Height - glyphHeight) / 2;
                Rectangle glyphRect = new Rectangle(glyphX, glyphY, glyphWidth, glyphHeight);

                Color backColor = cellStyle?.BackColor ?? SystemColors.Control;
                Color foreColor = cellStyle?.ForeColor ?? SystemColors.ControlText;

                using (SolidBrush backBrush = new SolidBrush(backColor))
                {
                    graphics.FillRectangle(backBrush, glyphRect.X - 1, glyphRect.Y - 1, glyphRect.Width + 2, glyphRect.Height + 2);
                }

                Point[] points = SortGlyphDirection == SortOrder.Ascending
                    ? new[]
                    {
                        new Point(glyphRect.Left, glyphRect.Bottom - 1),
                        new Point(glyphRect.Right - 1, glyphRect.Bottom - 1),
                        new Point(glyphRect.Left + glyphRect.Width / 2, glyphRect.Top)
                    }
                    : new[]
                    {
                        new Point(glyphRect.Left, glyphRect.Top),
                        new Point(glyphRect.Right - 1, glyphRect.Top),
                        new Point(glyphRect.Left + glyphRect.Width / 2, glyphRect.Bottom - 1)
                    };

                using (SolidBrush glyphBrush = new SolidBrush(foreColor))
                {
                    graphics.FillPolygon(glyphBrush, points);
                }
            }
        }
    }
}
