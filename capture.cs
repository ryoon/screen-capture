/*
 * The simplest active window screen capture program
 * Copyright (c) 2016 Ryo ONODERA All rights reserved.
 * Author: Ryo ONODERA <ryo_on@yk.rim.or.jp>
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
	private const int WM_HOTKEY = 0x312;

	private UInt32 serialnum = 1;
	private DateTime localTime;

	private int myHotKeyID;

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
		myHotKeyID = GlobalAddAtom("My Global Hot Key" + GetHashCode().ToString());
		if (!RegisterHotKey(this.Handle, myHotKeyID, MOD_CTRL, (int)Keys.PrintScreen))
		{
			MessageBox.Show("Failed to assign a hotkey.");
		}

		Text = "Window Capture Program";
		ClientSize = new Size(400, 200);

		Label label1 = new Label();
		label1.Location = new Point(10, 10);
		label1.Size = new Size(380, 40);
		label1.Font = new Font(label1.Font.FontFamily, 20);
		label1.Text = "Window Capture Program";

		Label label2 = new Label();
		label2.Location = new Point(10, 50);
		label2.Size = new Size(60, 20);
		label2.Text = "Save in";

		textbox1.Location = new Point(80, 50);
		textbox1.Size = new Size(300, 20);
		textbox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

		Button button1 = new Button();
		button1.Location = new Point(100, 80);
		button1.Size = new Size(100, 20);
		button1.Text = "Select folder";
		button1.Click += button1_Click;

		Button button2 = new Button();
		button2.Location = new Point(100, 120);
		button2.Size = new Size(100, 20);
		button2.Text = "Exit";
		button2.Click += button2_Click;


		Controls.AddRange(new Control[] { label1, label2, button1, button2, textbox1 });

	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == WM_HOTKEY)
		{
			captureWindow();
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
		if (!UnregisterHotKey(this.Handle, myHotKeyID))
		{
			MessageBox.Show("Failed to unregister the hotkey.");
		}
		Application.Exit();
	}

}
