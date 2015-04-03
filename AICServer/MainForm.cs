using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace AIC
{
	/// <summary>
	/// Arena Server control & view form
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.TextBox tbArenaDisplay;
		
		private Thread m_RefreshViewThread;
		private ArenaServer m_server;
		private bool m_serverIsStarted;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbLog;
		private System.Windows.Forms.Button btnRunClientA;
		private System.Windows.Forms.Button tbnRunBotB;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes the form
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			m_serverIsStarted = false;
			m_server = new ArenaServer();

			m_RefreshViewThread = new Thread(new ThreadStart(RefreshView));
			m_RefreshViewThread.Start();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.tbArenaDisplay = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.tbLog = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnRunClientA = new System.Windows.Forms.Button();
			this.tbnRunBotB = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// tbArenaDisplay
			// 
			this.tbArenaDisplay.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.tbArenaDisplay.Font = new System.Drawing.Font("Courier New", 7.25F);
			this.tbArenaDisplay.Location = new System.Drawing.Point(8, 56);
			this.tbArenaDisplay.Multiline = true;
			this.tbArenaDisplay.Name = "tbArenaDisplay";
			this.tbArenaDisplay.ReadOnly = true;
			this.tbArenaDisplay.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbArenaDisplay.Size = new System.Drawing.Size(776, 440);
			this.tbArenaDisplay.TabIndex = 0;
			this.tbArenaDisplay.Text = "";
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(8, 8);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(112, 24);
			this.btnStart.TabIndex = 1;
			this.btnStart.Text = "Start";
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnStop
			// 
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(128, 8);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(112, 24);
			this.btnStop.TabIndex = 2;
			this.btnStop.Text = "Stop";
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// tbLog
			// 
			this.tbLog.BackColor = System.Drawing.Color.White;
			this.tbLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tbLog.Location = new System.Drawing.Point(8, 520);
			this.tbLog.Multiline = true;
			this.tbLog.Name = "tbLog";
			this.tbLog.ReadOnly = true;
			this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbLog.Size = new System.Drawing.Size(776, 112);
			this.tbLog.TabIndex = 3;
			this.tbLog.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(168, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Arena:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 504);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(168, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Log:";
			// 
			// btnRunClientA
			// 
			this.btnRunClientA.Location = new System.Drawing.Point(552, 8);
			this.btnRunClientA.Name = "btnRunClientA";
			this.btnRunClientA.Size = new System.Drawing.Size(112, 24);
			this.btnRunClientA.TabIndex = 6;
			this.btnRunClientA.Text = "Run Bot A";
			this.btnRunClientA.Click += new System.EventHandler(this.btnRunBotA_Click);
			// 
			// tbnRunBotB
			// 
			this.tbnRunBotB.Location = new System.Drawing.Point(672, 8);
			this.tbnRunBotB.Name = "tbnRunBotB";
			this.tbnRunBotB.Size = new System.Drawing.Size(112, 24);
			this.tbnRunBotB.TabIndex = 7;
			this.tbnRunBotB.Text = "Run Bot B";
			this.tbnRunBotB.Click += new System.EventHandler(this.tbnRunBotB_Click);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(792, 637);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tbnRunBotB,
																		  this.btnRunClientA,
																		  this.label2,
																		  this.label1,
																		  this.tbLog,
																		  this.btnStop,
																		  this.btnStart,
																		  this.tbArenaDisplay});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "AIC Server";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}


		/// <summary>
		/// Event hander executed when user clicks on the Start button
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event</param>
		private void btnStart_Click(object sender, System.EventArgs e)
		{
			if (m_serverIsStarted)
				return;

			m_server.Start();

			btnStart.Enabled = false;
			btnStop.Enabled = true;
			m_serverIsStarted = true;
		}


		/// <summary>
		/// Event hander executed when user clicks on the Stop button
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event</param>
		private void btnStop_Click(object sender, System.EventArgs e)
		{	
			if (!m_serverIsStarted)
				return;

			if (hack_botAThread != null)
				hack_botAThread.Abort();

			if (hack_botBThread != null)
				hack_botBThread.Abort();

			m_server.Stop();

			btnStart.Enabled = true;
			btnStop.Enabled = false;
			m_serverIsStarted = false;
		}


		/// <summary>
		/// Looping thread method used for refreshing the arena preview
		/// </summary>
		private void RefreshView()
		{
			while (true)
			{
				tbArenaDisplay.Lines = m_server.GetStateDisplay();		
				tbLog.Lines = m_server.GetLog();
	
				Thread.Sleep(100);
			}
		}


		/// <summary>
		/// TEMP debug
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRunBotA_Click(object sender, System.EventArgs e)
		{
			hack_botAThread = new Thread(new ThreadStart(RunBotA));
			hack_botAThread.Start();
		}

		private void tbnRunBotB_Click(object sender, System.EventArgs e)
		{
			hack_botBThread = new Thread(new ThreadStart(RunBotB));
			hack_botBThread.Start();		
		}

		private Thread hack_botAThread;
		private Thread hack_botBThread;

		protected void RunBotA()
		{
			TestClientBot botA = new TestClientBot();
			botA.Run("bot_a");				
		}

		protected void RunBotB()
		{
			TestClientBot botA = new TestClientBot();
			botA.Run("bot_b");		
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (hack_botAThread != null)
				hack_botAThread.Abort();

			if (hack_botBThread != null)
				hack_botBThread.Abort();
			
			if (m_RefreshViewThread != null)
				m_RefreshViewThread.Abort();

			m_RefreshViewThread = null;
			hack_botAThread = null;
			hack_botBThread = null;

			m_server.Stop();
			m_server = null;
		}

	}
}
