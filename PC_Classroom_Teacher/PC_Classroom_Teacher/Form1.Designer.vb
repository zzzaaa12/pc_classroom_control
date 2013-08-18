<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
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

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label
        Me.txtInputPassword = New System.Windows.Forms.TextBox
        Me.confirm = New System.Windows.Forms.Button
        Me.cancel = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(22, 29)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(101, 12)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "請輸入驗證密碼："
        '
        'txtInputPassword
        '
        Me.txtInputPassword.Location = New System.Drawing.Point(129, 26)
        Me.txtInputPassword.Name = "txtInputPassword"
        Me.txtInputPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtInputPassword.Size = New System.Drawing.Size(168, 22)
        Me.txtInputPassword.TabIndex = 1
        '
        'confirm
        '
        Me.confirm.Location = New System.Drawing.Point(74, 63)
        Me.confirm.Name = "confirm"
        Me.confirm.Size = New System.Drawing.Size(75, 23)
        Me.confirm.TabIndex = 2
        Me.confirm.Text = "確定"
        Me.confirm.UseVisualStyleBackColor = True
        '
        'cancel
        '
        Me.cancel.Location = New System.Drawing.Point(167, 63)
        Me.cancel.Name = "cancel"
        Me.cancel.Size = New System.Drawing.Size(75, 23)
        Me.cancel.TabIndex = 3
        Me.cancel.Text = "取消"
        Me.cancel.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(324, 110)
        Me.Controls.Add(Me.cancel)
        Me.Controls.Add(Me.confirm)
        Me.Controls.Add(Me.txtInputPassword)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "PC_Classroom_Teacher"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtInputPassword As System.Windows.Forms.TextBox
    Friend WithEvents confirm As System.Windows.Forms.Button
    Friend WithEvents cancel As System.Windows.Forms.Button

End Class
