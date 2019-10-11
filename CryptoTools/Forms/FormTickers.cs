using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.PairRelations;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Units;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CryptoTools.Forms
{
    public partial class FormTickerData : Form
    {
        private FormMain fm;

        public FormTickerData(FormMain formMain)
        {
            InitializeComponent();

            MdiParent = formMain;
            fm = formMain;

            dgvTicker.Rows.Clear();
            dgvTicker.Columns.Clear();
            dgvTicker.Columns.Add("colCrypto", "Pairs");
            dgvTicker.Columns[0].Width = 50;
            for (int i = 0; i < fm.api.exchanges.Count; i++)
            {
                dgvTicker.Columns.Add("colEx" + i.ToString(), fm.api.exchanges[i].Name);
                dgvTicker.Columns[i + 1].Width = 300;
            }

            lblAllPairs.Text = "All pairs: " + fm.api.pairs.Count.ToString();

            BtnUpdate_Click(null, null);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            fm.api.OnTickersUpdated += TickerUpdated;
            fm.SetStatus("Updating tickers");
            fm.api.UpdateTickers();
            fm.api.OnTickersUpdated -= TickerUpdated;
        }

        private void TickerUpdated(int id)
        {
            UpdateGrid();
            fm.SetStatus("Sleeping");
        }

        private void UpdateGrid()
        {
            dgvTicker.Rows.Clear();
            dgvTicker.Rows.Add(fm.api.pairs.Count);
            for (int i = 0; i < dgvTicker.RowCount; i++)
                dgvTicker.Rows[i].Height = 120;

            bool is_full_checked = toolbtnAllPairs.Checked;
            string coinfilter = edtFilter.Text.ToUpper().Trim();

            foreach (KeyValuePair<int, PairInfo> vp in fm.api.pairs)
            {
                dgvTicker.Rows[vp.Value.index].Cells[0].Value =
                    vp.Value.index.ToString() + Environment.NewLine + Environment.NewLine +
                    Coin.Symbol(vp.Value.coin1) + Environment.NewLine + Coin.Symbol(vp.Value.coin2);

                if (coinfilter != "" || !is_full_checked)
                    dgvTicker.Rows[vp.Value.index].Visible = false;
            }

            for (int x = 0; x < fm.api.exchanges.Count; x++)
                foreach (KeyValuePair<int, ExchangePair> vp in fm.api.exchanges[x].pair)
                {
                    dgvTicker.Rows[vp.Value.index].Cells[x + 1].Value = "";
                    if (vp.Value.trade_supported && vp.Value.TickerAvailable)
                        dgvTicker.Rows[vp.Value.index].Cells[x + 1].Value = "Ticker: (bid) " + vp.Value.ticker.bid.ToStringFull() + "   (ask) " + vp.Value.ticker.ask.ToStringFull();
                    dgvTicker.Rows[vp.Value.index].Cells[x + 1].Style.BackColor = System.Drawing.Color.White;
                }

            for (int rid = 0; rid < fm.api.offers.Count; rid++)
            {
                Relation r = fm.api.offers[rid];
                if (r.pairid1 != r.pairid2)
                    continue;

                int row = fm.api.pairs[r.pairid1].index;
                if (coinfilter != "")
                    if (Coin.Symbol(r.ex1pair.coin1).Contains(coinfilter) || Coin.Symbol(r.ex1pair.coin2).Contains(coinfilter))
                        dgvTicker.Rows[row].Visible = true;

                if (!is_full_checked)
                    if (r.TickerAvailable() && fm.api.pairs[r.pairid1].usable)
                        dgvTicker.Rows[row].Visible = true;

                if (r.TickerAvailable())
                {
                    OfferCoefficients coeff = r.TickerCalc(r.ex1pair.ticker, r.ex2pair.ticker);
                    dgvTicker.Rows[row].Cells[r.ex1id + 1].Value += Environment.NewLine + r.ex2.Name + ":" +
                            "   " + char.ConvertFromUtf32(0x2192) + " " + coeff.taker1taker2.ToString("P") + "   " + char.ConvertFromUtf32(0x2190) + " " + coeff.taker2taker1.ToString("P") +
                            "   " + char.ConvertFromUtf32(0x2192) + " " + coeff.taker1maker2.ToString("P") + "   " + char.ConvertFromUtf32(0x2190) + " " + coeff.taker2taker1.ToString("P");
                    dgvTicker.Rows[row].Cells[r.ex2id + 1].Value += Environment.NewLine + r.ex1.Name + ":" +
                            "   " + char.ConvertFromUtf32(0x2192) + " " + coeff.taker2taker1.ToString("P") + "   " + char.ConvertFromUtf32(0x2190) + " " + coeff.taker1taker2.ToString("P") +
                            "   " + char.ConvertFromUtf32(0x2192) + " " + coeff.taker2maker1.ToString("P") + "   " + char.ConvertFromUtf32(0x2190) + " " + coeff.taker1maker2.ToString("P");
                    double maxdirect = Math.Max(coeff.taker1taker2, coeff.taker2taker1);
                    double maxwait = Math.Max(coeff.taker1maker2, coeff.taker2maker1);
                    if (maxdirect >= 0.01)
                    {
                        dgvTicker.Rows[row].Cells[r.ex1id + 1].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0xDB, 0x69);
                        dgvTicker.Rows[row].Cells[r.ex1id + 1].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0xDB, 0x69);
                    }
                    else if (maxwait > 0.01)
                    {
                        dgvTicker.Rows[row].Cells[r.ex2id + 1].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xF0, 0x4F);
                        dgvTicker.Rows[row].Cells[r.ex2id + 1].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xF0, 0x4F);
                    }
                }
            }

            lblDisplayedLabels.Text = "Displayed pairs: " + dgvTicker.Rows.GetRowCount(DataGridViewElementStates.Visible).ToString();
            lblLastUpdated.Text = "Last updated: " + DateTime.Now.ToString();
        }

        private void FormTickerData_FormClosing(object sender, FormClosingEventArgs e)
        {
            fm.api.OnTickersUpdated -= TickerUpdated;
        }

        private void BtnUpdateOrderBooks_Click(object sender, EventArgs e)
        {
            fm.SetStatus("Updating OrderBooks");
            fm.api.UpdateOrderBooks(null);
        }

        private void BtnUpdateFullOrderBooks_Click(object sender, EventArgs e)
        {
            fm.SetStatus("Updating OrderBooks");
            fm.api.UpdateOrderBooks(null);

        }
    }
}
