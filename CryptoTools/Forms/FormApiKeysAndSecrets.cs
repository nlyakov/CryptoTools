using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static CryptoTools.Generic.Security;

namespace CryptoTools.Forms
{
    public partial class FormApiSettings : Form
    {
        public FormApiSettings()
        {
            InitializeComponent();
        }

        private void BtnLoadSettings_Click(object sender, EventArgs e)
        {
            try
            {
                memSettings.Clear();
                string data = File.ReadAllText(@"data.key");
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(KeyEncryptor.Decrypt(data, edtPassword.Text)));
                memSettings.Text = decoded;
            }
            catch
            {
                memSettings.Clear();
                MessageBox.Show("Inivalid login password");
                edtPassword.Focus();
            }
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            if (edtPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("Password should not be empty!");
                return;
            }
            try
            {
                string encoded = KeyEncryptor.Encrypt(Convert.ToBase64String(Encoding.UTF8.GetBytes(memSettings.Text)), edtPassword.Text);
                File.WriteAllText(@"data.key", encoded);
                edtPassword.Clear();
                MessageBox.Show("Settings are successfully encrypted with that password.");
            }
            catch
            {
                MessageBox.Show("Settings cannot be encrypted with that password!");
            }
        }

        private void BtnClearSettings_Click(object sender, EventArgs e)
        {
            memSettings.Clear();
            edtPassword.Clear();
        }
    }
}
