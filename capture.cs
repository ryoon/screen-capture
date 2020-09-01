/*
 * The simplest active window screen capture program
 * Copyright (c) 2016, 2020 Ryo ONODERA All rights reserved.
 * Author: Ryo ONODERA <ryo@tetera.org>
 * License: MIT
 */

using System;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices; /* for GetAsyncKeyState */
using System.Drawing.Imaging;

class ScreenCapture : Form
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(System.Int32 vKey);
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modKey, int key);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
    private static extern ushort GlobalAddAtom(string lpString);

    private const int MOD_CTRL = 0x02;
    private const int MOD_SHIFT = 0x04;
    private const int WM_HOTKEY = 0x312;

    private const int VK_CONTROL = 0x11;
    private const int VK_SHIFT = 0x10;
    private const int VK_SNAPSHOT = 0x2c;

    private UInt32 serialnum = 1;
    private DateTime localTime;

    private int myHotKeyIDc;
    private int myHotKeyIDs;

    private TextBox textbox1 = new TextBox();

    [STAThread]
    public static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run( new ScreenCapture());
    }

    ScreenCapture()
    {
        CreateForm();
    }

    void CreateForm()
    {
        Label label1 = new Label();
        Label label2 = new Label();
        Label label3 = new Label();
        Button button1 = new Button();
        Button button2 = new Button();

        if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP") {
            myHotKeyIDc = GlobalAddAtom("My Global Hot Key ctrl" + GetHashCode().ToString());
            if (!RegisterHotKey(this.Handle, myHotKeyIDc, MOD_CTRL, (int)Keys.PrintScreen))
            {
                MessageBox.Show("ホットキー(ctrl)を登録できませんでした。");
            }
            myHotKeyIDs = GlobalAddAtom("My Global Hot Key shift" + GetHashCode().ToString());
            if (!RegisterHotKey(this.Handle, myHotKeyIDs, MOD_SHIFT, (int)Keys.PrintScreen))
            {
                MessageBox.Show("ホットキー(shift)を登録できませんでした。");
            }

            Text = "ウィンドウキャプチャープログラム";
            ClientSize = new Size(400, 200);

            label1.Location = new Point(10, 10);
            label1.Size = new Size(380, 40);
            label1.Font = new Font(label1.Font.FontFamily, 20);
            label1.Text = "ウィンドウキャプチャープログラム";

            label2.Location = new Point(10, 50);
            label2.Size = new Size(60, 20);
            label2.Text = "保存先";

            label3.Location = new Point(10, 150);
            label3.Size = new Size(350, 50);
            label3.Text = "Ctrl+PrintScreenを押すとアクティブなウィンドウを保存します。\nShift+PrintScreenを押すとデスクトップを保存します。";

            textbox1.Location = new Point(80, 50);
            textbox1.Size = new Size(300, 20);
            textbox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            button1.Location = new Point(100, 80);
            button1.Size = new Size(100, 20);
            button1.Text = "フォルダーを選択する";
            button1.Click += button1_Click;

            button2.Location = new Point(100, 120);
            button2.Size = new Size(100, 20);
            button2.Text = "終了";
	    button2.Click += button2_Click;
        } else {
            myHotKeyIDc = GlobalAddAtom("My Global Hot Key ctrl" + GetHashCode().ToString());
            if (!RegisterHotKey(this.Handle, myHotKeyIDc, MOD_CTRL, (int)Keys.PrintScreen))
            {
                MessageBox.Show("Failed to assign a hotkey (ctrl).");
            }
            myHotKeyIDs = GlobalAddAtom("My Global Hot Key shift" + GetHashCode().ToString());
            if (!RegisterHotKey(this.Handle, myHotKeyIDs, MOD_SHIFT, (int)Keys.PrintScreen))
            {
                MessageBox.Show("Failed to assign a hotkey. (shift)");
            }

            Text = "Window Capture Program";
            ClientSize = new Size(400, 200);

            label1.Location = new Point(10, 10);
            label1.Size = new Size(380, 40);
            label1.Font = new Font(label1.Font.FontFamily, 20);
            label1.Text = "Window Capture Program";

            label2.Location = new Point(10, 50);
            label2.Size = new Size(60, 20);
            label2.Text = "Save in";

            label3.Location = new Point(10, 150);
            label3.Size = new Size(350, 50);
            label3.Text = "Press Ctrl+PrintScreen to save the active window.\nPress Shift+PrintScreen to save the desktop.";

            textbox1.Location = new Point(80, 50);
            textbox1.Size = new Size(300, 20);
            textbox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            button1.Location = new Point(100, 80);
            button1.Size = new Size(100, 20);
            button1.Text = "Select folder";
            button1.Click += button1_Click;

            button2.Location = new Point(100, 120);
            button2.Size = new Size(100, 20);
            button2.Text = "Exit";
            button2.Click += button2_Click;
        }

        Controls.AddRange(new Control[] { label1, label2, label3, button1, button2, textbox1 });

    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == WM_HOTKEY)
        {
            int stateCtrl = GetAsyncKeyState(VK_CONTROL);
            int stateShift = GetAsyncKeyState(VK_SHIFT);
            if ((stateCtrl & 0x8000) == 0x8000)
            {
                captureWindow();
            }
            else if ((stateShift & 0x8000) == 0x8000)
            {
                captureDesktop();
            }
        }
    }

    void captureWindow()
    {
        SendKeys.SendWait("%{PRTSC}");
        IDataObject clipdata = Clipboard.GetDataObject();
        if (clipdata.GetDataPresent(DataFormats.Bitmap))
        {
            Bitmap bitmap = (Bitmap)clipdata.GetData(DataFormats.Bitmap);
            localTime = DateTime.Now;
            if (textbox1.Text == "")
                 textbox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            bitmap.Save(textbox1.Text + "\\" + serialnum.ToString("D4") + "_" +
                localTime.ToString("yyyyMMddHHmmss") + ".jpg", ImageFormat.Jpeg);
            serialnum++;
        }
    }

    void captureDesktop()
    {
        SendKeys.SendWait("^{PRTSC}");
        IDataObject clipdata = Clipboard.GetDataObject();
        if (clipdata.GetDataPresent(DataFormats.Bitmap))
        {
            Bitmap bitmap = (Bitmap)clipdata.GetData(DataFormats.Bitmap);
            localTime = DateTime.Now;
            if (textbox1.Text == "")
                 textbox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            bitmap.Save(textbox1.Text + "\\" + serialnum.ToString("D4") + "_" +
                localTime.ToString("yyyyMMddHHmmss") + ".jpg", ImageFormat.Jpeg);
            serialnum++;
        }
    }


    void button1_Click(object sender, System.EventArgs e)
    {
        FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
        folderBrowserDialog1.SelectedPath = textbox1.Text;
        if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
            textbox1.Text = folderBrowserDialog1.SelectedPath;
        }
    }

    void button2_Click(object sender, System.EventArgs e)
    {
        if (!UnregisterHotKey(this.Handle, myHotKeyIDc))
        {
            MessageBox.Show("Failed to unregister the hotkey (ctrl).");
        }
        if (!UnregisterHotKey(this.Handle, myHotKeyIDs))
        {
            MessageBox.Show("Failed to unregister the hotkey. (shift)");
        }
        Application.Exit();
    }

}
