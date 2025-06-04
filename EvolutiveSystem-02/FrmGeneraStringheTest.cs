using MIU.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EvolutiveSystem_02
{
    public partial class FrmGeneraStringheTest : Form
    {
        private MIUStringGenerator mIUStringGenerator = null;
        private System.Windows.Forms.ProgressBar dbload;
        private Label statusLabel = null;
        private List<string> field;
        private HashSet<string> MIUstring = new HashSet<string>();
        private int stepProces = 0;
        private string _connectionString = "";

        private System.Windows.Forms.ToolTip toolTip;
        public FrmGeneraStringheTest(List<string> Field, string ConnectionString)
        {
            InitializeComponent();
            this.field = Field;
            mIUStringGenerator = new MIUStringGenerator();
            mIUStringGenerator.GenerationProgressChanged += MIUStringGenerator_GenerationProgressChanged;
            toolTip = new System.Windows.Forms.ToolTip();
            this._connectionString = ConnectionString;
        }

        private void MIUStringGenerator_GenerationProgressChanged(object sender, GenerationProgressEventArgs e)
        {
            statusLabel.Text = "Inserimento lista stringhe MIU nel database";
            dbload.Value = e.Current;
        }

        private void FrmGeneraStringheTest_Load(object sender, EventArgs e)
        {
            int maxList = 0;
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.ColumnCount = 3;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Dock = DockStyle.Top; // O dove preferisci
            panel.Name = "Contenitore";
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            string[] record = null;
            int cnt = 0;
            Label lblName = null;
            Label lblvalue = null;
            Label lbldescription = null;
            foreach (string s in field)
            {
                record = s.Split(',');
                lblName = new Label()
                {
                    Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.Blue,
                    Text = string.Format($"{record[1].Trim()}: "),
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleRight,
                    //BackColor = (cnt % 2 == 0) ? Color.LightGray : Color.White, // Colore di sfondo alternato per la label del nome
                    //BorderStyle = BorderStyle.FixedSingle // Aggiungi un bordo
                };
                this.toolTip.SetToolTip(lblName, record[3]); // *** Aggiunto ToolTip ***
                panel.Controls.Add(lblName, 0, cnt);
                lblvalue = new Label()
                {
                    Font = new Font(this.Font.FontFamily, 10),
                    ForeColor = Color.Green,
                    Text = string.Format($"{record[2]}"),
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    //BackColor = (cnt % 2 == 0) ? Color.LightGray : Color.White, // Colore di sfondo alternato per la label del nome
                    //BorderStyle = BorderStyle.FixedSingle // Aggiungi un bordo
                };
                if (lblName.Text.Contains("NumeroStringheDaGenerare:"))
                {
                    maxList = Convert.ToInt32(lblvalue.Text);
                }
                panel.Controls.Add(lblvalue, 1, cnt);
                lbldescription = new Label()
                {
                    Font = new Font(this.Font.FontFamily, 10),
                    Text = string.Format($"{record[3]}"),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    //BackColor = (cnt % 2 == 0) ? Color.LightGray : Color.White, // Colore di sfondo alternato per la label del nome
                    //BorderStyle = BorderStyle.FixedSingle // Aggiungi un bordo
                };
                panel.Controls.Add(lbldescription, 2, cnt);
                panel.RowCount++;
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.RowStyles[cnt] = new RowStyle(SizeType.AutoSize);
                cnt++;
            }
            this.Controls.Add(panel);
            Panel pnlProgressBar = new Panel()
            {
                BackColor = Color.WhiteSmoke,
                Dock = DockStyle.Bottom,
                Height = 200
            };
            this.Controls.Add(pnlProgressBar);
            dbload = new System.Windows.Forms.ProgressBar();
            dbload.Height = 20;
            dbload.Top = 60;
            dbload.Left = 20;
            dbload.Width = pnlProgressBar.Width - 40;
            dbload.BackColor = Color.Blue;
            dbload.Maximum = maxList;
            pnlProgressBar.Controls.Add(dbload);
            System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button()
            {
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "BtnCancel",
                Text = "Close",
                Width = 80,
                Height = 40,
                Top = 100
            };
            btnCancel.Click += BtnCancel_Click;
            pnlProgressBar.Controls.Add(btnCancel);
            btnCancel.Left = (pnlProgressBar.Width - btnCancel.Width) / 2;
            statusLabel = new Label()
            {
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                ForeColor = Color.Red,
                Left = 10,
                Top = 10,
                Text = "...",
            };
            pnlProgressBar.Controls.Add(statusLabel);
            tmrStart.Enabled = true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddRowToDb()
        {
            int numberOfString = 0;
            int minLength = 0;
            int maxLength = 0;
            int minLevenshtein = 0;
            string StringaIniziale = "";
            bool check = true;
            string[] record = null;
            foreach (string s in field)
            {
                record = s.Split(',');
                switch (record[1].Trim())
                {
                    case "NumeroStringheDaGenerare":
                        {
                            check = int.TryParse(record[2], out numberOfString);
                        }
                        break;
                    case "LunghezzaMassimaStringa":
                        {
                            check = int.TryParse(record[2], out maxLength);
                        }
                        break;
                    case "SogliaLevenshtein":
                        {
                            check = int.TryParse(record[2], out minLevenshtein);
                        }
                        break;
                    case "StringaIniziale":
                        {
                            StringaIniziale = record[2].Trim();
                        }
                        break;
                    case "LunghezzaMinimaStringa":
                        {
                            check = int.TryParse(record[2], out minLength);
                        }
                        break;
                }

            }
            if (check) this.MIUstring = mIUStringGenerator.GenerateChaoticStrings(numberOfString, minLength, maxLength, minLevenshtein);
            //using (StreamWriter sw = new StreamWriter("ListMiu.txt")) 
            //{
            //    foreach(string s in this.MIUstring)
            //    {
            //        sw.WriteLine(s);
            //    }
            //}
        }

        private void tmrStart_Tick(object sender, EventArgs e)
        {
            switch (stepProces)
            {
                case 0:
                    {
                        tmrStart.Enabled = false;
                        statusLabel.Text = "Calcolo lista stringhe MIU";
                        AddRowToDb();
                        stepProces = 1;
                        tmrStart.Enabled = true;
                    }
                    break;
                case 1:
                    {
                        tmrStart.Enabled = false;
                        statusLabel.Text = "Inserimento lista stringhe MIU nel database";
                        this.dbload.Value = 0;
                        SalvaStringheInMIUStates(this.MIUstring, this._connectionString);
                        stepProces = 2;
                        tmrStart.Enabled = true;
                    }
                    break;
                case 2:
                    {
                        tmrStart.Enabled = false;
                        statusLabel.Text = "operazione terminata";
                    }
                    break;
            }
        }
        public void SalvaStringheInMIUStates(HashSet<string> stringsDaSalvare, string connectionString)
        {
                            List<string> listaDaSalvare = stringsDaSalvare.ToList();
                var duplicates = listaDaSalvare.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                             .Select(g => g.Key)
                                             .ToList();

                if (duplicates.Any())
                {
                    Console.WriteLine("Trovati duplicati nella lista da salvare:");
                    foreach (var dup in duplicates)
                    {
                        Console.WriteLine(dup);
                    }
                }

            using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
            {

                dbConnection.Open();
                using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
                {
                    MIUStringData data = null;
                    try
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO MIU_States (CurrentString, StringLength, Hash, DiscoveryTime_Int, DiscoveryTime_Text) VALUES (@CurrentString, @StringLength, @Hash, @DiscoveryTime_Int, @DiscoveryTime_Text);", dbConnection, transaction)) 
                            {
                                SQLiteParameter param = new SQLiteParameter("@CurrentString", System.Data.DbType.String);
                                SQLiteParameter param1 = new SQLiteParameter("@StringLength", System.Data.DbType.Int32);
                                SQLiteParameter param2 = new SQLiteParameter("@Hash", System.Data.DbType.String);
                                SQLiteParameter param3 = new SQLiteParameter("@DiscoveryTime_Int", System.Data.DbType.Int64);
                                SQLiteParameter param4 = new SQLiteParameter("@DiscoveryTime_Text", System.Data.DbType.String);
                                insertCommand.Parameters.Add(param);
                                insertCommand.Parameters.Add(param1);
                                insertCommand.Parameters.Add(param2);
                                insertCommand.Parameters.Add(param3);
                                insertCommand.Parameters.Add(param4);

                            foreach (string str in stringsDaSalvare) 
                            {
                                XElement miuStringDef = XElement.Parse(str);
                                data = new MIUStringData()
                                {
                                    CurrentString = miuStringDef.Element("CurrentString")?.Value.Trim(),
                                    CurrentStringLen = int.Parse(miuStringDef.Element("CurrentStringLen")?.Value ?? "0"),
                                    DiscoveryTime_Int = long.Parse(miuStringDef.Element("DiscoveryTime_Int")?.Value ?? "0"),
                                    DiscoveryTime_Text = miuStringDef.Element("DiscoveryTime_Text")?.Value,
                                    Hash = miuStringDef.Element("Hash")?.Value
                                };
                                Console.WriteLine($"{dbload.Value} {data.CurrentString}");
                                insertCommand.Parameters["@CurrentString"].Value = data.CurrentString;
                                insertCommand.Parameters["@StringLength"].Value = data.CurrentStringLen;
                                insertCommand.Parameters["@Hash"].Value = data.Hash;
                                insertCommand.Parameters["@DiscoveryTime_Int"].Value = data.DiscoveryTime_Int;
                                insertCommand.Parameters["@DiscoveryTime_Text"].Value = data.DiscoveryTime_Text;


                                insertCommand.ExecuteNonQuery();
                                dbload.Value += 1;
                            }
                            transaction.Commit();
                        }
                            //Console.WriteLine($"Sono state salvate {stringsDaSalvare.Count} stringhe nella tabella MIU_States (in transazione).");
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        //Console.WriteLine($"Errore durante il salvataggio nel database (transazione annullata): {ex.Message}");
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                }
            }
        }
    }
}
