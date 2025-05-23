using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyValueManager.App.Forms;
using KeyValueManager.App.Services;

namespace KeyValueManager.App;

static class Program
{
    private static readonly EncryptionService _encryptionService = new EncryptionService();
    private static readonly DatabaseService _databaseService = new DatabaseService("KeyValueManager.db", _encryptionService);

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (!CheckPasswordSetup())
        {
            Application.Exit();
            return;
        }

        Application.Run(new MainForm());
    }

    private static bool CheckPasswordSetup()
    {
        var password = _databaseService.GetSettingAsync("Password").Result;
        if (string.IsNullOrEmpty(password))
        {
            // First run - set up password
            using var form = new PasswordSetupForm(_databaseService, true);
            return form.ShowDialog() == DialogResult.OK;
        }
        else
        {
            // Check password
            using var form = new PasswordForm(_databaseService);
            return form.ShowDialog() == DialogResult.OK;
        }
    }
}