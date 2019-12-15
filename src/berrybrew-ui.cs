using BerryBrew;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

public class BBUI : System.Windows.Forms.Form {
    private Berrybrew bb = new Berrybrew();
    
    private System.Windows.Forms.NotifyIcon trayIcon;
    private System.Windows.Forms.ContextMenu contextMenu;
    private System.Windows.Forms.MenuItem rightClickExit;

    private Label currentPerlLabel;
    
    private ComboBox perlSwitchSelect;
    private Button perlSwitchButton;
    
    private ComboBox perlInstallSelect;
    private Button perlInstallButton;
    
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
        
        string iconPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string iconFile = System.IO.Directory.GetParent(iconPath) + @"\inc\berrybrew.ico";
        
        trayIcon.Icon = new Icon(iconFile);
        trayIcon.ContextMenu = this.contextMenu;
        trayIcon.Text = "berrybrew UI";
        trayIcon.Visible = true;
        trayIcon.Click += new System.EventHandler(this.trayIcon_Click);

        InitializeComponents();

        this.Name = "Form";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.ResumeLayout(false);

        this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
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
        
        this.InitializePerlInstallSelect();
        this.InitializePerlInstallButton();
    }
 
    private void InitializeCurrentPerlLabel() {
            this.currentPerlLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.currentPerlLabel.AutoSize = true;
            this.currentPerlLabel.Location = new System.Drawing.Point(10, 10);
            this.currentPerlLabel.Name = "currentPerlLabel";
            this.currentPerlLabel.Size = new System.Drawing.Size(35, 35);
            this.currentPerlLabel.TabIndex = 0;
            this.currentPerlLabel.Font = new Font(this.Font, FontStyle.Bold);

            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.currentPerlLabel);
            this.Name = "BBUI";
            this.ResumeLayout(false);
            this.PerformLayout();
    }

    private void CurrentPerlLabel_Redraw() {
         this.currentPerlLabel.Text = "Current Perl: ";

         string perlInUse = bb.PerlInUse().Name;
 
         if (perlInUse == null) {
             perlInUse = "None configured";
         }
 
         this.currentPerlLabel.Text = currentPerlLabel.Text += perlInUse;       
    }
    
    private void InitializePerlInstallButton() {
        this.perlInstallButton = new System.Windows.Forms.Button();

        this.perlInstallButton.Location = new System.Drawing.Point(139, 65);
        this.perlInstallButton.Name = "perlInstallButton";
        this.perlInstallButton.Size = new System.Drawing.Size(75, 23);
        this.perlInstallButton.TabIndex = 1;
        this.perlInstallButton.Text = "Install";
        this.perlInstallButton.UseVisualStyleBackColor = true;

        this.perlInstallButton.Click += new System.EventHandler(this.installPerlButton_Click);
    }
    
    private void installPerlButton_Click(object Sender, EventArgs e) {
        if (perlInstallSelect.Text == "") {
            System.Windows.Forms.MessageBox.Show("No Perl selected to install!");
            return;         
        }       
        
        string perlName = perlInstallSelect.Text;
        bb.Install(perlName);
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        Redraw();
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
 
    private void switchPerlButton_Click(object Sender, EventArgs e) {
        if (perlSwitchSelect.Text == "") {
            System.Windows.Forms.MessageBox.Show("No Perl selected to switch to!");
            return;         
        }
        
        string newPerl = perlSwitchSelect.Text;
        bb.Switch(newPerl);
        Redraw();
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        Redraw();
    }  
     
    private void InitializePerlInstallSelect() {
        this.perlInstallSelect = new System.Windows.Forms.ComboBox();
        this.perlInstallSelect.DropDownStyle = ComboBoxStyle.DropDownList;

        this.perlInstallSelect.FormattingEnabled = true;
        this.perlInstallSelect.Location = new System.Drawing.Point(10, 65);
        this.perlInstallSelect.Name = "perlSwitchSelect";
        this.perlInstallSelect.Size = new System.Drawing.Size(121, 30);
        this.perlInstallSelect.TabIndex = 0;

        foreach (string perlName in bb.AvailableList()) {
            this.perlInstallSelect.Items.Add(perlName );           
        }
    }

    private void PerlInstallSelect_Redraw() {
        perlInstallSelect.Items.Clear();
        
         foreach (string perlName in bb.AvailableList()) {
             this.perlInstallSelect.Items.Add(perlName );           
         }       
        
         perlInstallSelect.SelectedIndex = -1;
    }
     
    private void InitializePerlSwitchSelect() {
        this.perlSwitchSelect = new System.Windows.Forms.ComboBox();
        this.perlSwitchSelect.DropDownStyle = ComboBoxStyle.DropDownList;

        this.perlSwitchSelect.FormattingEnabled = true;
        this.perlSwitchSelect.Location = new System.Drawing.Point(10, 35);
        this.perlSwitchSelect.Name = "perlSwitchSelect";
        this.perlSwitchSelect.Size = new System.Drawing.Size(121, 30);
        this.perlSwitchSelect.TabIndex = 0;

        string perlInUse = bb.PerlInUse().Name;
        
        foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
            if (perl.Name == perlInUse)
                continue;
            
            this.perlSwitchSelect.Items.Add(perl.Name );           
        }
    }
 
    private void PerlSwitchSelect_Redraw() {
        perlSwitchSelect.Items.Clear();
        
        string perlInUse = bb.PerlInUse().Name;
        
        foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
            if (perl.Name == perlInUse)
                continue;
            
            this.perlSwitchSelect.Items.Add(perl.Name );           
        }

        perlSwitchSelect.SelectedIndex = -1;
    }

    private void trayIcon_Click(object Sender, EventArgs e) {
        if (this.WindowState == FormWindowState.Minimized) {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        else {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
    }
    
    private void rightClickExit_Click(object Sender, EventArgs e) {
        this.Close();
    }

    private void Form1_Load(object sender, EventArgs e) {
        
        this.ClientSize = new System.Drawing.Size(240, 100);

        this.Controls.Add(this.perlSwitchButton);
        this.Controls.Add(this.perlSwitchSelect);

        this.Controls.Add(this.perlInstallButton);
        this.Controls.Add(this.perlInstallSelect);

        Redraw();
        
        this.Name = "BBUI";
        this.Text = "Berrybrew UI";
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        this.ShowInTaskbar = false;
        this.ResumeLayout(false);
    }    
   
    private void Form1_FormClosing(Object sender, FormClosingEventArgs e) {
        if (! new StackTrace().GetFrames().Any(x => x.GetMethod().Name == "Close")){
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }
    }
    
    private void Redraw() {
        CurrentPerlLabel_Redraw();
        PerlInstallSelect_Redraw();
        PerlSwitchSelect_Redraw();
    }
    
    
}
