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

        }

        public void LoadFileList()
        {
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
                    checkedListBox1.Items.Add(Path.GetFileName(file));
                }
            }
            button1.Enabled = true;
        }

        public void CSVParse()
        {
            //
            if (csvfiles.Count > 0)
                foreach (string csv in csvfiles)
                {
                    //
                    string[] csvtext = File.ReadAllLines(csv);
                    row[] rows = new row[csvtext.Length];
                    int j = 0;
                    int tmpi = 0;
                    double tmpj = 1;
                    //fill array from parsed file
                    for (int i = 18; i < csvtext.Length; i++)
                    {
                        // string strrow = csvtext[i].Split(";");
                        String pattern = @"[/;]";
                        String[] elements = Regex.Split(csvtext[i], pattern);
                        // 0 - стержень 1 - сочетание 13 - минимум по мезусу 14 - максимум
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
                        j++;
                    }
                    //search max
                    //LINQ
                    //var query = from row tmprow in rows where tmprow.rod == 7 select tmprow; 
                    //double max =rows.Max(a => a.maxstress);
                    /*
                     if (ccrod != tmpi)
                            {
                                if (j > 0)
                                {
                                    maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString()+";"+ rows[maxind].maxstress.ToString()); 
                                }
                                ccrod = tmpi;
                                maxvalue = 0;
                                firstmax = true;
                            }
                            //
                             if(maxvalue < tmpj)
                            {
                                maxvalue = tmpj;
                                maxind = j;
                            } else
                                if(firstmax == true)
                                maxind = j;
                     */
                    int ccrod = rows[0].rod;
                    double maxvalue = rows[0].maxstress;
                    int maxind = 0;
                    //bool firstmax = false;
                    maxlist = new List<string>();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        if (rows[i] != null)
                        {
                            //debug
                            if (rows[i].comb == 333)
                            {
                                row d = rows[i];
                            }
                            if (ccrod != rows[i].rod)
                            {
                                maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString() + ";" + rows[maxind].maxstress.ToString());
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
                    maxlist.Add(rows[maxind].rod.ToString() + ";" + rows[maxind].comb.ToString() + ";" + rows[maxind].minstress.ToString() + ";" + rows[maxind].maxstress.ToString());

                    //save results
                    File.WriteAllLines(Path.GetFileName(csv)+"_result.txt", maxlist);

                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            LoadFileList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CSVParse();
        }
        
    }
}
