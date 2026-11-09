using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace mml2vgmIDEx64
{
    public class FrmFormatSetting : Form
    {
        private readonly ListBox lstCategory;
        private readonly Panel pnlMainArea;
        private readonly Panel pnlContent;
        private readonly Panel pnlButtons;
        private readonly Button btnOK;
        private readonly Button btnCancel;
        private readonly Dictionary<string, Control> categoryPages;
        private readonly GroupBox gbEditorFont;
        private readonly Label lblFontSummary;
        private readonly Button btnFontSelect;
        private readonly Button btnFontReset;
        private readonly string defaultFontName;
        private readonly float defaultFontSize;
        private readonly FontStyle defaultFontStyle;
        private Font selectedFont;

        public Setting setting { get; private set; }

        public FrmFormatSetting(Setting setting)
        {
            this.setting = setting.Copy();
            Setting defaultSetting = new Setting();
            defaultFontName = defaultSetting.other.TextFontName;
            defaultFontSize = defaultSetting.other.TextFontSize;
            defaultFontStyle = defaultSetting.other.TextFontStyle;
            try
            {
                selectedFont = new Font(this.setting.other.TextFontName, this.setting.other.TextFontSize, this.setting.other.TextFontStyle);
            }
            catch
            {
                selectedFont = new Font(SystemFonts.DefaultFont.FontFamily, 9.0f, FontStyle.Regular);
            }
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

            gbEditorFont = new GroupBox
            {
                Text = "フォント",
                Dock = DockStyle.Top,
                Height = 96
            };
            gbEditorFont.Resize += GbEditorFont_Resize;

            lblFontSummary = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                AutoEllipsis = true,
                Location = new Point(16, 29),
                Size = new Size(322, 26)
            };

            btnFontSelect = new Button
            {
                Text = "選択...",
                Size = new Size(74, 26)
            };
            btnFontSelect.Click += BtnFont_Click;

            btnFontReset = new Button
            {
                Text = "リセット",
                Size = new Size(74, 26)
            };
            btnFontReset.Click += BtnFontReset_Click;

            gbEditorFont.Controls.Add(lblFontSummary);
            gbEditorFont.Controls.Add(btnFontSelect);
            gbEditorFont.Controls.Add(btnFontReset);

            Panel editorPage = CreateCategoryPage("Editor");
            editorPage.Controls.Add(gbEditorFont);

            categoryPages.Add("共通", CreateCategoryPage("共通"));
            categoryPages.Add("Editor", editorPage);
            categoryPages.Add("Part counter", CreateCategoryPage("Part counter"));
            categoryPages.Add("Folder", CreateCategoryPage("Folder"));
            categoryPages.Add("Error list", CreateCategoryPage("Error list"));
            categoryPages.Add("Log", CreateCategoryPage("Log"));

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
            LayoutEditorFontControls();
            UpdateEditorFontLabels();
            lstCategory.SelectedIndex = 0;
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

        private void GbEditorFont_Resize(object sender, EventArgs e)
        {
            LayoutEditorFontControls();
        }

        private void LayoutEditorFontControls()
        {
            int rightX = Math.Max(16, gbEditorFont.ClientSize.Width - btnFontSelect.Width - 10);
            btnFontSelect.Location = new Point(rightX, 20);
            btnFontReset.Location = new Point(rightX, 50);

            lblFontSummary.Location = new Point(16, 35);
            lblFontSummary.Size = new Size(Math.Max(120, rightX - 24), 26);
        }

        private Panel CreateCategoryPage(string categoryName)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            if (categoryName != "Editor")
            {
                Label lbl = new Label
                {
                    AutoSize = true,
                    Text = $"{categoryName} の設定項目は準備中です。",
                    Location = new Point(12, 12)
                };
                panel.Controls.Add(lbl);
            }

            return panel;
        }

        private void LstCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCategory.SelectedItem == null) return;

            string key = lstCategory.SelectedItem.ToString();
            if (!categoryPages.TryGetValue(key, out Control page)) return;

            pnlContent.SuspendLayout();
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(page);
            pnlContent.ResumeLayout();
        }

        private void BtnFont_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = selectedFont;
                if (fd.ShowDialog(this) != DialogResult.OK) return;

                selectedFont.Dispose();
                selectedFont = (Font)fd.Font.Clone();
                UpdateEditorFontLabels();
            }
        }

        private void UpdateEditorFontLabels()
        {
            lblFontSummary.Text = $"{selectedFont.Name}, {selectedFont.Size}, {selectedFont.Style}";
            lblFontSummary.Font = selectedFont;
        }

        private void BtnFontReset_Click(object sender, EventArgs e)
        {
            selectedFont.Dispose();
            selectedFont = new Font(defaultFontName, defaultFontSize, defaultFontStyle);
            UpdateEditorFontLabels();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            setting.other.TextFontName = selectedFont.Name;
            setting.other.TextFontSize = selectedFont.Size;
            setting.other.TextFontStyle = selectedFont.Style;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            selectedFont.Dispose();
            base.OnFormClosed(e);
        }
    }
}
