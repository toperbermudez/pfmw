<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTest
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTest))
        Me.btnProcess = New System.Windows.Forms.Button()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.lblDeveloper = New System.Windows.Forms.Label()
        Me.PBImportInv = New System.Windows.Forms.PictureBox()
        Me.tmrProc = New System.Windows.Forms.Timer(Me.components)
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.lblLastUpdTime = New System.Windows.Forms.Label()
        Me.chkOverride = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        CType(Me.PBImportInv, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnProcess
        '
        Me.btnProcess.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btnProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnProcess.Font = New System.Drawing.Font("Segoe UI Semibold", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnProcess.ForeColor = System.Drawing.Color.White
        Me.btnProcess.Location = New System.Drawing.Point(15, 7)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Size = New System.Drawing.Size(519, 49)
        Me.btnProcess.TabIndex = 0
        Me.btnProcess.Text = "&Process"
        Me.btnProcess.UseVisualStyleBackColor = True
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.BackColor = System.Drawing.Color.Teal
        Me.lblVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblVersion.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVersion.ForeColor = System.Drawing.Color.Yellow
        Me.lblVersion.Location = New System.Drawing.Point(235, 84)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(44, 13)
        Me.lblVersion.TabIndex = 794
        Me.lblVersion.Text = "Version"
        Me.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.lblTime.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTime.ForeColor = System.Drawing.Color.Yellow
        Me.lblTime.Location = New System.Drawing.Point(12, 63)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(194, 13)
        Me.lblTime.TabIndex = 793
        Me.lblTime.Text = "Next update will be in 900 second(s)."
        '
        'lblDeveloper
        '
        Me.lblDeveloper.AutoSize = True
        Me.lblDeveloper.BackColor = System.Drawing.Color.Teal
        Me.lblDeveloper.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblDeveloper.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDeveloper.ForeColor = System.Drawing.Color.Yellow
        Me.lblDeveloper.Location = New System.Drawing.Point(12, 84)
        Me.lblDeveloper.Name = "lblDeveloper"
        Me.lblDeveloper.Size = New System.Drawing.Size(169, 13)
        Me.lblDeveloper.TabIndex = 796
        Me.lblDeveloper.Text = "Develop By: Ferdie E. Dela Peña"
        Me.lblDeveloper.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'PBImportInv
        '
        Me.PBImportInv.BackColor = System.Drawing.Color.White
        Me.PBImportInv.Image = CType(resources.GetObject("PBImportInv.Image"), System.Drawing.Image)
        Me.PBImportInv.Location = New System.Drawing.Point(-1, 110)
        Me.PBImportInv.Name = "PBImportInv"
        Me.PBImportInv.Size = New System.Drawing.Size(548, 29)
        Me.PBImportInv.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PBImportInv.TabIndex = 795
        Me.PBImportInv.TabStop = False
        Me.PBImportInv.Visible = False
        '
        'tmrProc
        '
        Me.tmrProc.Interval = 1000
        '
        'BackgroundWorker1
        '
        '
        'lblLastUpdTime
        '
        Me.lblLastUpdTime.AutoSize = True
        Me.lblLastUpdTime.BackColor = System.Drawing.Color.Teal
        Me.lblLastUpdTime.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblLastUpdTime.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastUpdTime.ForeColor = System.Drawing.Color.Yellow
        Me.lblLastUpdTime.Location = New System.Drawing.Point(347, 84)
        Me.lblLastUpdTime.Name = "lblLastUpdTime"
        Me.lblLastUpdTime.Size = New System.Drawing.Size(128, 13)
        Me.lblLastUpdTime.TabIndex = 797
        Me.lblLastUpdTime.Text = "Last Update: 07-24-2018"
        Me.lblLastUpdTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'chkOverride
        '
        Me.chkOverride.AutoSize = True
        Me.chkOverride.Enabled = False
        Me.chkOverride.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkOverride.ForeColor = System.Drawing.Color.White
        Me.chkOverride.Location = New System.Drawing.Point(468, 18)
        Me.chkOverride.Name = "chkOverride"
        Me.chkOverride.Size = New System.Drawing.Size(70, 30)
        Me.chkOverride.TabIndex = 798
        Me.chkOverride.Text = "Override" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Regular"
        Me.chkOverride.UseVisualStyleBackColor = True
        Me.chkOverride.Visible = False
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.White
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Label1.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.Label1.Font = New System.Drawing.Font("Segoe UI Semibold", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Yellow
        Me.Label1.Location = New System.Drawing.Point(0, 110)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(546, 28)
        Me.Label1.TabIndex = 799
        '
        'frmTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Teal
        Me.ClientSize = New System.Drawing.Size(546, 138)
        Me.Controls.Add(Me.btnProcess)
        Me.Controls.Add(Me.chkOverride)
        Me.Controls.Add(Me.lblLastUpdTime)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblTime)
        Me.Controls.Add(Me.lblDeveloper)
        Me.Controls.Add(Me.PBImportInv)
        Me.Controls.Add(Me.Label1)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.Name = "frmTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "PF Middleware"
        CType(Me.PBImportInv, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnProcess As System.Windows.Forms.Button
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents lblDeveloper As System.Windows.Forms.Label
    Friend WithEvents PBImportInv As System.Windows.Forms.PictureBox
    Friend WithEvents tmrProc As System.Windows.Forms.Timer
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents lblLastUpdTime As System.Windows.Forms.Label
    Friend WithEvents chkOverride As System.Windows.Forms.CheckBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
