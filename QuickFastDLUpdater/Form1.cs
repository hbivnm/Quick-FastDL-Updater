﻿using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFastDLUpdater
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        // _Click
        private void btnBrowseServer_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBoxServerpath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBrowseFastDL_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBoxFastDLpath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnPreCheck_Click(object sender, EventArgs e)
        {
            if (!sanitycheck())
                return;

            string fullMapPrefix = textBoxPrefix.Text;
            string[] prefixArr = null;
            if (string.IsNullOrWhiteSpace(textBoxPrefix.Text))
            {
                DialogResult result = MessageBox.Show("No map prefix was given (all maps will be counted.)\n\nWould you like to cancel and add a map prefix?", "No map prefix given", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes || result == DialogResult.Cancel)
                    return;
                else
                    fullMapPrefix = null;
            }

            if (fullMapPrefix != null)
                prefixArr = fullMapPrefix.Split('/');

            DirectoryInfo di = new DirectoryInfo(textBoxServerpath.Text + @"\csgo\maps");
            FileInfo[] filesArr = di.GetFiles("*.bsp");

            // .\csgo\maps
            int mapCount = 0;

            if (fullMapPrefix == null)
                mapCount = filesArr.Length;
            else if (prefixArr != null)
                foreach (FileInfo file in filesArr)
                    for (int i = 0; i < prefixArr.Length; i++)
                        if (file.Name.StartsWith(prefixArr[i]))
                            mapCount++;

            if (fullMapPrefix == null)
                MessageBox.Show("Server path: OK\nFastDL path: VALID\n\nMap count (*.bsp): " + mapCount, "Pre-check scan complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                string msg = "Server path: OK\nFastDL path: VALID\n\nMap count (*.bsp): " + mapCount + "\nPrefix(es): ";
                foreach (string prefix in prefixArr)
                    msg += prefix + ", ";

                MessageBox.Show(msg, "Pre-check scan complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            setStatusText("Starting...");
            if (!sanitycheck())
                return;

            setStatusText("Reading prefix(es)...");
            string fullMapPrefix = textBoxPrefix.Text;
            string[] prefixArr = null;
            if (string.IsNullOrWhiteSpace(textBoxPrefix.Text))
            {
                DialogResult result = MessageBox.Show("No map prefix was given (all maps will be counted.)\n\nWould you like to cancel and add a map prefix?", "No map prefix given", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes || result == DialogResult.Cancel)
                    return;
                else
                    fullMapPrefix = null;
            }

            if (fullMapPrefix != null)
                prefixArr = fullMapPrefix.Split('/');

            setStatusText("Getting .bsp files from /csgo/maps ...");
            DirectoryInfo di = new DirectoryInfo(textBoxServerpath.Text + @"\csgo\maps");
            FileInfo[] filesArr = di.GetFiles("*.bsp");
            
            if (fullMapPrefix == null) // Compress all .bsp files
            {
                foreach (FileInfo file in filesArr) // file: the file that is going to be compressed
                {
                    FileInfo compressedFile = new FileInfo(textBoxFastDLpath.Text + @"\maps\" + file.Name + ".bz2"); // compressedFile: Output, compressed file
                    using (FileStream fileStream = file.OpenRead()) 
                    {
                        using (FileStream compressedFileStream = compressedFile.Create())
                        {
                            try
                            {
                                BZip2.Compress(fileStream, compressedFileStream, true, 4096);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Failed @ BZip2.Compress(...)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                }
            }
            else if (prefixArr != null) // Compress .bsp files matching prefix
                foreach (FileInfo file in filesArr)
                    for (int i = 0; i < prefixArr.Length; i++)
                        if (file.Name.StartsWith(prefixArr[i]))
                        {
                            setStatusText("Compressing " + file.Name + "...");
                            FileInfo compressedFile = new FileInfo(textBoxFastDLpath.Text + @"\maps\" + file.Name + ".bz2"); // compressedFile: Output, compressed file
                            using (FileStream fileStream = file.OpenRead())
                            {
                                using (FileStream compressedFileStream = compressedFile.Create())
                                {
                                    try
                                    {
                                        BZip2.Compress(fileStream, compressedFileStream, true, 4096);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Failed @ BZip2.Compress(...)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                        }

            setStatusText("Ready");
        }

        private void linkLabelSteam_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://steamcommunity.com/profiles/76561197996468677");
        }

        private void linkLabelGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/HybridVenom");
        }

        // Funcs
        private bool sanitycheck()
        {
            setStatusText("Sanity check started...");
            if (string.IsNullOrWhiteSpace(textBoxServerpath.Text)) // Check if server textbox has text
            {
                MessageBox.Show("Server path: GIVEN\n\nNo server path was given.", "No server path given", MessageBoxButtons.OK, MessageBoxIcon.Error);
                setStatusText("Sanity check: FAILED");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(textBoxFastDLpath.Text)) // Check if fastdl textbox has text
            {
                MessageBox.Show("Server path: GIVEN\nFastDL path: NOT GIVEN\n\nNo path to FastDL was given.", "No FastDL path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                setStatusText("Sanity check: FAILED");
                return false;
            }

            bool srcdsExists = false; // Assume false
            FileInfo[] serverFiles = new DirectoryInfo(textBoxServerpath.Text).GetFiles();

            foreach (FileInfo file in serverFiles)
                if (file.Name == "srcds.exe")
                {
                    srcdsExists = true;
                    break;
                }

            if (!srcdsExists)
            {
                MessageBox.Show("Server path: INVALID\n\nPlease check that you entered the correct server path.\n(Same folder as 'srcds.exe')", "Invalid server path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                setStatusText("Sanity check: FAILED");
                return false;
            }

            if (!Directory.Exists(textBoxFastDLpath.Text))
            {
                MessageBox.Show("Server path: OK\nFastDL path: INVALID\n\nGiven FastDL path does not exist.", "FastDL path does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                setStatusText("Sanity check: FAILED");
                return false;
            }

            if (!Directory.Exists(textBoxFastDLpath.Text + @"\maps"))
            {
                MessageBox.Show("Server path: OK\nFastDL path: INVALID\n\nGiven FastDL path is missing a \"maps\" folder.", "FastDL path missing \"maps\" folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                setStatusText("Sanity check: FAILED");
                return false;
            }

            setStatusText("Sanity check: OK. Ready");
            return true;
        }

        private void setStatusText(string text)
        {
            labelStatusText.Text = text;
        }
    }
}
