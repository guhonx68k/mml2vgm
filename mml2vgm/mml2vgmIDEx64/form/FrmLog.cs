using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace mml2vgmIDEx64
{
    public partial class FrmLog : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public Action parentUpdate = null;

        public FrmLog(Setting setting, ThemeBase theme)
        {
            InitializeComponent();
            this.tbLog.BackColor = Color.FromArgb(setting.ColorScheme.Log_BackColor);
            this.tbLog.ForeColor = Color.FromArgb(setting.ColorScheme.Log_ForeColor);
            ApplyFontSetting(setting);
            ApplyScrollBarTheme(setting);
            theme.ApplyTo(toolStrip1);

            Common.SetDoubleBuffered(this);
            Common.SetDoubleBuffered(tbLog);
        }

        public void ApplyFontSetting(Setting setting)
        {
            try
            {
                tbLog.Font = new Font(setting.other.LogTextFontName, setting.other.LogTextFontSize, setting.other.LogTextFontStyle);
            }
            catch
            {
                tbLog.Font = new Font("Consolas", 9.0f, FontStyle.Regular);
            }
        }

        public void ApplyScrollBarTheme(Setting setting)
        {
            Common.ApplyScrollBarTheme(tbLog, setting.other.ScrollBarTheme);
        }

        public void ClearLog()
        {
            tbLog.Clear();
        }

        protected override string GetPersistString()
        {
            return this.Name;
        }

        private void FrmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            parentUpdate?.Invoke();
        }

        private void TbLog_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void TsbClearLog_Click(object sender, EventArgs e)
        {
            ClearLog();
        }
    }
}
