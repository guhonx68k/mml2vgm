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
            ApplyBackgroundSetting(setting);
            this.tbLog.ForeColor = ResolveTextColor(setting.other.PanelTextFontSettings.Log.Color, setting.ColorScheme.Log_ForeColor);
            ApplyFontSetting(setting.other.PanelTextFontSettings.Log);
            ApplyScrollBarTheme(setting);
            theme.ApplyTo(toolStrip1);

            Common.SetDoubleBuffered(this);
            Common.SetDoubleBuffered(tbLog);
        }

        public void ApplyFontSetting(Setting.PanelTextFontSettings.PanelTextFontSetting panelTextFontSetting)
        {
            try
            {
                tbLog.Font = new Font(
                    panelTextFontSetting.Name,
                    panelTextFontSetting.Size,
                    panelTextFontSetting.Style);
            }
            catch
            {
                tbLog.Font = new Font("Consolas", 9.0f, FontStyle.Regular);
            }

            tbLog.ForeColor = ResolveTextColor(panelTextFontSetting.Color, Color.FromArgb(235, 235, 255).ToArgb());
        }

        public void ApplyBackgroundSetting(Setting setting)
        {
            Color backColor = ResolvePanelBackColor(setting, setting.ColorScheme.Log_BackColor);
            this.BackColor = backColor;
            tbLog.BackColor = backColor;
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
