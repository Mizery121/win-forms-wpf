using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SimpleFileExplorer
{
    public partial class Form1 : Form
    {
        private TreeView treeView;
        private ListView listView;
        private TextBox addressBox;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private ContextMenuStrip contextMenuStrip;

        private Stack<string> backStack = new Stack<string>();
        private Stack<string> forwardStack = new Stack<string>();
        private string currentPath = "";
        private bool suppressSelection = false;

        public Form1()
        {
            InitializeComponent();
            InitializeExplorer();
        }

        private void InitializeExplorer()
        {
            this.Text = "Проводник";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Меню
            menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => this.Close()));

            var viewMenu = new ToolStripMenuItem("Вид");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem("Обновить", null, Refresh_Click, Keys.F5));

            var navMenu = new ToolStripMenuItem("Навигация");
            navMenu.DropDownItems.Add(new ToolStripMenuItem("Назад", null, Back_Click, Keys.Alt | Keys.Left));
            navMenu.DropDownItems.Add(new ToolStripMenuItem("Вперед", null, Forward_Click, Keys.Alt | Keys.Right));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(navMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Панель инструментов
            toolStrip = new ToolStrip();
            var backBtn = new ToolStripButton("Назад", null, Back_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var forwardBtn = new ToolStripButton("Вперед", null, Forward_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var upBtn = new ToolStripButton("Вверх", null, Up_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            var refreshBtn = new ToolStripButton("Обновить", null, Refresh_Click) { DisplayStyle = ToolStripItemDisplayStyle.Text };
            toolStrip.Items.Add(backBtn);
            toolStrip.Items.Add(forwardBtn);
            toolStrip.Items.Add(upBtn);
            toolStrip.Items.Add(refreshBtn);
            toolStrip.Items.Add(new ToolStripSeparator());

            addressBox = new TextBox();
            addressBox.Width = 300;
            addressBox.KeyDown += AddressBox_KeyDown;
            var addressLabel = new ToolStripLabel("Адрес:");
            var addressControl = new ToolStripControlHost(addressBox);
            toolStrip.Items.Add(addressLabel);
            toolStrip.Items.Add(addressControl);

            this.Controls.Add(toolStrip);

            // Разделитель (TreeView + ListView)
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Panel1MinSize = 200
            };

            treeView = new TreeView { Dock = DockStyle.Fill };
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.BeforeExpand += TreeView_BeforeExpand;
            split.Panel1.Controls.Add(treeView);

            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            listView.Columns.Add("Имя", 200);
            listView.Columns.Add("Тип", 100);
            listView.Columns.Add("Размер", 100);
            listView.DoubleClick += ListView_DoubleClick;
            listView.ContextMenuStrip = CreateContextMenu();
            split.Panel2.Controls.Add(listView);

            this.Controls.Add(split);

            // Порядок
            menuStrip.Dock = DockStyle.Top;
            toolStrip.Dock = DockStyle.Top;
            split.Dock = DockStyle.Fill;

            // Загружаем диски
            LoadDrives();
            if (treeView.Nodes.Count > 0)
            {
                treeView.SelectedNode = treeView.Nodes[0];
                treeView.Nodes[0].Expand();
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add(new ToolStripMenuItem("Открыть", null, (s, e) => OpenSelectedItem()));
            cms.Items.Add(new ToolStripMenuItem("Свойства", null, (s, e) => ShowProperties()));
            return cms;
        }

        private void LoadDrives()
        {
            treeView.Nodes.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var node = new TreeNode(drive.Name) { Tag = drive.Name };
                    node.Nodes.Add("Loading..."); // фиктивный узел
                    treeView.Nodes.Add(node);
                }
            }
        }

        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {
                e.Node.Nodes.Clear();
                string path = e.Node.Tag.ToString();
                try
                {
                    var dir = new DirectoryInfo(path);
                    foreach (var sub in dir.GetDirectories())
                    {
                        var subNode = new TreeNode(sub.Name) { Tag = sub.FullName };
                        subNode.Nodes.Add("Loading...");
                        e.Node.Nodes.Add(subNode);
                    }
                }
                catch { /* ignore */ }
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (suppressSelection) return;
            if (e.Node.Tag != null)
                NavigateTo(e.Node.Tag.ToString());
        }

        private void NavigateTo(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path)) return;

            if (!string.IsNullOrEmpty(currentPath) && currentPath != path)
            {
                backStack.Push(currentPath);
                forwardStack.Clear();
            }
            currentPath = path;
            LoadDirectory(path);
            UpdateAddress();
            // Выделяем узел в дереве (без вызова события)
            suppressSelection = true;
            SelectTreeNode(path);
            suppressSelection = false;
        }

        private void SelectTreeNode(string path)
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                if (node.Tag != null && node.Tag.ToString() == path)
                {
                    treeView.SelectedNode = node;
                    return;
                }
                var found = FindNodeRecursive(node, path);
                if (found != null)
                {
                    treeView.SelectedNode = found;
                    return;
                }
            }
        }

        private TreeNode FindNodeRecursive(TreeNode parent, string path)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                if (child.Tag != null && child.Tag.ToString() == path)
                    return child;
                var found = FindNodeRecursive(child, path);
                if (found != null) return found;
            }
            return null;
        }

        private void LoadDirectory(string path)
        {
            listView.Items.Clear();
            if (!Directory.Exists(path)) return;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var d in dir.GetDirectories())
                {
                    var item = new ListViewItem(d.Name) { Tag = d.FullName };
                    item.SubItems.Add("Папка");
                    item.SubItems.Add("");
                    listView.Items.Add(item);
                }
                foreach (var f in dir.GetFiles())
                {
                    var item = new ListViewItem(f.Name) { Tag = f.FullName };
                    item.SubItems.Add("Файл");
                    item.SubItems.Add(f.Length.ToString());
                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void UpdateAddress()
        {
            addressBox.Text = currentPath;
        }

        // ---- Навигация ----
        private void Back_Click(object sender, EventArgs e)
        {
            if (backStack.Count > 0)
            {
                forwardStack.Push(currentPath);
                NavigateTo(backStack.Pop());
            }
        }

        private void Forward_Click(object sender, EventArgs e)
        {
            if (forwardStack.Count > 0)
            {
                backStack.Push(currentPath);
                NavigateTo(forwardStack.Pop());
            }
        }

        private void Up_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                string parent = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(parent))
                    NavigateTo(parent);
                else
                {
                    // Корень диска – переходим к списку дисков
                    LoadDrives();
                    currentPath = "";
                    addressBox.Text = "";
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
                LoadDirectory(currentPath);
            else
                LoadDrives();
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = addressBox.Text.Trim();
                if (Directory.Exists(path))
                    NavigateTo(path);
                else
                    MessageBox.Show("Папка не найдена.");
            }
        }

        // ---- Работа со списком ----
        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedItem();
        }

        private void OpenSelectedItem()
        {
            if (listView.SelectedItems.Count == 0) return;
            var item = listView.SelectedItems[0];
            string fullPath = item.Tag.ToString();
            if (Directory.Exists(fullPath))
                NavigateTo(fullPath);
            else if (File.Exists(fullPath))
            {
                try { Process.Start(fullPath); }
                catch (Exception ex) { MessageBox.Show("Не удалось открыть: " + ex.Message); }
            }
        }

        private void ShowProperties()
        {
            if (listView.SelectedItems.Count == 0) return;
            var item = listView.SelectedItems[0];
            string path = item.Tag.ToString();
            string info = $"Путь: {path}\n";
            if (File.Exists(path))
            {
                var fi = new FileInfo(path);
                info += $"Размер: {fi.Length} байт\nСоздан: {fi.CreationTime}\nИзменён: {fi.LastWriteTime}";
            }
            else if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                info += $"Создан: {di.CreationTime}\nИзменён: {di.LastWriteTime}";
            }
            else return;
            MessageBox.Show(info, "Свойства");
        }

        // ---- Горячие клавиши ----
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.Left)) { Back_Click(null, null); return true; }
            if (keyData == (Keys.Alt | Keys.Right)) { Forward_Click(null, null); return true; }
            if (keyData == Keys.F5) { Refresh_Click(null, null); return true; }
            if (keyData == (Keys.Control | Keys.L)) { addressBox.Focus(); addressBox.SelectAll(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}