using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace filewatcher_with_copy_net_core_5_plus
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            savedGamesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StackOverflow",
                "filewatcher_with_copy"
            );
            // Start with clean slate for test
            Directory.Delete(savedGamesPath, recursive: true);
            // ===============================

            Directory.CreateDirectory(savedGamesPath);
            _fileSystemWatcher = new FileSystemWatcher()
            {
                Path = savedGamesPath,
                IncludeSubdirectories = false,
                SynchronizingObject = this, // But it seems to complain if not Invoked "anyway"
            };
            _fileSystemWatcher.Created += onCreated;
            _fileSystemWatcher.Changed += onChanged;
            _fileSystemWatcher.Deleted += onDeleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
        FileSystemWatcher _fileSystemWatcher;
        private readonly string savedGamesPath;

        private void onCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                var fiCopy = new FileInfo(e.FullPath);
                richTextBox1.AppendText($"Created Directory: {e.FullPath}", Color.LightCoral, newLine: false);
                richTextBox1.AppendText($" On: {fiCopy.CreationTime}", Color.Yellow);
                return; // this is a directory, not a file.
            }
            Debug.Assert(Path.GetDirectoryName(e.FullPath).Equals(savedGamesPath), "Expecting none other");

            var fiSrce = new FileInfo(e.FullPath);

            string folderName = Path.Combine(
                savedGamesPath,
                $"Save Game {fiSrce.CreationTime.ToString("dddd, dd MMMM yyyy")}");
            // Harmless if already exists
            Directory.CreateDirectory(folderName);

            string destFile = Path.Combine(folderName, e.Name);

            File.Copy(e.FullPath, destFile);
            Debug.Assert(
                fiSrce.CreationTime.Equals(fiSrce.LastWriteTime),
                "Expecting matching CreationTime"
            );

            var fiDest = new FileInfo(destFile);
            Debug.Assert(
                !fiSrce.CreationTime.Equals(fiDest.CreationTime),
                "Expecting different CreationTime"
            );
            fiDest.CreationTime = fiSrce.CreationTime;
            fiDest.LastWriteTime = fiSrce.LastWriteTime;
            Debug.Assert(
                fiSrce.CreationTime.Equals(fiDest.CreationTime),
                "Expecting matching CreationTime"
            );

            richTextBox1.AppendText($"Created: {e.FullPath}", Color.GreenYellow, newLine: false);
            richTextBox1.AppendText($" On: {fiSrce.CreationTime}", Color.Yellow);
        }

        private void onChanged(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                return; // this is a directory, not a file.
            }
            richTextBox1.AppendText($"Changed: {e.FullPath}{Environment.NewLine}", Color.LightCoral);
        }

        private void onDeleted(object sender, FileSystemEventArgs e)
        {
            richTextBox1.AppendText($"Deleted: {e.FullPath}{Environment.NewLine}", Color.LightSalmon);
        }


        // In MainForm.Designer.cs
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                _fileSystemWatcher.Dispose();
            }
            base.Dispose(disposing);
        }

        int _gameCount = 0;
        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            _gameCount++;
            var gamePrimary = Path.Combine(savedGamesPath, $"Game{_gameCount}.game");
            File.WriteAllText(gamePrimary, String.Empty);
        }
    }
    static class Extensions
    {
        public static void AppendText(this RichTextBox richTextBox, string text, Color color, bool newLine = true)
        {
            var colorB4 = richTextBox.SelectionColor;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = colorB4;
            if (newLine) richTextBox.AppendText(Environment.NewLine);
        }
    }
}
