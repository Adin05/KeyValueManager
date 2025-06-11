using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using KeyValueManager.App.Services;

namespace KeyValueManager.App.Forms
{
    public partial class PasswordSetupForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly EncryptionService _encryptionService;
        private readonly bool _isFirstRun;
        private readonly TextBox txtPassword;
        private readonly TextBox txtConfirmPassword;
        private TextBox txtResetKey;
        private readonly Button btnSave;
        private readonly Button btnCancel;
        private readonly Label lblTitle;
        private Label lblResetKey;

        public PasswordSetupForm(DatabaseService databaseService, bool isFirstRun)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _encryptionService = new EncryptionService();
            _isFirstRun = isFirstRun;

            // Form settings
            this.Text = isFirstRun ? "Key Value Manager - Set Password" : "Key Value Manager - Reset Password";
            this.Size = new System.Drawing.Size(400, isFirstRun ? 250 : 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title label
            lblTitle = new Label
            {
                Text = isFirstRun ? "Set Password" : "Reset Password",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // Password input
            var lblPassword = new Label
            {
                Text = "Enter Password:",
                Location = new System.Drawing.Point(20, 50),
                AutoSize = true
            };
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(340, 20),
                PasswordChar = '•'
            };
            this.Controls.Add(txtPassword);

            // Confirm password input
            var lblConfirmPassword = new Label
            {
                Text = "Confirm Password:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };
            this.Controls.Add(lblConfirmPassword);

            txtConfirmPassword = new TextBox
            {
                Location = new System.Drawing.Point(20, 120),
                Size = new System.Drawing.Size(340, 20),
                PasswordChar = '•'
            };
            this.Controls.Add(txtConfirmPassword);

            // Reset key display/input (only show when resetting password)
            if (!isFirstRun)
            {
                lblResetKey = new Label
                {
                    Text = "Enter Reset Key:",
                    Location = new System.Drawing.Point(20, 150),
                    AutoSize = true
                };
                this.Controls.Add(lblResetKey);

                txtResetKey = new TextBox
                {
                    Location = new System.Drawing.Point(20, 170),
                    Size = new System.Drawing.Size(340, 20)
                };
                this.Controls.Add(txtResetKey);
            }
            else
            {
                // Initialize txtResetKey for first run (it will be used to store the generated key)
                txtResetKey = new TextBox();
            }

            // Save button
            btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(20, isFirstRun ? 160 : 210),
                Size = new System.Drawing.Size(75, 23)
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(285, isFirstRun ? 160 : 210),
                Size = new System.Drawing.Size(75, 23)
            };
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            // Generate reset key only when setting new password
            if (isFirstRun)
            {
                GenerateResetKey();
            }
        }

        private void GenerateResetKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var keyBytes = new byte[16];
                rng.GetBytes(keyBytes);
                txtResetKey.Text = Convert.ToBase64String(keyBytes);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!_isFirstRun)
            {
                if (string.IsNullOrWhiteSpace(txtResetKey.Text))
                {
                    MessageBox.Show("Please enter your reset key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var storedResetKey = await _databaseService.GetSettingAsync("ResetKey");
                if (string.IsNullOrEmpty(storedResetKey))
                {
                    MessageBox.Show("No reset key found.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var decryptedResetKey = _encryptionService.Decrypt(storedResetKey);
                    if (decryptedResetKey != txtResetKey.Text)
                    {
                        MessageBox.Show("Invalid reset key.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Error validating reset key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Hash the password
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Text));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                await _databaseService.SaveSettingAsync("Password", hashedPassword);
            }

            if (_isFirstRun)
            {
                // Generate and save a new reset key only when setting new password
                GenerateResetKey();
                var encryptedResetKey = _encryptionService.Encrypt(txtResetKey.Text);
                await _databaseService.SaveSettingAsync("ResetKey", encryptedResetKey);

                // Create a custom form for displaying the reset key
                using (var resetKeyForm = new Form())
                {
                    resetKeyForm.Text = "Key Value Manager - Save Reset Key";
                    resetKeyForm.Size = new System.Drawing.Size(400, 200);
                    resetKeyForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    resetKeyForm.MaximizeBox = false;
                    resetKeyForm.StartPosition = FormStartPosition.CenterScreen;

                    var lblMessage = new Label
                    {
                        Text = "Please save this reset key in a secure location:",
                        Location = new System.Drawing.Point(20, 20),
                        AutoSize = true
                    };
                    resetKeyForm.Controls.Add(lblMessage);

                    var txtKey = new TextBox
                    {
                        Text = txtResetKey.Text,
                        Location = new System.Drawing.Point(20, 50),
                        Size = new System.Drawing.Size(340, 20),
                        ReadOnly = true
                    };
                    resetKeyForm.Controls.Add(txtKey);

                    var btnCopy = new Button
                    {
                        Text = "Copy to Clipboard",
                        Location = new System.Drawing.Point(20, 80),
                        Size = new System.Drawing.Size(120, 23)
                    };
                    btnCopy.Click += (s, ev) =>
                    {
                        Clipboard.SetText(txtKey.Text);
                        MessageBox.Show("Reset key copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };
                    resetKeyForm.Controls.Add(btnCopy);

                    var btnOK = new Button
                    {
                        Text = "OK",
                        DialogResult = DialogResult.OK,
                        Location = new System.Drawing.Point(285, 120),
                        Size = new System.Drawing.Size(75, 23)
                    };
                    resetKeyForm.Controls.Add(btnOK);

                    resetKeyForm.AcceptButton = btnOK;
                    resetKeyForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Your password has been reset successfully.", 
                    "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PasswordSetupForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Name = "PasswordSetupForm";
            this.ResumeLayout(false);
        }
    }
} 