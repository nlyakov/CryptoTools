using CryptoTools.API;
using CryptoTools.API.Objects.Coins;
using CryptoTools.Forms;
using System;
using System.Windows.Forms;

namespace CryptoTools
{
    public partial class FormMain : Form
    {
        public ApiWorker api;

        public FormMain()
        {
            InitializeComponent();

            //CheckForIllegalCrossThreadCalls = false;
            WindowState = FormWindowState.Maximized;
            api = new ApiWorker();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormLogin formLogin = new FormLogin();
            DialogResult rslt = formLogin.ShowDialog();
            if (rslt == DialogResult.OK)
            {
                api.InitAuthentication(formLogin.decodedAuth);
            }
            else if (rslt != DialogResult.Ignore)
                Close();

            api.OpenSockets();
            lblExchanges.Text = "Exchanges: " + api.exchanges.Count.ToString();
            lblPairs.Text = "Pairs: " + api.pairs.Count.ToString();
            lblCoins.Text = "Coins: " + Coin.Count.ToString();
            api.OnOrderBooksUpdated += OrderBookUpdating;
        }

        public void SetStatus(string newStatus)
        {
            lblStatus.Text = "Status: " + newStatus;
        }

        public FormOffers OffersForm(bool activate)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(FormOffers))
                {
                    if (activate)
                        form.Activate();
                    return (FormOffers)form;
                }
            }

            FormOffers formOffers = new FormOffers(this);
            formOffers.Show();
            return formOffers;
        }

        private void APIKeysAndSecretsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormApiSettings frmApiSettings = new FormApiSettings();
            frmApiSettings.ShowDialog();
        }

        private void ExchangeGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormTickerData formTicker = new FormTickerData(this);
            formTicker.Show();
        }

        private void OrderBookUpdating(int pairid)
        {
            if (pairid == -1)
            {
                SetStatus("Updating OrderBooks completed");
                SetStatus("Sleeping");
                OffersForm(true).OrderBookUpdateFinished();
            }
            else
            {
                SetStatus("Updating OrderBook for " + Coin.Symbol(api.pairs[pairid].coin1) + "/" + Coin.Symbol(api.pairs[pairid].coin2));
                OffersForm(false).UpdateGridCalc();
            }
        }

        private void ExchangeTransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBestTransferDeals formDeals = new FormBestTransferDeals(this);
            formDeals.Show();

            formDeals.RefreshData();
        }

        private void BookDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OffersForm(true);
        }

        //static int msgcount = 0;
        //static ReaderWriterLock locker = new ReaderWriterLock();
        private void MessageHandler(string msg)
        {
            /*msgcount++;
            try
            {
                locker.AcquireWriterLock(int.MaxValue);
                System.IO.File.AppendAllLines("log.txt", new[] { msgcount.ToString() + ":  " + msg });
            }
            finally
            {
                locker.ReleaseWriterLock();
            }*/
            //lblStatus.Text = "Status: " + msgcount.ToString() + " messages received";// Invoke(new Action(delegate { lblStatus.Text = "Messages received: " + msgcount.ToString(); }));
            /*            textBox1.Invoke(
                            new Action(
                                delegate
                                {
                                    textBox1.AppendText(msg + "\r\n");
                                })
                            );*/
        }

        private void WithdrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //(api.exchanges[1].Rest as RestBitfinex).ApiKeyInfo();
            //(api.exchanges[1].Rest as RestBitfinex).Withdraw(0);
        }
    }
}
