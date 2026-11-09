using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace mml2vgmIDEx64
{
    public class FrmFormatSetting : Form
    {
        private sealed class FontSettingValue
        {
            public string Name;
            public float Size;
            public FontStyle Style;

            public FontSettingValue(string name, float size, FontStyle style)
            {
                Name = name;
                Size = size;
                Style = style;
            }

            public FontSettingValue Copy()
            {
                return new FontSettingValue(Name, Size, Style);
            }
        }

        private readonly ListBox lstCategory;
        private readonly Panel pnlMainArea;
        private readonly Panel pnlContent;
        private readonly Panel pnlButtons;
        private readonly Button btnOK;
        private readonly Button btnCancel;
        private readonly Dictionary<string, Control> categoryPages;
        private readonly HashSet<string> fontCategories;
        private readonly Dictionary<string, FontSettingValue> categoryFontValues;
        private readonly Dictionary<string, FontSettingValue> defaultCategoryFontValues;
        private readonly GroupBox gbFont;
        private readonly Label lblFontSummary;
        private readonly Button btnFontSelect;
        private readonly Button btnFontReset;
        private readonly Label lblTargetCategory;
        private Font previewFont;
        private string activeFontCategory = "Editor";

        public Setting setting { get; private set; }

        public FrmFormatSetting(Setting setting)
        {
            this.setting = setting.Copy();
            Setting defaultSetting = new Setting();

            fontCategories = new HashSet<string>
            {
                "Editor",
                "Part counter",
                "Folder",
                "Error list",
                "Log"
            };

            categoryFontValues = new Dictionary<string, FontSettingValue>
            {
                ["Editor"] = new FontSettingValue(this.setting.other.TextFontName, this.setting.other.TextFontSize, this.setting.other.TextFontStyle),
                ["Part counter"] = new FontSettingValue(this.setting.other.PartCounterTextFontName, this.setting.other.PartCounterTextFontSize, this.setting.other.PartCounterTextFontStyle),
                ["Folder"] = new FontSettingValue(this.setting.other.FolderTextFontName, this.setting.other.FolderTextFontSize, this.setting.other.FolderTextFontStyle),
                ["Error list"] = new FontSettingValue(this.setting.other.ErrorListTextFontName, this.setting.other.ErrorListTextFontSize, this.setting.other.ErrorListTextFontStyle),
                ["Log"] = new FontSettingValue(this.setting.other.LogTextFontName, this.setting.other.LogTextFontSize, this.setting.other.LogTextFontStyle)
            };

            defaultCategoryFontValues = new Dictionary<string, FontSettingValue>
            {
                ["Editor"] = new FontSettingValue(defaultSetting.other.TextFontName, defaultSetting.other.TextFontSize, defaultSetting.other.TextFontStyle),
                ["Part counter"] = new FontSettingValue(defaultSetting.other.PartCounterTextFontName, defaultSetting.other.PartCounterTextFontSize, defaultSetting.other.PartCounterTextFontStyle),
                ["Folder"] = new FontSettingValue(defaultSetting.other.FolderTextFontName, defaultSetting.other.FolderTextFontSize, defaultSetting.other.FolderTextFontStyle),
                ["Error list"] = new FontSettingValue(defaultSetting.other.ErrorListTextFontName, defaultSetting.other.ErrorListTextFontSize, defaultSetting.other.ErrorListTextFontStyle),
                ["Log"] = new FontSettingValue(defaultSetting.other.LogTextFontName, defaultSetting.other.LogTextFontSize, defaultSetting.other.LogTextFontStyle)
            };

            categoryPages = new Dictionary<string, Control>();

            Text = "書式設定";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(640, 420);

            lstCategory = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 160,
                IntegralHeight = false
            };
            lstCategory.SelectedIndexChanged += LstCategory_SelectedIndexChanged;

            pnlMainArea = new Panel
            {
                Dock = DockStyle.Fill
            };

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            pnlButtons = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Control
            };
            pnlButtons.Resize += PnlButtons_Resize;

            btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(84, 30)
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "キャンセル",
                DialogResult = DialogResult.Cancel,
                Size = new Size(84, 30)
            };

            pnlButtons.Controls.Add(btnOK);
            pnlButtons.Controls.Add(btnCancel);

            gbFont = new GroupBox
            {
                Text = "フォント",
                Dock = DockStyle.Top,
                Height = 104
            };
            gbFont.Resize += GbFont_Resize;

            lblTargetCategory = new Label
            {
                AutoSize = true,
                Text = "対象: Editor",
                Location = new Point(16, 16)
            };

            lblFontSummary = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                AutoEllipsis = true,
                Location = new Point(16, 35),
                Size = new Size(322, 26)
            };

            btnFontSelect = new Button
            {
                Text = "選択...",
                Size = new Size(74, 26)
            };
            btnFontSelect.Click += BtnFontSelect_Click;

            btnFontReset = new Button
            {
                Text = "リセット",
                Size = new Size(74, 26)
            };
            btnFontReset.Click += BtnFontReset_Click;

            gbFont.Controls.Add(lblTargetCategory);
            gbFont.Controls.Add(lblFontSummary);
            gbFont.Controls.Add(btnFontSelect);
            gbFont.Controls.Add(btnFontReset);

            Panel fontPage = new Panel { Dock = DockStyle.Fill };
            fontPage.Controls.Add(gbFont);

            categoryPages.Add("共通", CreatePlaceholderPage("共通"));
            categoryPages.Add("Editor", fontPage);
            categoryPages.Add("Part counter", fontPage);
            categoryPages.Add("Folder", fontPage);
            categoryPages.Add("Error list", fontPage);
            categoryPages.Add("Log", fontPage);

            foreach (string key in categoryPages.Keys)
            {
                lstCategory.Items.Add(key);
            }

            pnlMainArea.Controls.Add(pnlContent);
            pnlMainArea.Controls.Add(lstCategory);

            TableLayoutPanel rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
            rootLayout.Controls.Add(pnlMainArea, 0, 0);
            rootLayout.Controls.Add(pnlButtons, 0, 1);

            Controls.Add(rootLayout);

            AcceptButton = btnOK;
            CancelButton = btnCancel;

            LayoutBottomButtons();
            LayoutFontControls();
            UpdateFontSummary();
            lstCategory.SelectedIndex = 0;
        }

        private Panel CreatePlaceholderPage(string categoryName)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            Label lbl = new Label
            {
                AutoSize = true,
                Text = $"{categoryName} の設定項目は準備中です。",
                Location = new Point(12, 12)
            };
            panel.Controls.Add(lbl);

            return panel;
        }

        private void PnlButtons_Resize(object sender, EventArgs e)
        {
            LayoutBottomButtons();
        }

        private void LayoutBottomButtons()
        {
            int marginBottom = 11;
            int marginRight = 12;
            int gap = 8;

            btnCancel.Location = new Point(
                Math.Max(0, pnlButtons.ClientSize.Width - marginRight - btnCancel.Width),
                Math.Max(0, pnlButtons.ClientSize.Height - marginBottom - btnCancel.Height));

            btnOK.Location = new Point(
                Math.Max(0, btnCancel.Left - gap - btnOK.Width),
                btnCancel.Top);
        }

        private void GbFont_Resize(object sender, EventArgs e)
        {
            LayoutFontControls();
        }

        private void LayoutFontControls()
        {
            int rightX = Math.Max(16, gbFont.ClientSize.Width - btnFontSelect.Width - 10);
            btnFontSelect.Location = new Point(rightX, 20);
            btnFontReset.Location = new Point(rightX, 50);

            lblTargetCategory.Location = new Point(16, 22);
            lblFontSummary.Location = new Point(16, 50);
            lblFontSummary.Size = new Size(Math.Max(120, rightX - 24), 26);
        }

        private void LstCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCategory.SelectedItem == null) return;

            string key = lstCategory.SelectedItem.ToString();
            if (fontCategories.Contains(key))
            {
                activeFontCategory = key;
                UpdateFontSummary();
            }

            if (!categoryPages.TryGetValue(key, out Control page)) return;

            pnlContent.SuspendLayout();
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(page);
            pnlContent.ResumeLayout();
        }

        private Font CreateFont(FontSettingValue v)
        {
            try
            {
                return new Font(v.Name, v.Size, v.Style);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily, 9.0f, FontStyle.Regular);
            }
        }

        private void UpdateFontSummary()
        {
            if (!categoryFontValues.TryGetValue(activeFontCategory, out FontSettingValue v)) return;

            lblTargetCategory.Text = $"対象: {activeFontCategory}";
            lblFontSummary.Text = $"{v.Name}, {v.Size}, {v.Style}";

            previewFont?.Dispose();
            previewFont = CreateFont(v);
            lblFontSummary.Font = previewFont;
        }

        private void BtnFontSelect_Click(object sender, EventArgs e)
        {
            if (!categoryFontValues.TryGetValue(activeFontCategory, out FontSettingValue v)) return;

            using (FontDialog fd = new FontDialog())
            {
                using (Font baseFont = CreateFont(v))
                {
                    fd.Font = baseFont;
                    if (fd.ShowDialog(this) != DialogResult.OK) return;
                }

                v.Name = fd.Font.Name;
                v.Size = fd.Font.Size;
                v.Style = fd.Font.Style;
                UpdateFontSummary();
            }
        }

        private void BtnFontReset_Click(object sender, EventArgs e)
        {
            if (!defaultCategoryFontValues.TryGetValue(activeFontCategory, out FontSettingValue dv)) return;
            categoryFontValues[activeFontCategory] = dv.Copy();
            UpdateFontSummary();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FontSettingValue editor = categoryFontValues["Editor"];
            setting.other.TextFontName = editor.Name;
            setting.other.TextFontSize = editor.Size;
            setting.other.TextFontStyle = editor.Style;

            FontSettingValue partCounter = categoryFontValues["Part counter"];
            setting.other.PartCounterTextFontName = partCounter.Name;
            setting.other.PartCounterTextFontSize = partCounter.Size;
            setting.other.PartCounterTextFontStyle = partCounter.Style;

            FontSettingValue folder = categoryFontValues["Folder"];
            setting.other.FolderTextFontName = folder.Name;
            setting.other.FolderTextFontSize = folder.Size;
            setting.other.FolderTextFontStyle = folder.Style;

            FontSettingValue errorList = categoryFontValues["Error list"];
            setting.other.ErrorListTextFontName = errorList.Name;
            setting.other.ErrorListTextFontSize = errorList.Size;
            setting.other.ErrorListTextFontStyle = errorList.Style;

            FontSettingValue log = categoryFontValues["Log"];
            setting.other.LogTextFontName = log.Name;
            setting.other.LogTextFontSize = log.Size;
            setting.other.LogTextFontStyle = log.Style;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            previewFont?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
