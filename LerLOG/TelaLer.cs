using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
// ===============================
// AUTHOR       : JEFFERSON BRANDÃO DA COSTA - ANALISTA/PROGRAMADOR
// CREATE DATE  : 14/05/2018
// DESCRIPTION  : Sistema para ler pasta LOG das estaçoes  ASSY, FAT, TPT modelo E331
// SPECIAL NOTES:
// ===============================
// Change History: version 1.0.0.2
// Date:11/06/18 Correção para não travar programa interface qdo programa lerLog acessar arquivo [Abrir arquivo com permissao somente leitura]  
//==================================

namespace LerLOG
{
    public partial class TelaLerLog : Form
    {
        int statusConexao = 0;
        int segundos = 0;
        int tempo = 0;

        public TelaLerLog()
        {
            InitializeComponent();
            //  
            tempo = GetTempo();
            //
            toolTip1.SetToolTip(btnRefresh, "Ligar");
            toolTip1.SetToolTip(btParar, "Parar");
            //
            #region RODAPÉ

            int anoCriacao = 2018;
            int anoAtual = DateTime.Now.Year;
            string texto = anoCriacao == anoAtual ? " Foxconn CNSBG All Rights Reserved." : "-" + anoAtual + " Foxconn CNSBG All Rights Reserved.";
            //
            lbRodape.Text = "Copyright © " + anoCriacao + texto;

            #endregion
            //
            Process aProcess = Process.GetCurrentProcess();
            string aProcName = aProcess.ProcessName;

            if (Process.GetProcessesByName(aProcName).Length > 1)
            {
                MessageBox.Show("O programa já está em execução!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //
            timerCount.Start();
            statusConexao = 1;
        }

        public void GetArquivo()
        {
            #region LOG txt

            string nomeArquivo = DateTime.Now.Date.ToString("yyyy-M-dd") + ".txt";
            string caminho = GetCaminho();
            string arquivo = caminho + nomeArquivo;
            string linha;
            string log = string.Empty;
            string nomeColuna = string.Empty;
            //
            gridLog.Rows.Clear();

            nomeColuna = caminho.Replace(@"\", string.Empty).Replace("EFAPWEB-SERVER", string.Empty).Trim(); //\\EFAPWEB-SERVER\LogFAT\
            nomeColuna = string.IsNullOrEmpty(nomeColuna) ? "LOG" : nomeColuna;
            //
            if (gridLog.ColumnCount == 0)//Para refresh não duplicar coluna
            {
                gridLog.Columns.Add(nomeColuna, nomeColuna);//CRIA COLUNA
            }
            //
            if (System.IO.File.Exists(arquivo))
            {
                #region CONFIGURAR ARQUIVO PARA SOMENTE LEITURA

                //FileStream d = new FileStream(arquivo.Trim(), FileMode.OpenOrCreate, FileAccess.Write);
                //StreamWriter c = new StreamWriter(d);
                //c.Write(false);//abrir arquivo permissao somente leitura para não travar programa INTERFACE
                //c.Close();

                FileStream d = new FileStream(arquivo.Trim(), FileMode.OpenOrCreate, FileAccess.Read);

               

                #endregion

                System.IO.StreamReader arqTXT = new System.IO.StreamReader(arquivo);

                //gridLog.Columns.Add("LOG", "LOG");//CRIA COLUNA
                while ((linha = arqTXT.ReadLine()) != null)
                {
                    if (!linha.Contains("File not found"))
                        gridLog.Rows.Add(linha);
                }

                d.Flush();
                d.Close();

                //Ordem decrescente
                gridLog.Sort(gridLog.Columns[0], ListSortDirection.Descending);
                gridLog.Columns[0].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Descending;
                //
                gridLog.Rows[0].Selected = true;//deixar selecionado o item mais recente da lista
                //
                arqTXT.Close();
                //
                lbAviso.Visible = false;
            }
            else
            {
                lbAviso.Visible = true;
                lbAviso.Text = "Arquivo ou caminho não existe " + arquivo;
            }
            //
            if (gridLog.Rows.Count > 0)
            {
                for (int rows = 0; rows < gridLog.Rows.Count; rows++)
                {
                    //aumenta tamanho das linhas
                    DataGridViewRow row = gridLog.Rows[rows];
                    row.Height = 50;

                    //deixa as linhas em negrito
                    DataGridViewCellStyle style = gridLog.RowsDefaultCellStyle;
                    style.Font = new Font(gridLog.Font, FontStyle.Bold);

                    if (rows > 0)//nao permite deixar selecinado nenhum item que nao seja o mais recente da lista
                        gridLog.Rows[rows].Selected = false;
                }
            }

            #endregion
        }

        public string GetCaminho()
        {
            #region CAMINHO txt

            string arquivo = AppDomain.CurrentDomain.BaseDirectory + @"\CONFIG\CAMINHO.txt";
            string linha;
            string caminho = string.Empty;
            //
            if (System.IO.File.Exists(arquivo))
            {
                System.IO.StreamReader arqTXT = new System.IO.StreamReader(arquivo);
                //
                while ((linha = arqTXT.ReadLine()) != null)
                {
                    for (int index = 0; index < linha.Length; index++)
                    {
                        caminho += linha[index];
                    }
                }
                //
                arqTXT.Close();
            }
            else
            {
                caminho = string.Empty;
            }
            //
            return caminho.Trim();

            #endregion
        }

        public int GetTempo()
        {
            #region SEGUNDOS txt

            string arquivo = AppDomain.CurrentDomain.BaseDirectory + @"\CONFIG\TEMPO.txt";
            string linha;
            int tempo = 0;
            string aux = string.Empty;
            //
            if (System.IO.File.Exists(arquivo))
            {
                System.IO.StreamReader arqTXT = new System.IO.StreamReader(arquivo);
                //
                while ((linha = arqTXT.ReadLine()) != null)
                {
                    for (int index = 0; index < linha.Length; index++)
                    {
                        aux += linha[index];
                    }
                }
                //
                arqTXT.Close();
            }

            tempo = string.IsNullOrEmpty(aux) ? tempo : int.Parse(aux.Trim());
            //
            return tempo;

            #endregion
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            lbAviso.Visible = false;
            timerCount.Start();
            statusConexao = 1;
        }

        private void timerCount_Tick(object sender, EventArgs e)
        {
            timerCount.Stop();
            timerCount.Start();
            //
            statusConexao = 1;
            //
            if (statusConexao == 1)
            {
                lbtime.Text = segundos.ToString() + ":s";
                segundos = segundos + 1;
                //
                if (segundos == tempo)//5 segundos
                {
                    timerCount.Stop();
                    timerCount.Start();
                    //
                    GetArquivo();
                    //
                    segundos = 0;
                }
            }
            else
            {
                lbAviso.Text = "PARADO";
                lbAviso.Visible = true;
                lbAviso.ForeColor = System.Drawing.Color.Red;
                statusConexao = 0;
                segundos = 0;
                lbtime.Text = "0:s";
            }
        }

        private void btParar_Click(object sender, EventArgs e)
        {
            lbAviso.Text = "PARADO";
            lbAviso.Visible = true;
            lbAviso.ForeColor = System.Drawing.Color.Red;
            timerCount.Stop();
            //
            lbtime.Text = "0:s";
            segundos = 0;
        }

    }
}
