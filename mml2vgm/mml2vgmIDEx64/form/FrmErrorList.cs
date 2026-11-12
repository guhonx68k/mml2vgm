using System;
using System.Drawing;
using System.Windows.Forms;

namespace mml2vgmIDEx64
{
    public partial class FrmErrorList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public Action parentUpdate = null;
        public Action<string, long, bool> parentJumpDocument = null;

        public FrmErrorList(Setting setting)
        {
            InitializeComponent();
            ApplyBackgroundSetting(setting);
            this.dataGridView1.ForeColor = ResolveTextColor(setting.other.PanelTextFontSettings.ErrorList.Color, setting.ColorScheme.ErrorList_ForeColor);
            ApplyFontSetting(setting.other.PanelTextFontSettings.ErrorList);
            ApplyScrollBarTheme(setting);

            Common.SetDoubleBuffered(dataGridView1);
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
            int rowIndex = e.RowIndex;
            JumpDocument(rowIndex, false);
        }

        private void DataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int rowIndex = e.RowIndex;
            JumpDocument(rowIndex, true);
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count < 1 || dataGridView1.SelectedRows[0].Index < 0) return;
            int rowIndex = dataGridView1.SelectedRows[0].Index;
            JumpDocument(rowIndex, false);
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
        }

        private void FrmErrorList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void FrmErrorList_Load(object sender, EventArgs e)
        {

        }
    }
}
