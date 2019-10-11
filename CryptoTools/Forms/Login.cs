using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static CryptoTools.Logics.Security;

namespace CryptoTools
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        public string decodedAuth = "";

        private bool CheckPassword(string pass)
        {
            try
            {
                string data = File.ReadAllText(@"data.key");
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(KeyEncryptor.Decrypt(data, pass)));
                dynamic jsnObj = JsonConvert.DeserializeObject(decoded);
                if ((bool)jsnObj.correct)
                    decodedAuth = decoded;
                return (bool)jsnObj.correct;
            }
            catch
            {
                return false;
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (CheckPassword(edtPassword.Text))
            {
                DialogResult = DialogResult.OK;
                return;
            }

            edtPassword.Clear();
            MessageBox.Show("Inivalid login password");
            edtPassword.Focus();
        }

        private void EdtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(this, new EventArgs());
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            return;
        }
    }
}
