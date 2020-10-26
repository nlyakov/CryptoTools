using CryptoTools.API.Objects;
using CryptoTools.API.Objects.Coins;
using CryptoTools.Generic.Helpers;
using System.Windows.Forms;

namespace CryptoTools.Forms
{
    public partial class FormBestTransferDeals : Form
    {
        private FormMain fm;

        public FormBestTransferDeals(FormMain formMain)
        {
            InitializeComponent();

            MdiParent = formMain;
            fm = formMain;

            dgvTransferDeals.Rows.Clear();
            dgvTransferDeals.Columns.Clear();
            dgvTransferDeals.Columns.Add("colExchange1", "Exchange 1");
            dgvTransferDeals.Columns[0].Width = 70;
            dgvTransferDeals.Columns.Add("colExchange2", "Exchange 2");
            dgvTransferDeals.Columns[1].Width = 70;
            dgvTransferDeals.Columns.Add("colPair", "Pair");
            dgvTransferDeals.Columns[2].Width = 50;
            dgvTransferDeals.Columns.Add("colCoeff", "Coeff");
            dgvTransferDeals.Columns[3].Width = 60;

            //fm.api.OnTickersUpdated += RefreshData;
        }

        public void RefreshData()
        {
            fm.api.RefreshTransferDeals(500);

            dgvTransferDeals.Rows.Clear();
            if (fm.api.deals.Count > 0)
                dgvTransferDeals.Rows.Add(fm.api.deals.Count - 1);

            for (int i = 0; i < fm.api.deals.Count; i++)
            {
                TransferDeal deal = fm.api.deals[i];
                dgvTransferDeals.Rows[i].Cells[0].Value = fm.api.exchanges[deal.exch1].Name;
                dgvTransferDeals.Rows[i].Cells[1].Value = fm.api.exchanges[deal.exch2].Name;
                dgvTransferDeals.Rows[i].Cells[2].Value = Coin.Symbol(fm.api.pairs[deal.pairid].coin1) + "/" + Coin.Symbol(fm.api.pairs[deal.pairid].coin2);
                dgvTransferDeals.Rows[i].Cells[3].Value = deal.coeff.ToStringFull();
            }
        }
    }
}
