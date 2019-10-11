using CryptoTools.API.Exchanges;
using CryptoTools.API.Objects.Coins;
using CryptoTools.API.Objects.OrderBooks;
using CryptoTools.API.Objects.PairRelations;
using CryptoTools.API.Objects.Pairs;
using CryptoTools.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace CryptoTools.Forms
{
    enum enOffersGridSort { ogsUnsorted, ogsPairAsc, ogsPairDesc, ogsExchange1Asc, ogsExchange1Desc, ogsExchange2Asc, ogsExchange2Desc, ogsTT, ogsMT, ogsTM };

    public partial class FormOffers : Form
    {
        private FormMain fm;
        private double trade_amount = 100;
        private OrderBook[] eventbook = new OrderBook[2] { null, null };

        internal class GridRowStruct
        {
            public Relation r;
            public string Pair { get; set; }
            public string Exchange1 { get; set; }
            public string Exchange2 { get; set; }
            public string Amount { get; set; }
            public string TakerTaker { get; set; }
            public string MakerTaker { get; set; }
            public string TakerMaker { get; set; }
        }

        BindingSource bs = new BindingSource();
        List<GridRowStruct> FullList = new List<GridRowStruct>();
        List<GridRowStruct> WorkingList = new List<GridRowStruct>();
        enOffersGridSort OffersGridSortOrder = enOffersGridSort.ogsUnsorted;

        public FormOffers(FormMain formMain)
        {
            InitializeComponent();

            MdiParent = formMain;
            fm = formMain;

            InitFilters();
            InitList();
            InitGrid();
            lblOpportunities.Text = "Offers: " + fm.api.offers.Count.ToString();
            edtTradeAmount.Text = trade_amount.ToString();
        }

        private void InitFilters()
        {
            void AddIfNotExist(System.Windows.Forms.ComboBox cb, string item)
            {
                foreach (string cbi in cb.Items)
                    if (cbi==item)
                        return;

                cb.Items.Add(item);
                return;
            }

            cbExchange1.Items.Clear();
            cbExchange2.Items.Clear();
            for (int i = 0; i < fm.api.exchanges.Count; i++)
            {
                cbExchange1.Items.Add(fm.api.exchanges[i].Name);
                cbExchange2.Items.Add(fm.api.exchanges[i].Name);
            }
            cbExchange1.Sorted = true;
            cbExchange1.Sorted = false;
            cbExchange1.Items.Insert(0, "All");
            cbExchange1.SelectedIndex = 0;
            cbExchange1.SelectedIndexChanged += BtnRecalc_Click;
            cbExchange2.Sorted = true;
            cbExchange2.Sorted = false;
            cbExchange2.Items.Insert(0, "All");
            cbExchange2.SelectedIndex = 0;
            cbExchange2.SelectedIndexChanged += BtnRecalc_Click;

            cbCoin1.Items.Clear();
            cbCoin2.Items.Clear();
            for (int i = 0; i < fm.api.offers.Count; i++)
            {
                AddIfNotExist(cbCoin1, Coin.Symbol(fm.api.offers[i].ex1pair.coin1));
                AddIfNotExist(cbCoin2, Coin.Symbol(fm.api.offers[i].ex1pair.coin2));
            }
            cbCoin1.Sorted = true;
            cbCoin1.Sorted = false;
            cbCoin1.Items.Insert(0, "All");
            cbCoin1.SelectedIndex = 0;
            cbCoin2.SelectedIndexChanged += BtnRecalc_Click;
            cbCoin2.Sorted = true;
            cbCoin2.Sorted = false;
            cbCoin2.Items.Insert(0, "All");
            cbCoin2.SelectedIndex = 0;
            cbCoin1.SelectedIndexChanged += BtnRecalc_Click;
        }

        private void InitGrid()
        {
            bs.DataSource = typeof(GridRowStruct);
            dgvOffers.DataSource = bs;

            dgvOffers.ColumnHeadersHeight = 35;
            dgvOffers.Columns[0].HeaderText = "Pair";
            dgvOffers.Columns[0].Width = 180;
            dgvOffers.Columns[1].HeaderText = "Exchange 1";
            dgvOffers.Columns[1].Width = 70;
            dgvOffers.Columns[2].HeaderText = "Exchange 2";
            dgvOffers.Columns[2].Width = 70;
            dgvOffers.Columns[3].HeaderText = "Аmount";
            dgvOffers.Columns[3].Width = 60;
            dgvOffers.Columns[4].HeaderText = "Taker" + Environment.NewLine + "Taker";
            dgvOffers.Columns[4].Width = 60;
            dgvOffers.Columns[5].HeaderText = "Maker" + Environment.NewLine + "Taker";
            dgvOffers.Columns[5].Width = 60;
            dgvOffers.Columns[6].HeaderText = "Taker" + Environment.NewLine + "Maker";
            dgvOffers.Columns[6].Width = 60;

            bs.DataSource = WorkingList;
        }

        private void InitList()
        {
            void AddRowListItem(Relation r)
            {
                string pairstr = Coin.Symbol(fm.api.pairs[r.pairid1].coin1) + "/" + Coin.Symbol(fm.api.pairs[r.pairid1].coin2);
                if (r.pairid2 != r.pairid1)
                    pairstr += " to " + Coin.Symbol(fm.api.pairs[r.pairid2].coin1) + "/" + Coin.Symbol(fm.api.pairs[r.pairid2].coin2);

                FullList.Add(new GridRowStruct()
                {
                    r = r,
                    Pair = pairstr,
                    Exchange1 = r.ex1.Name,
                    Exchange2 = r.ex2.Name,
                    Amount = "",
                    TakerTaker = "",
                    MakerTaker = "",
                    TakerMaker = ""
                });
            }

            for (int i = 0; i < fm.api.offers.Count; i++)
            {
                AddRowListItem(fm.api.offers[i]);
                AddRowListItem(fm.api.offers[i].GetReversed());
            }
            WorkingList = FullList.ToList();
        }

        private void BtnUpdateVisible_Click(object sender, EventArgs e)
        {
            List<int> pklist = new List<int>();
            for (int i = 0; i < WorkingList.Count; i++)
            {
                if (!pklist.Contains(WorkingList[i].r.pairid1))
                    pklist.Add(WorkingList[i].r.pairid1);
                if (!pklist.Contains(WorkingList[i].r.pairid2))
                    pklist.Add(WorkingList[i].r.pairid2);
            }
            int[] pkeys = new int[pklist.Count];
            pklist.CopyTo(pkeys, 0);

            btnStop.Enabled = true;
            fm.SetStatus("Updating OrderBooks");
            fm.api.UpdateOrderBooks(pkeys);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            fm.api.OrderBookUpdateStop();
        }

        public void OrderBookUpdateFinished()
        {
            btnStop.Enabled = false;
        }

        private async void BtnUpdateRow_Click(object sender, System.EventArgs e)
        {
            int id = dgvOffers.CurrentCell.RowIndex;
            if (id == -1)
                return;

            Relation r = WorkingList[id].r;

            Task[] tasks = new Task[2];
            tasks[0] = Task.Factory.StartNew(() => r.ex1.RestUpdateOrderBook(r.pairid1));
            tasks[1] = Task.Factory.StartNew(() => r.ex2.RestUpdateOrderBook(r.pairid2));
            await Task.WhenAll(tasks);

            UpdateGridCalc();
            ShowOrderBooks(r);
        }

        private void UpdateRowCalc(GridRowStruct rs)
        {
            /*if (r.ex1pair.margin_supported)
                dgvOffers.Rows[i].Cells[1].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0xFA, 0xFF, 0xEB);
            else
                dgvOffers.Rows[i].Cells[1].Style.BackColor = System.Drawing.Color.White;*/
            /*if (r.ex2pair.margin_supported)
                dgvOffers.Rows[i].Cells[2].Style.BackColor = System.Drawing.Color.FromArgb(0xFF, 0xFA, 0xFF, 0xEB);
            else
                dgvOffers.Rows[i].Cells[2].Style.BackColor = System.Drawing.Color.White;*/


            if (rs.r.OrderBookAvailable())
            {
                double pair_amount = trade_amount;
                if (Coin.Info(rs.r.ex1pair.coin2).type == enCoinType.ctCrypto)
                    pair_amount = Math.Round(trade_amount / fm.api.CoinAnyPrice(rs.r.ex1pair.coin2), 4);
                if (pair_amount > 0)
                    rs.Amount = pair_amount.ToString();

                OfferCoefficients offer = rs.r.OrderBookCalc(rs.r.ex1pair.book, rs.r.ex2pair.book, pair_amount);
                rs.TakerTaker = offer.taker1taker2.ToString("P");
                rs.MakerTaker = offer.maker1taker2.ToString("P");
                rs.TakerMaker = offer.taker1maker2.ToString("P");
            }
            else
            {
                rs.Amount = "";

                rs.TakerTaker = "";
                rs.MakerTaker = "";
                rs.TakerMaker = "";
            }
        }

        public void UpdateGridCalc()
        {
            for (int i = 0; i < bs.Count; i++)
                UpdateRowCalc(WorkingList[i]);

            dgvOffers.Invoke(new MethodInvoker(delegate { dgvOffers.Refresh(); }));
        }

        private void ShowOrderBooks(Relation r)
        {
            void ShowBook(DataGridView grid, ExchangePair ep, OrderBookRecords offers)
            {
                grid.ColumnCount = 3;
                if (offers.type == enOrderBookType.obAsk)
                    grid.Columns[0].HeaderText = "ASK";
                else
                    grid.Columns[0].HeaderText = "BID";
                grid.Columns[0].Width = 130;
                grid.Columns[1].HeaderText = "Amount(" + Coin.Symbol(ep.coin1) + ")";
                grid.Columns[1].Width = 130;
                grid.Columns[2].HeaderText = "Value (" + Coin.Symbol(ep.coin2) + ")";
                grid.Columns[2].Width = 130;
                if (offers.dict.Count == 0)
                {
                    grid.RowCount = offers.dict.Count;
                }
                else
                {
                    grid.RowCount = offers.dict.Count;
                    int row = 0;
                    foreach (KeyValuePair<double, double> vp in offers.dict)
                    {
                        grid.Rows[row].Cells[0].Value = vp.Key.ToStringFull();
                        grid.Rows[row].Cells[1].Value = vp.Value.ToStringFull();
                        grid.Rows[row].Cells[2].Value = (vp.Key * vp.Value).ToStringFull();
                        row++;
                    }
                }
            }

            MakeHandler(0, r.ex1pair.book);
            MakeHandler(1, r.ex2pair.book);
            r.ex1pair.book.MakeSnapshot();
            r.ex2pair.book.MakeSnapshot();
            lblExchange1.Text = "Exchange: " + r.ex1.Name;
            lblPair1.Text = "Pair: " + fm.api.pairs[r.pairid1].PairName;
            //lblLastUpdate1.Text = r.ex1pair.book.LastUpdate.ToString();
            ShowBook(dgvEx1BookBid, r.ex1pair, r.ex1pair.book.bid);
            ShowBook(dgvEx1BookAsk, r.ex1pair, r.ex1pair.book.ask);
            lblExchange2.Text = "Exchange: " + r.ex2.Name;
            lblPair2.Text = "Pair: " + fm.api.pairs[r.pairid2].PairName;
            //lblLastUpdate2.Text = r.ex2pair.book.ToString();
            ShowBook(dgvEx2BookBid, r.ex2pair, r.ex2pair.book.bid);
            ShowBook(dgvEx2BookAsk, r.ex2pair, r.ex2pair.book.ask);
        }

        private void MakeHandler(int evid, OrderBook ob)
        {
            if (eventbook[evid] == ob)
                return;

            if (eventbook[evid] != null)
                eventbook[evid].OBChangedHandler -= OBChanged;

            eventbook[evid] = ob;
            eventbook[evid].OBChangedHandler += OBChanged;
        }

        private void OBChanged(OrderBook ob)
        {
            void UpdateBook(DataGridView grid, OrderBookRecords offers)
            {
                grid.Invoke(
                    new Action(() =>
                    {
                        if (offers.dict.Count == 0)
                        {
                                grid.RowCount = offers.dict.Count;
                        }
                        else
                        {
                            if (offers.dict.Count > grid.RowCount)
                                grid.RowCount = offers.dict.Count;
                            int row = 0;
                            foreach (KeyValuePair<double, double> vp in offers.dict)
                            {
                                grid.Rows[row].Cells[0].Value = vp.Key.ToStringFull();
                                grid.Rows[row].Cells[1].Value = vp.Value.ToStringFull();
                                grid.Rows[row].Cells[2].Value = (vp.Key * vp.Value).ToStringFull();
                                row++;
                            }
                            while (row < grid.RowCount)
                            {
                                grid.Rows[row].Cells[0].Value = "";
                                grid.Rows[row].Cells[1].Value = "";
                                grid.Rows[row].Cells[2].Value = "";
                                row++;
                            }
                        }
                    }));
            }

            if (ob == eventbook[0])
            {
                ob.MakeSnapshot();
                Invoke(new Action(() => { lblLastUpdate1.Text = ob.LastUpdate.ToString(); }));
                UpdateBook(dgvEx1BookBid, ob.bid);
                UpdateBook(dgvEx1BookAsk, ob.ask);
            }
            else if (ob == eventbook[1])
            {
                ob.MakeSnapshot();
                Invoke(new Action(() => { lblLastUpdate2.Text = ob.LastUpdate.ToString(); }));
                UpdateBook(dgvEx2BookBid, ob.bid);
                UpdateBook(dgvEx2BookAsk, ob.ask);
            }
            else
                return;

            if (dgvOffers.SelectedRows.Count > 0)
            {
                int id = dgvOffers.SelectedRows[0].Index;
                if (id != -1)
                    UpdateRowCalc(WorkingList[id]);
            }

        }

        private void EdtTradeAmount_Leave(object sender, EventArgs e)
        {
            double amount;
            if (double.TryParse(edtTradeAmount.Text, out amount))
                trade_amount = amount;
            else
                edtTradeAmount.Text = trade_amount.ToString();
        }

        private void BtnRecalc_Click(object sender, EventArgs e)
        {
            UpdateGridCalc();
            SortGrid();
            dgvOffers.Refresh();
        }

        private void DgvOffers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOffers.SelectedRows.Count == 0)
                return;
            int id = dgvOffers.SelectedRows[0].Index;
            if (id == -1)
                return;

            ShowOrderBooks(WorkingList[id].r);
        }

        private void FilterIndexChanged(object sender, EventArgs e)
        {
            WorkingList = FullList.ToList();
            if (cbCoin1.SelectedIndex > 0)
                WorkingList = WorkingList.FindAll(p => fm.api.pairs[p.r.pairid1].coin1 == Coin.AddOrGet(cbCoin1.SelectedItem.ToString())).ToList();
            if (cbCoin2.SelectedIndex > 0)
                WorkingList = WorkingList.FindAll(p => fm.api.pairs[p.r.pairid1].coin2 == Coin.AddOrGet(cbCoin2.SelectedItem.ToString()) || fm.api.pairs[p.r.pairid2].coin2 == Coin.AddOrGet(cbCoin2.SelectedItem.ToString()));
            if (cbExchange1.SelectedIndex > 0)
                WorkingList = WorkingList.FindAll(p => p.Exchange1 == cbExchange1.SelectedItem.ToString());
            if (cbExchange2.SelectedIndex > 0)
                WorkingList = WorkingList.FindAll(p => p.Exchange2 == cbExchange2.SelectedItem.ToString());

            bs.DataSource = WorkingList;
            SortGrid();
            dgvOffers.Refresh();
        }

        private void SortGrid()
        {
            int ComparePerc(string x, string y)
            {
                if (y == "")
                    y = "-9999999999999";
                if (x == "")
                    x = "-9999999999999";

                double val1;
                if (!double.TryParse(x.Replace('%', ' '), out val1))
                    val1 = -1;
                double val2;
                if (!double.TryParse(y.Replace('%', ' '), out val2))
                    val2 = -1;
                return val2.CompareTo(val1);
            }

            switch (OffersGridSortOrder)
            {
                case enOffersGridSort.ogsUnsorted:
                    break;
                case enOffersGridSort.ogsPairAsc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return x.Pair.CompareTo(y.Pair);
                    });
                    break;
                case enOffersGridSort.ogsPairDesc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return y.Pair.CompareTo(x.Pair);
                    });
                    break;
                case enOffersGridSort.ogsExchange1Asc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return x.Exchange1.CompareTo(y.Exchange1);
                    });
                    break;
                case enOffersGridSort.ogsExchange1Desc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return y.Exchange1.CompareTo(x.Exchange1);
                    });
                    break;
                case enOffersGridSort.ogsExchange2Asc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return x.Exchange2.CompareTo(y.Exchange2);
                    });
                    break;
                case enOffersGridSort.ogsExchange2Desc:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return y.Exchange2.CompareTo(x.Exchange2);
                    });
                    break;
                case enOffersGridSort.ogsTT:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return ComparePerc(x.TakerTaker, y.TakerTaker);
                    });
                    break;
                case enOffersGridSort.ogsMT:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return ComparePerc(x.MakerTaker, y.MakerTaker);
                    });
                    break;
                case enOffersGridSort.ogsTM:
                    WorkingList.Sort(delegate (GridRowStruct x, GridRowStruct y)
                    {
                        return ComparePerc(x.TakerMaker, y.TakerMaker);
                    });
                    break;
            }
        }

        private void DgvOffers_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    if (OffersGridSortOrder == enOffersGridSort.ogsPairAsc)
                        OffersGridSortOrder = enOffersGridSort.ogsPairDesc;
                    else
                        OffersGridSortOrder = enOffersGridSort.ogsPairAsc;
                    break;
                case 1:
                    if (OffersGridSortOrder == enOffersGridSort.ogsExchange1Asc)
                        OffersGridSortOrder = enOffersGridSort.ogsExchange1Desc;
                    else
                        OffersGridSortOrder = enOffersGridSort.ogsExchange1Asc;
                    break;
                case 2:
                    if (OffersGridSortOrder == enOffersGridSort.ogsExchange2Asc)
                        OffersGridSortOrder = enOffersGridSort.ogsExchange2Desc;
                    else
                        OffersGridSortOrder = enOffersGridSort.ogsExchange2Asc;
                    break;
                case 4:
                    OffersGridSortOrder = enOffersGridSort.ogsTT;
                    break;
                case 5:
                    OffersGridSortOrder = enOffersGridSort.ogsMT;
                    break;
                case 6:
                    OffersGridSortOrder = enOffersGridSort.ogsTM;
                    break;
            }

            SortGrid();
            dgvOffers.Refresh();
        }

        private void FormOffers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (eventbook[0] != null)
                eventbook[0].OBChangedHandler -= OBChanged;
            if (eventbook[1] != null)
                eventbook[1].OBChangedHandler -= OBChanged;
        }
    }
}
