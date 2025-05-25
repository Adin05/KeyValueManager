using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using KeyValueManager.App.Models;
using KeyValueManager.App.Services;

namespace KeyValueManager.App.Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly EncryptionService _encryptionService;
        private System.Windows.Forms.Timer _clipboardTimer;
        private string _lastCopiedValue;

        public MainForm()
        {
            InitializeComponent();
            _encryptionService = new EncryptionService();
            _databaseService = new DatabaseService("KeyValueManager.db", _encryptionService);
            InitializeClipboardTimer();
            LoadData();
        }

        private void InitializeClipboardTimer()
        {
            _clipboardTimer = new System.Windows.Forms.Timer();
            _clipboardTimer.Interval = 60000; // 1 minute
            _clipboardTimer.Tick += ClipboardTimer_Tick;
        }

        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastCopiedValue))
            {
                Clipboard.Clear();
                _lastCopiedValue = null;
                _clipboardTimer.Stop();
            }
        }

        private async void LoadData()
        {
            try
            {
                var entries = await _databaseService.GetAllEntriesAsync();
                dataGridView.DataSource = entries;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            using var form = new EntryForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _databaseService.AddEntryAsync(form.Entry);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) return;

            var entry = (KeyValueEntry)dataGridView.SelectedRows[0].DataBoundItem;
            using var form = new EntryForm(entry);
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _databaseService.UpdateEntryAsync(form.Entry);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0) return;

            var entry = (KeyValueEntry)dataGridView.SelectedRows[0].DataBoundItem;
            if (MessageBox.Show($"Are you sure you want to delete the entry '{entry.Key}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    await _databaseService.DeleteEntryAsync(entry.Key);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var entry = (KeyValueEntry)dataGridView.Rows[e.RowIndex].DataBoundItem;
            string valueToCopy = null;

            switch (e.ColumnIndex)
            {
                case 2: // Value1
                    valueToCopy = entry.Value1;
                    break;
                case 3: // Value2
                    valueToCopy = entry.Value2;
                    break;
                case 4: // Value3
                    valueToCopy = entry.Value3;
                    break;
            }

            if (!string.IsNullOrEmpty(valueToCopy))
            {
                Clipboard.SetText(valueToCopy);
                _lastCopiedValue = valueToCopy;
                _clipboardTimer.Start();
                MessageBox.Show("Value copied to clipboard. It will be cleared in 1 minute.", "Copied",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(12, 41);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(776, 397);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(93, 12);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(174, 12);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dataGridView);
            this.Name = "MainForm";
            this.Text = "Key Value Manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
    }
} 