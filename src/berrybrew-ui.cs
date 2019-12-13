using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

public class BBUI : System.Windows.Forms.Form {
    private System.Windows.Forms.NotifyIcon trayIcon;
    private System.Windows.Forms.ContextMenu contextMenu;
    private System.Windows.Forms.MenuItem rightClickExit;

    private ComboBox perlSwitchSelect;
    private Button perlSwitchButton;
    private Label currentPerlLabel;
    
    private System.ComponentModel.IContainer components;

    [STAThread]
    static void Main() {
        Application.Run(new BBUI());
    }

    public BBUI() {

        this.components = new System.ComponentModel.Container();
        this.contextMenu = new System.Windows.Forms.ContextMenu();
        this.rightClickExit = new System.Windows.Forms.MenuItem();

        this.contextMenu.MenuItems.AddRange(
                    new System.Windows.Forms.MenuItem[] { this.rightClickExit });

        this.rightClickExit.Index = 0;
        this.rightClickExit.Text = "Exit";
        this.rightClickExit.Click += new System.EventHandler(this.rightClickExit_Click);

        this.ClientSize = new System.Drawing.Size(240, 100);
        this.Text = "berrybrew UI";

        this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);

        trayIcon.Icon = new Icon("inc/berrybrew.ico");
        trayIcon.ContextMenu = this.contextMenu;
        trayIcon.Text = "berrybrew UI";
        trayIcon.Visible = true;
        trayIcon.Click += new System.EventHandler(this.trayIcon_Click);

        InitializeComponents();

        this.Name = "Form";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.ResumeLayout(false);
        

        this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            if (components != null)
                components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponents() {
        this.InitializeCurrentPerlLabel();
        this.InitializePerlSwitchSelect();
        this.InitializePerlSwitchButton();
    }

    private void InitializePerlSwitchButton() {
        this.perlSwitchButton = new System.Windows.Forms.Button();

        this.perlSwitchButton.Location = new System.Drawing.Point(139, 35);
        this.perlSwitchButton.Name = "perlSwitchButton";
        this.perlSwitchButton.Size = new System.Drawing.Size(75, 23);
        this.perlSwitchButton.TabIndex = 1;
        this.perlSwitchButton.Text = "Switch";
        this.perlSwitchButton.UseVisualStyleBackColor = true;

        this.perlSwitchButton.Click += new System.EventHandler(this.switchPerlButton_Click);
    }
    
    private void InitializePerlSwitchSelect() {
        this.perlSwitchSelect = new System.Windows.Forms.ComboBox();
        this.perlSwitchSelect.DropDownStyle = ComboBoxStyle.DropDownList;

        this.perlSwitchSelect.FormattingEnabled = true;
        this.perlSwitchSelect.Location = new System.Drawing.Point(10, 35);
        this.perlSwitchSelect.Name = "perlSwitchSelect";
        this.perlSwitchSelect.Size = new System.Drawing.Size(121, 30);
        this.perlSwitchSelect.TabIndex = 0;

        this.perlSwitchSelect.Items.AddRange(new object[] {
            "5.20.3_64",
            "5.16.3_32",
            "5.12.4_32",
            });

        this.perlSwitchSelect.SelectedIndex = 0;
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
        Debug.WriteLine("CLOSED");
        
    }
   
    private void Form1_Load(object sender, EventArgs e) {
        
        //this.ClientSize = new System.Drawing.Size(240, 100);

        this.Controls.Add(this.perlSwitchButton);
        this.Controls.Add(this.perlSwitchSelect);
        this.currentPerlLabel.Text = currentPerlLabel.Text += "5.30.1_64";
        this.Name = "BBUI";
        this.Text = "Berrybrew UI";
        this.WindowState = FormWindowState.Minimized;
        //this.Hide();
        this.ShowInTaskbar = false;
        this.ResumeLayout(false);
    }    
   
    private void Form1_FormClosing(Object sender, FormClosingEventArgs e) {
        if (! new StackTrace().GetFrames().Any(x => x.GetMethod().Name == "Close")){
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }
    }

    private void InitializeCurrentPerlLabel() {
            this.currentPerlLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.currentPerlLabel.AutoSize = true;
            this.currentPerlLabel.Location = new System.Drawing.Point(10, 10);
            this.currentPerlLabel.Name = "currentPerlLabel";
            this.currentPerlLabel.Size = new System.Drawing.Size(35, 35);
            this.currentPerlLabel.TabIndex = 0;
            this.currentPerlLabel.Text = "Current Perl: ";
            this.currentPerlLabel.Font = new Font(this.Font, FontStyle.Bold);

            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.currentPerlLabel);
            this.Name = "BBUI";
            this.ResumeLayout(false);
            this.PerformLayout();
    }
 
    private void trayIcon_Click(object Sender, EventArgs e) {
        if (this.WindowState == FormWindowState.Minimized) {
            this.WindowState = FormWindowState.Normal;
        }
        else {
            this.WindowState = FormWindowState.Minimized;
        }
    }
    
    private void rightClickExit_Click(object Sender, EventArgs e) {
        this.Close();
    }
 
    private void switchPerlButton_Click(object Sender, EventArgs e) {
        string newPerl = perlSwitchSelect.Text;
        Debug.WriteLine(newPerl);
        this.Close();
    }  
}
