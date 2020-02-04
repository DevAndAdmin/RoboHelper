using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
//using Microsoft.Office.Interop.Excel;


namespace robo_parser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //string[] csvfiles;
        List<string> csvfiles;
        List<string> maxlist;

        //
        public class row
        {
            public int rod;
            public int comb;
            public double minstress;
            public double maxstress;
            public string data;
        }

        public void LoadFileList()
        {
            DataGridViewRow rr;
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.StartupPath);
            label2.Text = Application.StartupPath;
            string[] allfiles = Directory.GetFiles(Application.StartupPath);
            csvfiles = new List<string>();
            foreach (string file in allfiles)
            {
                if (Path.GetExtension(file) == ".csv")
                {
                    //csvfiles.Add(new Bitmap(file.FullName)); //если расширение подошло, создаём Bitmap
                    csvfiles.Add(file);
                    //listBox1.Items.Add(Path.GetFileName(file));
                    int rowNumber = dataGridView1.Rows.Add();
                    dataGridView1.Rows[rowNumber].Cells[0].Value = true;
                    dataGridView1.Rows[rowNumber].Cells[1].Value = Path.GetFileName(file);
                    dataGridView1.Rows[rowNumber].Cells[2].Value = 0;
                    dataGridView1.Rows[rowNumber].Cells[3].Value = false;
                    dataGridView1.Rows[rowNumber].Cells[4].Value = "";
                }
            }
            button1.Enabled = true;
        }

        public async void CSVParse()
        {
            //
            button1.Enabled = false;
            string[] csvtext = {""};
            int listid = 0;
            string fname = "";
            if (csvfiles.Count > 0)
                foreach (string csv in csvfiles)
                {
                    bool expl = false;
                    bool nocombi = true;
                    int cmbmin = 0;
                    int cmbmax = 0;
                    fname = Path.GetFileName(csv);
                    //listid = listBox1.FindString(fname);
                    // datagrid
                    foreach (DataGridViewRow rrow in dataGridView1.Rows)
                    {
                        if (rrow.Cells[1].Value.ToString().Equals(fname))
                        {
                            listid = rrow.Index;
                            break;
                        }
                    }
                    dataGridView1.Rows[listid].Cells[4].Value = "working...";

                    //listBox1.Items[listid] = fname + "   - working...";
                    try
                    {
                        await Task.Run(() =>
                        {
                            csvtext = System.IO.File.ReadAllLines(csv);
                        });
                    }
                    catch
                    {
                        dataGridView1.Rows[listid].Cells[4].Value = "read file error.";
                        //listBox1.Items[listid] = fname + "   - read file error.";
                    }
                    if (csvtext.Length > 0)
                    {
                        row[] rows = new row[csvtext.Length];
                        int j = 0;
                        int tmpi = 0;
                        double tmpj = 1;
                        //fill array from parsed file
                        await Task.Run(() =>
                        {
                            for (int i = 18; i < csvtext.Length; i++)
                            {
                                String pattern = @"[/;]";
                                String[] elements = Regex.Split(csvtext[i], pattern);     // 0 - стержень 1 - сочетание 12 - минимум по мезусу 13 - максимум 16 - data
                                rows[j] = new row();
                                if (int.TryParse(elements[0], out tmpi))
                                    rows[j].rod = tmpi;
                                else
                                    rows[j].rod = 0;
                                if (int.TryParse(elements[1], out tmpi))
                                    rows[j].comb = tmpi;
                                else
                                    rows[j].comb = 0;
                                if (double.TryParse(elements[12], out tmpj))
                                    rows[j].minstress = tmpj;
                                else
                                    rows[j].minstress = 0;
                                if (double.TryParse(elements[13], out tmpj))
                                    rows[j].maxstress = tmpj;
                                else
                                    rows[j].maxstress = 0;
                                if (elements[16] != null)
                                    rows[j].data = elements[16];
                                else
                                    rows[j].data = "";
                                j++;
                            }
                        });
                        //search max
                        //LINQ
                        //var query = from row tmprow in rows where tmprow.rod == 7 select tmprow; 
                        //double max =rows.Max(a => a.maxstress);

                        //chek combi
                        if (dataGridView1.Rows[listid].Cells[2].Value.ToString().Contains('-'))
                        {
                            try
                            {
                                string[] strrange = dataGridView1.Rows[listid].Cells[2].Value.ToString().Split('-');
                                int.TryParse(strrange[0].ToString(), out cmbmin);
                                int.TryParse(strrange[1].ToString(), out cmbmax);
                                nocombi = false;
                            }
                            catch { }
                        }
                        else
                        {
                            nocombi = true;
                        }
                        
                        //chek explosion
                        expl = (bool)dataGridView1.Rows[listid].Cells[3].Value;

                        bool noresult = true;
                        int ccrod = rows[0].rod;
                        double maxvalue = rows[0].maxstress;
                        int maxind = 0;
                        maxlist = new List<string>();
                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (rows[i] != null)
                            {
                                if (nocombi == true )
                                {
                                    if (ccrod != rows[i].rod)
                                    {
                                        maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString() + ";" + rows[maxind].maxstress.ToString() + ";" + rows[maxind].data);
                                        ccrod = rows[i].rod;
                                        maxvalue = rows[i].maxstress;
                                        maxind = i;
                                    }
                                    if (maxvalue < rows[i].maxstress)
                                    {
                                        maxvalue = rows[i].maxstress;
                                        maxind = i;
                                    }
                                }
                                else
                                {
                                    if (rows[i].comb >= cmbmin & rows[i].comb <= cmbmax)
                                    {
                                        noresult = false;
                                        if (ccrod != rows[i].rod)
                                        {
                                            maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString() + ";" + rows[maxind].maxstress.ToString() + ";" + rows[maxind].data);
                                            ccrod = rows[i].rod;
                                            maxvalue = rows[i].maxstress;
                                            maxind = i;
                                        }
                                        if (maxvalue < rows[i].maxstress)
                                        {
                                            maxvalue = rows[i].maxstress;
                                            maxind = i;
                                        }
                                    }
                                }

                            }
                        }
                        if(nocombi == true | noresult == false)
                            maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString() + ";" + rows[maxind].maxstress.ToString() + ";" + rows[maxind].data);

                        //save results
                        if (maxlist.Count > 0)
                        {
                            dataGridView1.Rows[listid].Cells[4].Value = "save.";
                            //listBox1.Items[listid] = fname + "   - save.";
                            try
                            {
                                await Task.Run(() =>
                                {
                                    System.IO.File.WriteAllLines(Path.GetFileName(csv) + "_result.txt", maxlist);
                                });
                                //excel save
                                /*
                                 // Загрузить Excel, затем создать новую пустую рабочую книгу
                Excel.Application excelApp = new Excel.Application();

                // Сделать приложение Excel видимым
                excelApp.Visible = true;
                excelApp.Workbooks.Add();
                Excel._Worksheet workSheet = excelApp.ActiveSheet;
                // Установить заголовки столбцов в ячейках
                workSheet.Cells[1, "A"] = "NameCompany";
                workSheet.Cells[1, "B"] = "Site";
                workSheet.Cells[1, "C"] = "Cost";
                int row = 1;
                foreach (Price c in vPices)
                {
                    row++;
                    workSheet.Cells[row, "A"] = c.Name;
                    workSheet.Cells[row, "B"] = c.Site;
                    workSheet.Cells[row, "C"] = c.Cost;
                }
                   // Сохранить файл, выйти из Excel

                    // убрать предупреждения!!! нужно для перезаписи
                    excelApp.DisplayAlerts = false;
                    workSheet.SaveAs(string.Format(@"{0}\Price.xlsx", Environment.CurrentDirectory));

                    excelApp.Quit();
                                 */
                            }
                            catch
                            {
                                dataGridView1.Rows[listid].Cells[4].Value = "save result file error.";
                                //listBox1.Items[listid] = fname + "   - save result file error.";
                            }
                        }
                        else
                        {
                            dataGridView1.Rows[listid].Cells[4].Value = "No result.";
                        }
                    }
                }
            button1.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            LoadFileList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            CSVParse();
        }
        
    }
}
