Public Class Form1
    Dim strPasswd As String = "" '這邊可以設定教師端的密碼
    Dim intFail As Integer = 1
    Dim boolLock As Boolean = False
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '將密碼視窗定位在畫面正中央
        Me.Left = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2
        Me.Top = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2
    End Sub

    Private Sub check_permission()
        boolLock = True
        If txtInputPassword.Text = strPasswd Then
            Form2.Show()
            If strPasswd.Length = 0 Then
                MsgBox("目前沒有密碼，建議設定一組密碼上去")
            End If
            Me.Visible = False
            txtInputPassword.Text = ""
        Else
            If intFail < 5 Then
                MsgBox("請重新輸入密碼！", , "密碼錯誤")
                txtInputPassword.Text = ""
                intFail = intFail + 1
            Else
                Me.Visible = False
                MsgBox("密碼錯誤達5次，程式結束！", MsgBoxStyle.Critical, "密碼錯誤")
                End
            End If
        End If
    End Sub

    Private Sub confirm_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles confirm.Click
        check_permission()
    End Sub

    Private Sub cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancel.Click
        Me.Visible = False
        MsgBox("程式結束！")
        End
    End Sub

    Private Sub TextBox1_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtInputPassword.KeyUp
        If e.KeyValue = System.Windows.Forms.Keys.Enter Then '按下 Enter 鍵
            If boolLock Then
                boolLock = False
            Else
                check_permission()
            End If
        End If
    End Sub
End Class


