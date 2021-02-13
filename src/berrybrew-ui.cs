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
    private Button perlOpenButton;
    private Button perlOffButton;

    private ComboBox perlSwitchSelect;
    private Button perlSwitchButton;

    private ComboBox perlInstallSelect;
    private Button perlInstallButton;

    private ComboBox perlRemoveSelect;
    private Button perlRemoveButton;

    private ComboBox perlUseSelect;
    private Button perlUseButton;

	private CheckBox fileAssocCheckBox;
	private CheckBox warnOrphansCheckBox;
	private CheckBox debugCheckBox;
	private CheckBox windowsHomedirCheckBox;

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
        this.InitializePerlOpenButton();
        this.InitializePerlOffButton();

        this.InitializePerlSwitchSelect();
        this.InitializePerlSwitchButton();

        this.InitializePerlInstallSelect();
        this.InitializePerlInstallButton();

        this.InitializePerlRemoveSelect();
        this.InitializePerlRemoveButton();

        this.InitializePerlUseSelect();
        this.InitializePerlUseButton();

        this.InitializeFileAssocCheckBox();
		this.InitializeWarnOrphansCheckBox();
		this.InitializeDebugCheckBox();
		this.InitializeWindowsHomedirCheckBox();
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

    private void InitializePerlOpenButton() {
        this.perlOpenButton = new System.Windows.Forms.Button();

        this.perlOpenButton.Location = new System.Drawing.Point(169, 10);
        this.perlOpenButton.Name = "perlOpenButton";
        this.perlOpenButton.Size = new System.Drawing.Size(45, 20);
        this.perlOpenButton.TabIndex = 1;
        this.perlOpenButton.Text = "Open";
        this.perlOpenButton.UseVisualStyleBackColor = true;

        this.perlOpenButton.Click += new System.EventHandler(this.openPerlButton_Click);
    }

    private void openPerlButton_Click(object Sender, EventArgs e) {
        string perlInUse = bb.PerlInUse().Name;

        if (perlInUse == null) {
            System.Windows.Forms.MessageBox.Show("No Perl currently in use!");
            return;
        }

        bb.UseCompile(perlInUse, true);
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        DrawComponents();
    }

    private void InitializePerlOffButton() {
        this.perlOffButton = new System.Windows.Forms.Button();

        this.perlOffButton.Location = new System.Drawing.Point(215, 10);
        this.perlOffButton.Name = "perlOffButton";
        this.perlOffButton.Size = new System.Drawing.Size(35, 20);
        this.perlOffButton.TabIndex = 1;
        this.perlOffButton.Text = "Off";
        this.perlOffButton.UseVisualStyleBackColor = true;

        this.perlOffButton.Click += new System.EventHandler(this.offPerlButton_Click);
    }

    private void offPerlButton_Click(object Sender, EventArgs e) {
        string perlInUse = bb.PerlInUse().Name;

        if (perlInUse == null) {
            System.Windows.Forms.MessageBox.Show("No Perl currently in use!");
            return;
        }

        bb.Off();
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        DrawComponents();
    }

    private void CurrentPerlLabel_Redraw() {
         this.currentPerlLabel.Text = "Current Perl: ";

         string perlInUse = bb.PerlInUse().Name;
 
         if (perlInUse == null) {
             perlInUse = "Not configured";
         }
 
         this.currentPerlLabel.Text = currentPerlLabel.Text += perlInUse;       
    }

   	private void InitializeFileAssocCheckBox() {
		this.fileAssocCheckBox = new System.Windows.Forms.CheckBox();
		this.fileAssocCheckBox.Width = 200;
		this.fileAssocCheckBox.AutoSize = true;
		this.fileAssocCheckBox.Text = "Manage file association";
        this.fileAssocCheckBox.Location = new System.Drawing.Point(10, 155);
        this.fileAssocCheckBox.Checked = FileAssocManaged() ? true : false;
        this.fileAssocCheckBox.CheckedChanged += new System.EventHandler(this.fileAssocCheckedChanged);
		Controls.Add(fileAssocCheckBox);
	}
    private void FileAssocCheckBox_Redraw() {
        this.fileAssocCheckBox.Checked = FileAssocManaged() ? true : false;
    }
    private bool FileAssocManaged() {
        string assoc = bb.Options("file_assoc", null, true);
        return assoc == "berrybrewPerl" ? true : false;
    }
    private void fileAssocCheckedChanged(object Sender, EventArgs e) {
        if (fileAssocCheckBox.Checked) {
            if (String.IsNullOrEmpty(bb.PerlInUse().Name)) {
                System.Windows.Forms.MessageBox.Show("No berrybrew Perl in use. Can't set file association.");
                fileAssocCheckBox.Checked = false;
            }
            else {
                Console.WriteLine("Setting file assoc");
                bb.FileAssoc("set");
            }
        }
        else {
            Console.WriteLine("Unsetting file assoc");
            bb.FileAssoc("unset");
        }
    }

   	private void InitializeWarnOrphansCheckBox() {
		this.warnOrphansCheckBox = new System.Windows.Forms.CheckBox();
		this.warnOrphansCheckBox.Width = 200;
		this.warnOrphansCheckBox.AutoSize = true;
		this.warnOrphansCheckBox.Text = "Warn on orphans";
        this.warnOrphansCheckBox.Checked = WarnOrphans() ? true : false;
        this.warnOrphansCheckBox.Location = new System.Drawing.Point(10, 175);
        this.warnOrphansCheckBox.CheckedChanged += new System.EventHandler(this.warnOrphansCheckedChanged);
		Controls.Add(warnOrphansCheckBox);
	}
    private bool WarnOrphans() {
        string assoc = bb.Options("warn_orphans", null, true);
        return assoc == "true" ? true : false;
    }
    private void warnOrphansCheckedChanged(object Sender, EventArgs e) {
        if (warnOrphansCheckBox.Checked) {
            Console.WriteLine("Setting warn_orphans");
            bb.Options("warn_orphans", "true", true);
        }
        else {
            Console.WriteLine("Unsetting warn_orphans");
            bb.Options("warn_orphans", "false", true);
        }
    }
    private void WarnOrphansCheckBox_Redraw() {
        this.warnOrphansCheckBox.Checked = WarnOrphans() ? true : false;
    }

    private void InitializeDebugCheckBox() {
        this.debugCheckBox = new System.Windows.Forms.CheckBox();
        this.debugCheckBox.Width = 200;
        this.debugCheckBox.AutoSize = true;
        this.debugCheckBox.Checked = bb.Options("debug", null, true) == "true" ? true : false;
        this.debugCheckBox.Text = "Debug";
        this.debugCheckBox.Location = new System.Drawing.Point(10, 195);
        this.debugCheckBox.CheckedChanged += new System.EventHandler(this.debugCheckedChanged);
        Controls.Add(debugCheckBox);
    }
    private void debugCheckedChanged(object Sender, EventArgs e) {
        if (debugCheckBox.Checked) {
            Console.WriteLine("Setting debug");
            bb.Options("debug", "true", true);
        }
        else {
            Console.WriteLine("Unsetting debug");
            bb.Options("debug", "false", true);
        }
    }
    private void DebugCheckBox_Redraw() {
        this.debugCheckBox.Checked = bb.Options("debug", null, true) == "true" ? true : false;
    }

    private void InitializeWindowsHomedirCheckBox() {
        this.windowsHomedirCheckBox = new System.Windows.Forms.CheckBox();
        this.windowsHomedirCheckBox.Width = 200;
        this.windowsHomedirCheckBox.AutoSize = true;
        this.windowsHomedirCheckBox.Checked = bb.Options("windows_homedir", null, true) == "true" ? true : false;
        this.windowsHomedirCheckBox.Text = "Windows homedir";
        this.windowsHomedirCheckBox.Location = new System.Drawing.Point(10, 215);
        this.windowsHomedirCheckBox.CheckedChanged += new System.EventHandler(this.windowsHomedirCheckedChanged);
        Controls.Add(windowsHomedirCheckBox);
    }
    private void windowsHomedirCheckedChanged(object Sender, EventArgs e) {
        if (windowsHomedirCheckBox.Checked) {
            Console.WriteLine("Setting windows_homedir");
            bb.Options("windows_homedir", "true", true);
        }
        else {
            Console.WriteLine("Unsetting windows_homedir");
            bb.Options("windows_homedir", "false", true);
        }
    }
    private void WindowsHomedirCheckBox_Redraw() {
        this.windowsHomedirCheckBox.Checked = bb.Options("windows_homedir", null, true) == "true" ? true : false;
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
        DrawComponents();
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
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
        Application.Restart();
        Environment.Exit(0);
    }
      
    private void InitializePerlRemoveButton() {
        this.perlRemoveButton = new System.Windows.Forms.Button();

        this.perlRemoveButton.Location = new System.Drawing.Point(139, 125);
        this.perlRemoveButton.Name = "perlRemoveButton";
        this.perlRemoveButton.Size = new System.Drawing.Size(75, 23);
        this.perlRemoveButton.TabIndex = 1;
        this.perlRemoveButton.Text = "Remove";
        this.perlRemoveButton.UseVisualStyleBackColor = true;

        this.perlRemoveButton.Click += new System.EventHandler(this.removePerlButton_Click);
    }
 
    private void removePerlButton_Click(object Sender, EventArgs e) {
        if (perlRemoveSelect.Text == "") {
            System.Windows.Forms.MessageBox.Show("No Perl selected to remove!");
            return;         
        }
        
        string removePerl = perlRemoveSelect.Text;
        bb.PerlRemove(removePerl);
        DrawComponents();
    }

    private void InitializePerlUseButton() {
        this.perlUseButton = new System.Windows.Forms.Button();

        this.perlUseButton.Location = new System.Drawing.Point(139, 95);
        this.perlUseButton.Name = "perlUseButton";
        this.perlUseButton.Size = new System.Drawing.Size(75, 23);
        this.perlUseButton.TabIndex = 1;
        this.perlUseButton.Text = "Use";
        this.perlUseButton.UseVisualStyleBackColor = true;

        this.perlUseButton.Click += new System.EventHandler(this.usePerlButton_Click);
    }

    private void usePerlButton_Click(object Sender, EventArgs e) {
        if (perlUseSelect.Text == "") {
            System.Windows.Forms.MessageBox.Show("No Perl selected to use!");
            return;
        }

        string perlName = perlUseSelect.Text;
        bb.UseCompile(perlName, true);
        DrawComponents();
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
      
    private void InitializePerlRemoveSelect() {
        this.perlRemoveSelect = new System.Windows.Forms.ComboBox();
        this.perlRemoveSelect.DropDownStyle = ComboBoxStyle.DropDownList;

        this.perlRemoveSelect.FormattingEnabled = true;
        this.perlRemoveSelect.Location = new System.Drawing.Point(10, 125);
        this.perlRemoveSelect.Name = "perlRemoveSelect";
        this.perlRemoveSelect.Size = new System.Drawing.Size(121, 30);
        this.perlRemoveSelect.TabIndex = 0;

        foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
            this.perlRemoveSelect.Items.Add(perl.Name);           
        }
    }

    private void PerlRemoveSelect_Redraw() {
        perlRemoveSelect.Items.Clear();
        
         foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
             this.perlRemoveSelect.Items.Add(perl.Name);           
         }       
        
         perlRemoveSelect.SelectedIndex = -1;
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

    private void InitializePerlUseSelect() {
        this.perlUseSelect = new System.Windows.Forms.ComboBox();
        this.perlUseSelect.DropDownStyle = ComboBoxStyle.DropDownList;

        this.perlUseSelect.FormattingEnabled = true;
        this.perlUseSelect.Location = new System.Drawing.Point(10, 95);
        this.perlUseSelect.Name = "perlUseSelect";
        this.perlUseSelect.Size = new System.Drawing.Size(121, 30);
        this.perlUseSelect.TabIndex = 0;

        foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
            this.perlUseSelect.Items.Add(perl.Name );
        }
    }

    private void PerlUseSelect_Redraw() {
        perlUseSelect.Items.Clear();

        foreach (StrawberryPerl perl in bb.PerlsInstalled()) {
            this.perlUseSelect.Items.Add(perl.Name );
        }
         perlUseSelect.SelectedIndex = -1;
    }

    private void trayIcon_Click(object Sender, EventArgs e) {
        DrawComponents();

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

        this.ClientSize = new System.Drawing.Size(275, 250);

        if (bb.PerlInUse().Name != null) {
            this.Controls.Add(this.perlOpenButton);
        }

        this.Controls.Add(this.perlSwitchButton);
        this.Controls.Add(this.perlSwitchSelect);

        this.Controls.Add(this.perlInstallButton);
        this.Controls.Add(this.perlInstallSelect);

        this.Controls.Add(this.perlRemoveButton);
        this.Controls.Add(this.perlRemoveSelect);

        this.Controls.Add(this.perlUseButton);
        this.Controls.Add(this.perlUseSelect);

        DrawComponents();

        this.Name = "BBUI";

        string runMode = Environment.GetEnvironmentVariable("BB_RUN_MODE");

        if (runMode == "prod") {
            this.Text = "Berrybrew UI v" + bb.Version();
        }
        else if (runMode == "build") {
            this.Text = "BB-build UI v" + bb.Version();
        }

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

    private void DrawComponents() {
        if (bb.PerlInUse().Name != null) {
            this.Controls.Add(this.perlOpenButton);
            this.Controls.Add(this.perlOffButton);
        }
        else {
             this.Controls.Remove(this.perlOpenButton);
             this.Controls.Remove(this.perlOffButton);
        }

        CurrentPerlLabel_Redraw();
        PerlInstallSelect_Redraw();
        PerlSwitchSelect_Redraw();
        PerlRemoveSelect_Redraw();
        PerlUseSelect_Redraw();

        FileAssocCheckBox_Redraw();
        WarnOrphansCheckBox_Redraw();
        DebugCheckBox_Redraw();
        WindowsHomedirCheckBox_Redraw();
    }
}
