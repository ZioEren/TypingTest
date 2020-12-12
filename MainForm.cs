using System.Windows.Forms;
using System.Threading;
using System;
using System.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
public partial class MainForm : Form
{
    [DllImport("psapi.dll")]
    static extern int EmptyWorkingSet(IntPtr hwProc);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessWorkingSetSize(IntPtr process, UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);
    public static bool canStartTest = true, testStarted = false;
    public static List<string> words = new List<string>();
    public static Random rand = new Random();
    public static int actualCPM, seconds, errors, correctWords;
    public MainForm()
    {
        InitializeComponent();
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        CheckForIllegalCrossThreadCalls = false;
        new Thread(clearRam).Start();
        new Thread(textCorrector).Start();
        foreach (string word in System.IO.File.ReadAllLines("words.txt"))
        {
            words.Add(word);
        }
        refreshWords();
    }
    public void textCorrector()
    {
        while (true)
        {
            Thread.Sleep(10);
            if (textBox1.Text.StartsWith(" "))
            {
                textBox1.Text = textBox1.Text.Substring(1, textBox1.Text.Length - 1);
            }
        }
    }
    public void removePossible()
    {
        Thread.Sleep(2000);
        canStartTest = true;
    }
    public void clearRam()
    {
        while (true)
        {
            Thread.Sleep(1000);
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
        }
    }
    public void typingTimer()
    {
        while (true)
        {
            Thread.Sleep(1000);
            if (!testStarted)
            {
                return;
            }
            if (seconds != 61)
            {
                seconds++;
                label1.Text = "Time remaining: " + (61 - seconds).ToString() + " seconds.";
            }
            else
            {
                button1.PerformClick();
                return;
            }
        }
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Process.GetCurrentProcess().Kill();
    }
    private void textBox1_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            string currentWord = "";
            foreach (char c in richTextBox1.Text.ToCharArray())
            {
                if (c.ToString() == " ")
                {
                    break;
                }
                else
                {
                    currentWord += c.ToString();
                }
            }
            richTextBox1.Text = richTextBox1.Text.Substring(currentWord.Length + 1, richTextBox1.Text.Length - (currentWord.Length + 1));
            if (textBox1.Text == currentWord)
            {
                actualCPM += currentWord.Length;
                correctWords++;
            }
            else
            {
                errors++;
            }
            textBox1.Text = "";
            textBox1.SelectionStart = 0;
        }
        else
        {
            if (!testStarted && canStartTest)
            {
                button1.PerformClick();
            }
        }
    }
    private void button1_Click(object sender, EventArgs e)
    {
        if (canStartTest)
        {
            if (button1.Text.StartsWith("Sta"))
            {
                testStarted = true;
                actualCPM = 0;
                errors = 0;
                seconds = 0;
                correctWords = 0;
                button1.Text = "Stop Typing Test";
                label2.Text = "Write some words.";
                new Thread(typingTimer).Start();

            }
            else
            {
                testStarted = false;
                canStartTest = false;
                button1.Text = "Start Typing Test";
                label2.Text = "You scored " + actualCPM.ToString() + " CPM (" + (actualCPM / 5).ToString() + " WPM) with " + errors.ToString() + " errors and " + correctWords.ToString() + " correct words.";
                label1.Text = "Time remaining: 0 seconds.";
                new Thread(removePossible).Start();
                refreshWords();            
            }
        }
    }
    public void refreshWords()
    {
        richTextBox1.Text = "";
        for (int i = 0; i < 300; i++)
        {
            if (richTextBox1.Text == "")
            {
                richTextBox1.Text = words[rand.Next(0, words.Count)];
            }
            else
            {
                richTextBox1.Text += " " + words[rand.Next(0, words.Count)];
            }
        }
    }
}