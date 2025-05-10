using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MasterLog;


namespace Test
{
    public partial class Form1 : Form
    {
        string pathStart = @"C:\temp\log\";
        string fileStart = "ProvaLogFile";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            txtPath.Text = pathStart;
            txtFileName.Text = fileStart;
            this.Refresh();

            Logger log = new Logger(txtPath.Text, txtFileName.Text, 10);
            lblReturnedPath.Text = log.GetPercorsoCompleto();
            lblReturnedFileName.Text = log.GetNomeEseguibile();
            for (int i = 1; i < 11; i++)
            {
                log.Log(LogLevel.ERROR, "ciclo numero: " + i.ToString());
            }

            //log.DeleteOldFile(nomeFiltro, 2);
            //log.Write("Seconda riga");
            //log.Write("Vediamo ora cosa cavolo succede se metto una riga più lunga delle altre e magari caratteri strani come *;!%");
            //txtBoxRead.Text = log.Read();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //string[] fileList = System.IO.Directory.GetFiles(pathStart, nomeFiltro + "*", System.IO.SearchOption.TopDirectoryOnly);

            //for (int i = 0; i < fileList.Length; i++)
            //{
            //    System.IO.File.SetCreationTimeUtc(fileList[i], new DateTime(2015, 02, i + 1));
            //}
        }

    }
}