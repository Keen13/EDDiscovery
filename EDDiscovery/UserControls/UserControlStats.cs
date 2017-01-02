﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace EDDiscovery.UserControls
{
    public partial class UserControlStats : UserControlCommonBase
    {
        private int displaynumber = 0;
        private EDDiscoveryForm discoveryform;
        private TravelHistoryControl travelhistorycontrol;

        public UserControlStats()
        {
            InitializeComponent();
        }

        public override void Init( EDDiscoveryForm ed, int vn) //0=primary, 1 = first windowed version, etc
        {
            discoveryform = ed;
            travelhistorycontrol = ed.TravelControl;
            displaynumber = vn;
            discoveryform.OnNewEntry += AddNewEntry;
            travelhistorycontrol.OnTravelSelectionChanged += SelectionChanged;
        }

        public override void Closing()
        {
            discoveryform.OnNewEntry -= AddNewEntry;
            travelhistorycontrol.OnTravelSelectionChanged -= SelectionChanged;
        }

        private void AddNewEntry(HistoryEntry he, HistoryList hl)
        {
            Stats(he,hl);
        }

        public void SelectionChanged(HistoryEntry he, HistoryList hl)
        {
            Stats(he, hl);
        }

        private void Stats(HistoryEntry he, HistoryList hl)
        {
            if (tabControlCustomStats.SelectedIndex == 0)
            {
                StatsGeneral(he, hl);
            }
            if (tabControlCustomStats.SelectedIndex == 1)
            {
                StatsTravel(he, hl);
            }

        }

        private void StatsGeneral(HistoryEntry he, HistoryList hl)
        {
            dataGridViewStats.Rows.Clear();

            if (he != null)
            {
                StatToDGV("Visits", hl.GetVisitsCount(he.System.name, he.System.id_edsm) + " to system " + he.System.name);
                StatToDGV("Jumps Before System", hl.GetFSDJumpsBeforeUTC(he.EventTimeUTC));
            }

            int totaljumps = hl.GetFSDJumps(new TimeSpan(10000, 0, 0, 0));
            StatToDGV("Total No of jumps: ", totaljumps);
            if (totaljumps > 0)
            {
                StatToDGV("Jump History", "24 Hours: " + hl.GetFSDJumps(new TimeSpan(1, 0, 0, 0)) +
                                      ", One Week: " + hl.GetFSDJumps(new TimeSpan(7, 0, 0, 0)) +
                                      ", 30 Days: " + hl.GetFSDJumps(new TimeSpan(30, 0, 0, 0)) +
                                      ", One Year: " + hl.GetFSDJumps(new TimeSpan(365, 0, 0, 0))
                                      );

                HistoryEntry north = hl.GetConditionally(Double.MinValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.z > l; if (v) l = s.System.z; return v; });

                HistoryEntry south = hl.GetConditionally(Double.MaxValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.z < l; if (v) l = s.System.z; return v; });

                HistoryEntry east = hl.GetConditionally(Double.MinValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.x > l; if (v) l = s.System.x; return v; });

                HistoryEntry west = hl.GetConditionally(Double.MaxValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.x < l; if (v) l = s.System.x; return v; });

                HistoryEntry up = hl.GetConditionally(Double.MinValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.y > l; if (v) l = s.System.y; return v; });

                HistoryEntry down = hl.GetConditionally(Double.MaxValue, (HistoryEntry s, ref double l) =>
                { bool v = s.IsFSDJump && s.System.HasCoordinate && s.System.y < l; if (v) l = s.System.y; return v; });

                StatToDGV("Most North", north.System.name + " @ " + north.System.x.ToString("0.0") + "," + north.System.y.ToString("0.0") + "," + north.System.z.ToString("0.0"));
                StatToDGV("Most South", south.System.name + " @ " + south.System.x.ToString("0.0") + "," + south.System.y.ToString("0.0") + "," + south.System.z.ToString("0.0"));
                StatToDGV("Most East", east.System.name + " @ " + east.System.x.ToString("0.0") + "," + east.System.y.ToString("0.0") + "," + east.System.z.ToString("0.0"));
                StatToDGV("Most West", west.System.name + " @ " + west.System.x.ToString("0.0") + "," + west.System.y.ToString("0.0") + "," + west.System.z.ToString("0.0"));
                StatToDGV("Most Highest", up.System.name + " @ " + up.System.x.ToString("0.0") + "," + up.System.y.ToString("0.0") + "," + up.System.z.ToString("0.0"));
                StatToDGV("Most Lowest", down.System.name + " @ " + down.System.x.ToString("0.0") + "," + down.System.y.ToString("0.0") + "," + down.System.z.ToString("0.0"));


                var groupeddata = from data in hl.OrderByDate
                                  where data.IsFSDJump
                                  group data by data.System.name
                                      into grouped
                                  select new
                                  {
                                      Title = grouped.Key,
                                      Count = grouped.Count()
                                  };

                mostVisited.Visible = true;

                Color GridC = discoveryform.theme.GridCellText;
                Color TextC = discoveryform.theme.VisitedSystemColor;
                mostVisited.Titles.Clear();
                mostVisited.Titles.Add(new Title("Most Visited", Docking.Top, discoveryform.theme.GetFont, TextC));
                mostVisited.Series[0].Points.Clear();

                mostVisited.ChartAreas[0].AxisX.LabelStyle.ForeColor = TextC;
                mostVisited.ChartAreas[0].AxisY.LabelStyle.ForeColor = TextC;
                mostVisited.ChartAreas[0].AxisX.MajorGrid.LineColor = GridC;
                mostVisited.ChartAreas[0].AxisX.MinorGrid.LineColor = GridC;
                mostVisited.ChartAreas[0].AxisY.MajorGrid.LineColor = GridC;
                mostVisited.ChartAreas[0].AxisY.MinorGrid.LineColor = GridC;
                mostVisited.ChartAreas[0].BorderColor = GridC;
                mostVisited.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
                mostVisited.ChartAreas[0].BorderWidth = 2;

                mostVisited.ChartAreas[0].BackColor = Color.Transparent;
                mostVisited.Series[0].Color = GridC;
                mostVisited.BorderlineColor = Color.Transparent;

                int i = 0;
                foreach (var data in from a in groupeddata orderby a.Count descending select a)
                {
                    if (data.Count <= 1 || i == 10)
                        break;

                    mostVisited.Series[0].Points.Add(new DataPoint(i, data.Count));
                    mostVisited.Series[0].Points[i].AxisLabel = data.Title;
                    mostVisited.Series[0].Points[i].LabelForeColor = TextC;
                    i++;
                }
            }
            else
                mostVisited.Visible = false;

            SizeControls();
        }

        void StatsTravel(HistoryEntry he, HistoryList hl)
        {
            if (userControlStatsTimeTravel.TimeMode == UserControlStatsTimeModeEnum.Summary)
            {
                dataGridViewTravel.Rows.Clear();
                dataGridViewTravel.Columns.Clear();
                dataGridViewTravel.Dock = DockStyle.Fill;
                dataGridViewTravel.Visible = true;
                    

                var Col1 = new DataGridViewTextBoxColumn();
                Col1.HeaderText = "Last";

                var Col2 = new DataGridViewTextBoxColumn();
                Col2.HeaderText = "24 hours";

                var Col3 = new DataGridViewTextBoxColumn();
                Col3.HeaderText = "week";

                var Col4 = new DataGridViewTextBoxColumn();
                Col4.HeaderText = "month";

                var Col5 = new DataGridViewTextBoxColumn();
                Col5.HeaderText = "all";

                dataGridViewTravel.Columns.AddRange(new DataGridViewColumn[] { Col1, Col2, Col3, Col4, Col5 });


                int[] intar = new int[4];
                string[] strarr = new string[4];


                intar[0] = hl.GetFSDJumps(new TimeSpan(1, 0, 0, 0));
                intar[1] = hl.GetFSDJumps(new TimeSpan(7, 0, 0, 0));
                intar[2] = hl.GetFSDJumps(new TimeSpan(30, 0, 0, 0));
                intar[3] = hl.GetFSDJumps(new TimeSpan(36500, 0, 0, 0));
                StatToDGV(dataGridViewTravel, "Jumps", intar);


                intar[0] = hl.GetDocked(DateTime.Now.AddDays(-1), DateTime.Now);
                intar[1] = hl.GetDocked(DateTime.Now.AddDays(-7), DateTime.Now);
                intar[2] = hl.GetDocked(DateTime.Now.AddDays(-30), DateTime.Now);
                intar[3] = hl.GetDocked(new DateTime(2012,1,1), DateTime.Now);
                StatToDGV(dataGridViewTravel, "Docked", intar);

                intar[0] = hl.GetTouchDown(DateTime.Now.AddDays(-1), DateTime.Now);
                intar[1] = hl.GetTouchDown(DateTime.Now.AddDays(-7), DateTime.Now);
                intar[2] = hl.GetTouchDown(DateTime.Now.AddDays(-30), DateTime.Now);
                intar[3] = hl.GetTouchDown(new DateTime(2012, 1, 1), DateTime.Now);
                StatToDGV(dataGridViewTravel, "Landed", intar);





            }
        }

        void SizeControls()
        { 
            int height = 0;
            foreach (DataGridViewRow row in dataGridViewStats.Rows)
            {
                height += row.Height + 1;
            }
            height += dataGridViewStats.ColumnHeadersHeight + 2;
            dataGridViewStats.Size = new Size(panelData.DisplayRectangle.Width - panelData.ScrollBarWidth, height);             // all controls should be placed each time.
            //System.Diagnostics.Debug.WriteLine("DGV {0} {1}", dataGridViewStats.Size, dataGridViewStats.Location);
            mostVisited.Location = new Point(0, height);
            mostVisited.Size = new Size(panelData.DisplayRectangle.Width - panelData.ScrollBarWidth, mostVisited.Height);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);       
            SizeControls();         // need to size controls here as well.. goes tabstrip.. create user control.. calls updatestats with incorrect size.. added to UC panel.. relayout
        }

        void StatToDGV(string title, string data)
        {
            object[] rowobj = { title, data };
            dataGridViewStats.Rows.Add(rowobj);
        }

        void StatToDGV(string title, int data)
        {
            object[] rowobj = { title, data.ToString() };
            dataGridViewStats.Rows.Add(rowobj);
        }

        void StatToDGV(DataGridView datagrid,  string title, string[] data)
        {
            object[] rowobj = new object[data.Length + 1];

            rowobj[0] = title;
            for (int ii = 0; ii < data.Length; ii++)
                rowobj[ii + 1] = data[ii];

            datagrid.Rows.Add(rowobj);
        }

        void StatToDGV(DataGridView datagrid, string title, int[] data)
        {
            object[] rowobj = new object[data.Length + 1];

            rowobj[0] = title;
            for (int ii = 0; ii < data.Length; ii++)
                rowobj[ii + 1] = data[ii].ToString();

            datagrid.Rows.Add(rowobj);
        }


        private void panelData_Paint(object sender, PaintEventArgs e)
        {

        }

        private void userControlStatsTimeTravel_TimeModeChanged(object sender, EventArgs e)
        {

        }

        private void userControlStatsTimeTravel_DrawModeChanged(object sender, EventArgs e)
        {

        }
    }
}
