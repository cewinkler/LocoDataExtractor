﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace LocoDataExtractor
{
    public partial class LocoExtractor : Form
    {

        public int Frequency = 1;
        public string FilePath = "";
        public string ChosenFile = "";
        public LocoExtractor()
        {
            InitializeComponent();
            fileSelect.InitialDirectory = "C:\\Files\\EncData\\";
            BinSize.Text = @"10";
            SampleFrequency.Text = Frequency.ToString(CultureInfo.InvariantCulture);
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            FilePath = "";
            fileSelect.ShowDialog();
        }

        private void fileSelect_FileOk(object sender, CancelEventArgs e)
        {
            foreach (var file in fileSelect.FileNames)
            {
                if (String.IsNullOrEmpty(FilePath)) FilePath = Path.GetDirectoryName(file);
                if (!String.IsNullOrEmpty(file)) FileList.Items.Add(Path.GetFileName(file));
            }
            
        }

        private void btExtract_Click(object sender, EventArgs e)
        {
            int bin;
            Int32.TryParse(BinSize.Text, out bin);
            var bins = new[] { bin, bin, bin, bin, bin }; // leaving this legacy code in case decision is made to have unique bin sizes.
            Frequency = 0;
            Int32.TryParse(SampleFrequency.Text, out Frequency);
            foreach (var binsize in bins)
            {
                if (binsize <= 0)
                {
                    AddText("ERROR: One of the supplied 'Bin Sizes' is invalid (less than or equal to zero). Please fix and try again.");
                    return;
                }
                if (Frequency <= 0)
                {
                    AddText("ERROR: 'Sample Frequency' is invalid (less than or equal to zero). Please fix and try again.");
                    return;
                }
            }
            try
            {
                //foreach (var lr in from string line in fileSelect.FileNames select new LocoReader(line, Frequency))
                if (ChosenFile.Length == 0)
                {
                    AddText("Please select a file from the Selected File(s) list box.");
                    return;
                }

                var lrGen = new LocoReader(FilePath + "\\" + ChosenFile, Frequency);
                var lrName = lrGen.GenerateFixedFile();
                var lr = new LocoReader(lrName);
                AddText("Save location: " + Path.GetDirectoryName(lrName));
                lr.ImmobileTime(bins[0]);
                AddText("'Immobile Time' saved to: " + Path.GetFileName(lr.Measurer.OutputFile));
                lr.HorizontalMovement(bins[2]);
                AddText("'Horizontal Movement' saved to: " + Path.GetFileName(lr.Measurer.OutputFile));
                lr.VerticalMovement(bins[1]);
                AddText("'Vertical Movement' saved to: " + Path.GetFileName(lr.Measurer.OutputFile));
                lr.CenterVertical(bins[4]);
                AddText("'Central-Vertical Movement' saved to: " + Path.GetFileName(lr.Measurer.OutputFile));
                lr.VerticalTime(bins[3]);
                AddText("'Vertical Time' saved to: " + Path.GetFileName(lr.Measurer.OutputFile));
                if (settingsDrug.Text.Length > 0 && settingsDrug.Text.Length > 0 && settingsRatID.Text.Length > 0)
                {
                    if (lr.GenRData(settingsRatID.Text, settingSessNo.Text, settingsDrug.Text))
                        AddText("'R Data' has been saved to: " + Path.GetFileName(lr.OutputFile));
                    else
                        AddText(
                            "'R Data' has not been generated: The number of bins is not equal across all constructs.");
                }
                AddText("Data has been successfully extracted.");
                File.Delete(lrName);

            }
            catch (Exception err)
            {
                AddText("[LocoReader] ERROR: " + err.Message);
            }
        }

        private void AddText(string text)
        {
            console.Invoke((MethodInvoker)delegate
            {
                console.AppendText(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + text + "\n");
                console.ScrollToCaret();
            });
        }

        private void btRepair_Click(object sender, EventArgs e)
        {
            foreach (string line in fileSelect.FileNames)
            {
                var lr = new LocoReader(line, Frequency);
                lr.GenerateFixedFile(false);
            }
        }

        private void lbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChosenFile = FileList.Text;
            textBox1.Text = ChosenFile;
        }
    }
}
