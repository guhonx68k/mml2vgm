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
        private readonly HashSet<string> textColorCategories;
        private readonly Dictionary<string, int> categoryTextColorValues;
        private readonly Dictionary<string, int> defaultCategoryTextColorValues;
        private readonly GroupBox gbFont;
        private readonly ComboBox cmbScrollBarTheme;
        private readonly Label lblFontSummary;
        private readonly Button btnFontSelect;
        private readonly Button btnFontReset;
        private readonly Button btnTextColorSelect;
        private readonly Button btnTextColorReset;
        private readonly Button btnCommonBackColorSelect;
        private readonly Button btnCommonBackColorReset;
        private Font previewFont;
        private string activeFontCategory = "Editor";
        private int selectedScrollBarTheme;
        private int selectedCommonPanelBackColor;
        private readonly int defaultCommonPanelBackColor;

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
                ["Editor"] = new FontSettingValue(this.setting.other.PanelTextFontSettings.Editor.Name, this.setting.other.PanelTextFontSettings.Editor.Size, this.setting.other.PanelTextFontSettings.Editor.Style),
                ["Part counter"] = new FontSettingValue(this.setting.other.PanelTextFontSettings.PartCounter.Name, this.setting.other.PanelTextFontSettings.PartCounter.Size, this.setting.other.PanelTextFontSettings.PartCounter.Style),
                ["Folder"] = new FontSettingValue(this.setting.other.PanelTextFontSettings.Folder.Name, this.setting.other.PanelTextFontSettings.Folder.Size, this.setting.other.PanelTextFontSettings.Folder.Style),
                ["Error list"] = new FontSettingValue(this.setting.other.PanelTextFontSettings.ErrorList.Name, this.setting.other.PanelTextFontSettings.ErrorList.Size, this.setting.other.PanelTextFontSettings.ErrorList.Style),
                ["Log"] = new FontSettingValue(this.setting.other.PanelTextFontSettings.Log.Name, this.setting.other.PanelTextFontSettings.Log.Size, this.setting.other.PanelTextFontSettings.Log.Style)
            };

            defaultCategoryFontValues = new Dictionary<string, FontSettingValue>
            {
                ["Editor"] = new FontSettingValue(defaultSetting.other.TextFontName, defaultSetting.other.TextFontSize, defaultSetting.other.TextFontStyle),
                ["Part counter"] = new FontSettingValue(defaultSetting.other.PartCounterTextFontName, defaultSetting.other.PartCounterTextFontSize, defaultSetting.other.PartCounterTextFontStyle),
                ["Folder"] = new FontSettingValue(defaultSetting.other.FolderTextFontName, defaultSetting.other.FolderTextFontSize, defaultSetting.other.FolderTextFontStyle),
                ["Error list"] = new FontSettingValue(defaultSetting.other.ErrorListTextFontName, defaultSetting.other.ErrorListTextFontSize, defaultSetting.other.ErrorListTextFontStyle),
                ["Log"] = new FontSettingValue(defaultSetting.other.LogTextFontName, defaultSetting.other.LogTextFontSize, defaultSetting.other.LogTextFontStyle)
            };

            textColorCategories = new HashSet<string>
            {
                "Part counter",
                "Folder",
                "Error list",
                "Log"
            };

            categoryTextColorValues = new Dictionary<string, int>
            {
                ["Part counter"] = ResolveCategoryTextColorArgb(this.setting, "Part counter"),
                ["Folder"] = ResolveCategoryTextColorArgb(this.setting, "Folder"),
                ["Error list"] = ResolveCategoryTextColorArgb(this.setting, "Error list"),
                ["Log"] = ResolveCategoryTextColorArgb(this.setting, "Log")
            };

            defaultCategoryTextColorValues = new Dictionary<string, int>
            {
                ["Part counter"] = ResolveCategoryTextColorArgb(defaultSetting, "Part counter"),
                ["Folder"] = ResolveCategoryTextColorArgb(defaultSetting, "Folder"),
                ["Error list"] = ResolveCategoryTextColorArgb(defaultSetting, "Error list"),
                ["Log"] = ResolveCategoryTextColorArgb(defaultSetting, "Log")
            };
            selectedScrollBarTheme = this.setting.other.ScrollBarTheme;
            selectedCommonPanelBackColor = this.setting.other.CommonPanelBackColor;
            defaultCommonPanelBackColor = defaultSetting.other.CommonPanelBackColor;

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
                Text = "テキスト",
                Dock = DockStyle.Top,
                Height = 140
            };
            gbFont.Resize += GbFont_Resize;

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

            btnTextColorSelect = new Button
            {
                Text = "色...",
                Size = new Size(74, 26),
                UseVisualStyleBackColor = false
            };
            btnTextColorSelect.Click += BtnTextColorSelect_Click;

            btnTextColorReset = new Button
            {
                Text = "リセット",
                Size = new Size(74, 26)
            };
            btnTextColorReset.Click += BtnTextColorReset_Click;

            gbFont.Controls.Add(lblFontSummary);
            gbFont.Controls.Add(btnFontSelect);
            gbFont.Controls.Add(btnFontReset);
            gbFont.Controls.Add(btnTextColorSelect);
            gbFont.Controls.Add(btnTextColorReset);

            Panel fontPage = new Panel { Dock = DockStyle.Fill };
            fontPage.Controls.Add(gbFont);

            GroupBox gbCommon = new GroupBox
            {
                Text = "スクロールバー",
                Dock = DockStyle.Top,
                Height = 84
            };
            Label lblScrollBarTheme = new Label
            {
                AutoSize = true,
                Text = "カラー",
                Location = new Point(16, 36)
            };
            cmbScrollBarTheme = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(72, 32),
                Width = 140
            };
            cmbScrollBarTheme.Items.Add("ライトモード");
            cmbScrollBarTheme.Items.Add("ダークモード");
            cmbScrollBarTheme.SelectedIndex = selectedScrollBarTheme == 0 ? 0 : 1;
            cmbScrollBarTheme.SelectedIndexChanged += CmbScrollBarTheme_SelectedIndexChanged;
            gbCommon.Controls.Add(lblScrollBarTheme);
            gbCommon.Controls.Add(cmbScrollBarTheme);

            GroupBox gbCommonBackColor = new GroupBox
            {
                Text = "背景色",
                Dock = DockStyle.Top,
                Height = 84
            };
            Label lblCommonBackColor = new Label
            {
                AutoSize = true,
                Text = "カラー",
                Location = new Point(16, 36)
            };
            btnCommonBackColorSelect = new Button
            {
                Text = "色...",
                Location = new Point(72, 32),
                Size = new Size(120, 26),
                UseVisualStyleBackColor = false
            };
            btnCommonBackColorSelect.Click += BtnCommonBackColorSelect_Click;
            btnCommonBackColorReset = new Button
            {
                Text = "リセット",
                Location = new Point(200, 32),
                Size = new Size(74, 26)
            };
            btnCommonBackColorReset.Click += BtnCommonBackColorReset_Click;
            gbCommonBackColor.Controls.Add(lblCommonBackColor);
            gbCommonBackColor.Controls.Add(btnCommonBackColorSelect);
            gbCommonBackColor.Controls.Add(btnCommonBackColorReset);

            Panel commonPage = new Panel { Dock = DockStyle.Fill };
            commonPage.Controls.Add(gbCommonBackColor);
            commonPage.Controls.Add(gbCommon);

            categoryPages.Add("共通", commonPage);
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
            UpdateCommonPanelBackgroundButton();
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
            btnTextColorSelect.Location = new Point(rightX, 80);
            btnTextColorReset.Location = new Point(Math.Max(16, rightX - btnTextColorReset.Width - 8), 80);

            lblFontSummary.Location = new Point(16, 24);
            lblFontSummary.Size = new Size(Math.Max(120, rightX - 24), 56);
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

        private void CmbScrollBarTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedScrollBarTheme = cmbScrollBarTheme.SelectedIndex == 0 ? 0 : 1;
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

            lblFontSummary.Text = $"サンプル 123 ABC あいう\r\n{v.Name}, {v.Size}, {v.Style}";

            previewFont?.Dispose();
            previewFont = CreateFont(v);
            lblFontSummary.Font = previewFont;

            Color backgroundColor = GetCategoryBackgroundColor(activeFontCategory);
            Color textColor = GetCategoryTextColor(activeFontCategory);
            lblFontSummary.BackColor = backgroundColor;
            lblFontSummary.ForeColor = textColor;

            bool canEditTextColor = textColorCategories.Contains(activeFontCategory);
            btnTextColorSelect.Enabled = canEditTextColor;
            btnTextColorReset.Enabled = canEditTextColor;
            UpdateColorSampleButton(textColor, canEditTextColor);
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

        private void BtnTextColorSelect_Click(object sender, EventArgs e)
        {
            if (!textColorCategories.Contains(activeFontCategory)) return;
            if (!categoryTextColorValues.TryGetValue(activeFontCategory, out int colorArgb)) return;

            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = Color.FromArgb(colorArgb);
                if (cd.ShowDialog(this) != DialogResult.OK) return;
                categoryTextColorValues[activeFontCategory] = cd.Color.ToArgb();
                UpdateFontSummary();
            }
        }

        private void BtnTextColorReset_Click(object sender, EventArgs e)
        {
            if (!textColorCategories.Contains(activeFontCategory)) return;
            if (!defaultCategoryTextColorValues.TryGetValue(activeFontCategory, out int colorArgb)) return;
            categoryTextColorValues[activeFontCategory] = colorArgb;
            UpdateFontSummary();
        }

        private void BtnCommonBackColorSelect_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = Color.FromArgb(GetCommonPanelBackColorArgb(setting.ColorScheme.PartCounter_BackColor));
                if (cd.ShowDialog(this) != DialogResult.OK) return;
                selectedCommonPanelBackColor = cd.Color.ToArgb();
            }

            UpdateCommonPanelBackgroundButton();
            if (fontCategories.Contains(activeFontCategory))
            {
                UpdateFontSummary();
            }
        }

        private void BtnCommonBackColorReset_Click(object sender, EventArgs e)
        {
            selectedCommonPanelBackColor = defaultCommonPanelBackColor;
            UpdateCommonPanelBackgroundButton();
            if (fontCategories.Contains(activeFontCategory))
            {
                UpdateFontSummary();
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FontSettingValue editor = categoryFontValues["Editor"];
            setting.other.PanelTextFontSettings.Editor.Name = editor.Name;
            setting.other.PanelTextFontSettings.Editor.Size = editor.Size;
            setting.other.PanelTextFontSettings.Editor.Style = editor.Style;

            FontSettingValue partCounter = categoryFontValues["Part counter"];
            setting.other.PanelTextFontSettings.PartCounter.Name = partCounter.Name;
            setting.other.PanelTextFontSettings.PartCounter.Size = partCounter.Size;
            setting.other.PanelTextFontSettings.PartCounter.Style = partCounter.Style;

            FontSettingValue folder = categoryFontValues["Folder"];
            setting.other.PanelTextFontSettings.Folder.Name = folder.Name;
            setting.other.PanelTextFontSettings.Folder.Size = folder.Size;
            setting.other.PanelTextFontSettings.Folder.Style = folder.Style;

            FontSettingValue errorList = categoryFontValues["Error list"];
            setting.other.PanelTextFontSettings.ErrorList.Name = errorList.Name;
            setting.other.PanelTextFontSettings.ErrorList.Size = errorList.Size;
            setting.other.PanelTextFontSettings.ErrorList.Style = errorList.Style;

            FontSettingValue log = categoryFontValues["Log"];
            setting.other.PanelTextFontSettings.Log.Name = log.Name;
            setting.other.PanelTextFontSettings.Log.Size = log.Size;
            setting.other.PanelTextFontSettings.Log.Style = log.Style;
            setting.other.PanelTextFontSettings.PartCounter.Color = categoryTextColorValues["Part counter"];
            setting.other.PanelTextFontSettings.Folder.Color = categoryTextColorValues["Folder"];
            setting.other.PanelTextFontSettings.ErrorList.Color = categoryTextColorValues["Error list"];
            setting.other.PanelTextFontSettings.Log.Color = categoryTextColorValues["Log"];
            setting.other.CommonPanelBackColor = selectedCommonPanelBackColor;
            setting.other.ScrollBarTheme = selectedScrollBarTheme;
        }

        private static bool IsUnsetColor(int colorArgb)
        {
            return colorArgb == Color.Empty.ToArgb() || colorArgb == 0;
        }

        private static int ResolveCategoryTextColorArgb(Setting targetSetting, string category)
        {
            switch (category)
            {
                case "Part counter":
                    return IsUnsetColor(targetSetting.other.PanelTextFontSettings.PartCounter.Color)
                        ? targetSetting.ColorScheme.PartCounter_ForeColor
                        : targetSetting.other.PanelTextFontSettings.PartCounter.Color;
                case "Folder":
                    return IsUnsetColor(targetSetting.other.PanelTextFontSettings.Folder.Color)
                        ? targetSetting.ColorScheme.FolderTree_ForeColor
                        : targetSetting.other.PanelTextFontSettings.Folder.Color;
                case "Error list":
                    return IsUnsetColor(targetSetting.other.PanelTextFontSettings.ErrorList.Color)
                        ? targetSetting.ColorScheme.ErrorList_ForeColor
                        : targetSetting.other.PanelTextFontSettings.ErrorList.Color;
                case "Log":
                    return IsUnsetColor(targetSetting.other.PanelTextFontSettings.Log.Color)
                        ? targetSetting.ColorScheme.Log_ForeColor
                        : targetSetting.other.PanelTextFontSettings.Log.Color;
                case "Editor":
                    return targetSetting.ColorScheme.Azuki_ForeColor;
                default:
                    return SystemColors.ControlText.ToArgb();
            }
        }

        private Color GetCategoryTextColor(string category)
        {
            if (categoryTextColorValues.TryGetValue(category, out int colorArgb))
            {
                return Color.FromArgb(colorArgb);
            }

            return Color.FromArgb(ResolveCategoryTextColorArgb(setting, category));
        }

        private Color GetCategoryBackgroundColor(string category)
        {
            switch (category)
            {
                case "Editor":
                    return Color.FromArgb(setting.ColorScheme.Azuki_BackColor);
                case "Part counter":
                    return Color.FromArgb(GetCommonPanelBackColorArgb(setting.ColorScheme.PartCounter_BackColor));
                case "Folder":
                    return Color.FromArgb(GetCommonPanelBackColorArgb(setting.ColorScheme.FolderTree_BackColor));
                case "Error list":
                    return Color.FromArgb(GetCommonPanelBackColorArgb(setting.ColorScheme.ErrorList_BackColor));
                case "Log":
                    return Color.FromArgb(GetCommonPanelBackColorArgb(setting.ColorScheme.Log_BackColor));
                default:
                    return SystemColors.Window;
            }
        }

        private int GetCommonPanelBackColorArgb(int fallbackColor)
        {
            return IsUnsetColor(selectedCommonPanelBackColor)
                ? fallbackColor
                : selectedCommonPanelBackColor;
        }

        private void UpdateCommonPanelBackgroundButton()
        {
            if (IsUnsetColor(selectedCommonPanelBackColor))
            {
                btnCommonBackColorSelect.Text = "未設定";
                btnCommonBackColorSelect.BackColor = SystemColors.Control;
                btnCommonBackColorSelect.ForeColor = SystemColors.ControlText;
                return;
            }

            Color color = Color.FromArgb(selectedCommonPanelBackColor);
            btnCommonBackColorSelect.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            btnCommonBackColorSelect.BackColor = color;
            btnCommonBackColorSelect.ForeColor = color.GetBrightness() > 0.6f ? Color.Black : Color.White;
        }

        private void UpdateColorSampleButton(Color color, bool enabled)
        {
            if (!enabled)
            {
                btnTextColorSelect.Text = "色...";
                btnTextColorSelect.BackColor = SystemColors.Control;
                btnTextColorSelect.ForeColor = SystemColors.ControlText;
                return;
            }

            btnTextColorSelect.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            btnTextColorSelect.BackColor = color;
            btnTextColorSelect.ForeColor = color.GetBrightness() > 0.6f ? Color.Black : Color.White;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            previewFont?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
