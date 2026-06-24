using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SimpleTextEditor
{
    public partial class Form1 : Form
    {
        private string currentFilePath = "";
        private bool isModified = false;
        private TextBox textBox;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private ContextMenuStrip contextMenuStrip;

        public Form1()
        {
            InitializeComponent();
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            this.Text = "Текстовый редактор";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Главное текстовое поле
            textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                WordWrap = true,
                ContextMenuStrip = CreateContextMenu()
            };
            textBox.TextChanged += TextBox_TextChanged;
            this.Controls.Add(textBox);

            // Меню
            menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;

            // Файл
            var fileMenu = new ToolStripMenuItem("Файл");
            var newMenuItem = new ToolStripMenuItem("Новый", null, NewFile_Click, Keys.Control | Keys.N);
            var openMenuItem = new ToolStripMenuItem("Открыть", null, OpenFile_Click, Keys.Control | Keys.O);
            var saveMenuItem = new ToolStripMenuItem("Сохранить", null, SaveFile_Click, Keys.Control | Keys.S);
            var saveAsMenuItem = new ToolStripMenuItem("Сохранить как", null, SaveAsFile_Click);
            var exitMenuItem = new ToolStripMenuItem("Выход", null, Exit_Click);
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                newMenuItem, openMenuItem, saveMenuItem, saveAsMenuItem,
                new ToolStripSeparator(), exitMenuItem
            });

            // Правка
            var editMenu = new ToolStripMenuItem("Правка");
            var undoMenuItem = new ToolStripMenuItem("Отменить", null, Undo_Click, Keys.Control | Keys.Z);
            var cutMenuItem = new ToolStripMenuItem("Вырезать", null, Cut_Click, Keys.Control | Keys.X);
            var copyMenuItem = new ToolStripMenuItem("Копировать", null, Copy_Click, Keys.Control | Keys.C);
            var pasteMenuItem = new ToolStripMenuItem("Вставить", null, Paste_Click, Keys.Control | Keys.V);
            var selectAllMenuItem = new ToolStripMenuItem("Выделить всё", null, SelectAll_Click, Keys.Control | Keys.A);
            editMenu.DropDownItems.AddRange(new ToolStripItem[] {
                undoMenuItem, new ToolStripSeparator(),
                cutMenuItem, copyMenuItem, pasteMenuItem,
                new ToolStripSeparator(), selectAllMenuItem
            });

            // Настройки
            var settingsMenu = new ToolStripMenuItem("Настройки");
            var fontMenuItem = new ToolStripMenuItem("Шрифт", null, Font_Click);
            var fontColorMenuItem = new ToolStripMenuItem("Цвет шрифта", null, FontColor_Click);
            var backColorMenuItem = new ToolStripMenuItem("Цвет фона", null, BackColor_Click);
            settingsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                fontMenuItem, fontColorMenuItem, backColorMenuItem
            });

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, settingsMenu });
            this.Controls.Add(menuStrip);

            // Панель инструментов
            toolStrip = new ToolStrip();
            var newButton = new ToolStripButton("Новый", null, NewFile_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var openButton = new ToolStripButton("Открыть", null, OpenFile_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var saveButton = new ToolStripButton("Сохранить", null, SaveFile_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            toolStrip.Items.Add(newButton);
            toolStrip.Items.Add(openButton);
            toolStrip.Items.Add(saveButton);
            toolStrip.Items.Add(new ToolStripSeparator());

            var undoButton = new ToolStripButton("Отменить", null, Undo_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var cutButton = new ToolStripButton("Вырезать", null, Cut_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var copyButton = new ToolStripButton("Копировать", null, Copy_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var pasteButton = new ToolStripButton("Вставить", null, Paste_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            toolStrip.Items.Add(undoButton);
            toolStrip.Items.Add(cutButton);
            toolStrip.Items.Add(copyButton);
            toolStrip.Items.Add(pasteButton);
            toolStrip.Items.Add(new ToolStripSeparator());

            var settingsButton = new ToolStripButton("Настройки", null, (s, e) => { }) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            toolStrip.Items.Add(settingsButton);

            this.Controls.Add(toolStrip);

            // Расположение
            menuStrip.Dock = DockStyle.Top;
            toolStrip.Dock = DockStyle.Top;
            textBox.Dock = DockStyle.Fill;

            UpdateTitle();
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add(new ToolStripMenuItem("Отменить", null, Undo_Click));
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(new ToolStripMenuItem("Вырезать", null, Cut_Click));
            cms.Items.Add(new ToolStripMenuItem("Копировать", null, Copy_Click));
            cms.Items.Add(new ToolStripMenuItem("Вставить", null, Paste_Click));
            return cms;
        }

        private void UpdateTitle()
        {
            string title = "Текстовый редактор";
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                title = currentFilePath;
                if (isModified) title += "*";
            }
            this.Text = title;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateTitle();
        }

        // ---- Обработчики меню и кнопок ----

        private void NewFile_Click(object sender, EventArgs e)
        {
            if (isModified && !ConfirmSave()) return;
            textBox.Clear();
            currentFilePath = "";
            isModified = false;
            UpdateTitle();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (isModified && !ConfirmSave()) return;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        textBox.Text = File.ReadAllText(ofd.FileName);
                        currentFilePath = ofd.FileName;
                        isModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка открытия: " + ex.Message);
                    }
                }
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
                SaveAsFile_Click(sender, e);
            else
            {
                try
                {
                    File.WriteAllText(currentFilePath, textBox.Text);
                    isModified = false;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения: " + ex.Message);
                }
            }
        }

        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, textBox.Text);
                        currentFilePath = sfd.FileName;
                        isModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка сохранения: " + ex.Message);
                    }
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            if (isModified && !ConfirmSave()) return;
            this.Close();
        }

        private bool ConfirmSave()
        {
            var result = MessageBox.Show("Текст изменён. Сохранить?", "Предупреждение",
                                          MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes)
            {
                SaveFile_Click(null, null);
                return true;
            }
            else if (result == DialogResult.No)
                return true;
            else
                return false;
        }

        // ---- Команды правки ----
        private void Undo_Click(object sender, EventArgs e)
        {
            if (textBox.CanUndo) textBox.Undo();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText.Length > 0)
                Clipboard.SetText(textBox.SelectedText);
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText.Length > 0)
            {
                Clipboard.SetText(textBox.SelectedText);
                textBox.SelectedText = "";
            }
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
                textBox.Paste();
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        // ---- Настройки ----
        private void Font_Click(object sender, EventArgs e)
        {
            using (var fd = new FontDialog())
            {
                fd.Font = textBox.Font;
                if (fd.ShowDialog() == DialogResult.OK)
                    textBox.Font = fd.Font;
            }
        }

        private void FontColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = textBox.ForeColor;
                if (cd.ShowDialog() == DialogResult.OK)
                    textBox.ForeColor = cd.Color;
            }
        }

        private void BackColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = textBox.BackColor;
                if (cd.ShowDialog() == DialogResult.OK)
                    textBox.BackColor = cd.Color;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isModified && !ConfirmSave())
                e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}