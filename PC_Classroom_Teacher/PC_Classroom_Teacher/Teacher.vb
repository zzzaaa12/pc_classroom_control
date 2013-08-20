Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.Text
Imports System.IO
Imports Microsoft.Win32

Public Class Teacher

    Dim PC_Name As String   '本機電腦名稱
    Dim PC(55) As CheckBox  '畫面上的CheckBoxes，用來代表被控制的學生機(教師機也可)
    Dim Row(9) As CheckBox  '畫面上的群組 R1~R9
    Dim Col(6) As CheckBox  '畫面上的群組 C1~C6
    Dim strNamePrefix = "S" '學生機編號前段
    Dim send_port As Integer = 5566    '發送命命的 port
    Dim receive_port As Integer = 5567 '接收回傳的 port
    Dim last_row_checked(9) As Boolean '紀錄上一次的群組設定(橫向)
    Dim last_col_checked(6) As Boolean '紀錄上一次的群組設定(縱向)
    Dim strListPath As String = "C:\Program Files\MMLab\MAC.txt" '存放學生機資料的地方
    Dim vnc_path As String = "C:\Program Files\RealVNC\vncviewer.exe" 'VNC Client

    '關於UDP Client的設定
    Dim UDPC As UdpClient '發送命命
    Dim UDPS As UdpClient '接收回傳值
    Dim dist As IPEndPoint = New IPEndPoint(IPAddress.Broadcast, send_port)
    Dim send_buf, recv_buf As Byte()

    Private Sub Teacher_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Control.CheckForIllegalCrossThreadCalls = False

        '將視窗定位在畫面正中央
        Me.Left = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2
        Me.Top = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2

        '取得預設的電腦名稱
        PC_Name = Environment.MachineName

        '將畫面上的CheckBox對應到PC, Row, Col等變數，統一用陣列處理
        For i = 1 To 70
            If i <= 55 Then
                PC(i) = Me.Controls.Item("CheckBox" & i.ToString.Trim)
            ElseIf i >= 56 And i <= 64 Then
                Row(i - 55) = Me.Controls.Item("CheckBox" & i.ToString.Trim)
            Else
                Col(i - 64) = Me.Controls.Item("CheckBox" & i.ToString.Trim)
            End If
        Next

        '指定畫面上CheckBox所顯示的編號
        For i = 2 To 54
            If i < 10 Then
                PC(i).Text = "0" + i.ToString
            Else
                PC(i).Text = i.ToString
            End If
        Next i

        '預設勾選所有學生機
        select_all(True)

        'UDP Client: 發送指令用
        UDPC = New UdpClient()
        UDPC.EnableBroadcast = True

        'UDP Server: 接收回傳訊息用(for掃描功能)
        UDPS = New UdpClient(receive_port)
        Timer1.Start()

    End Sub

    'Timer功能：更新畫面上的時間&接收來自學生機的回傳值
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        Dim strRecv As String
        Dim number As Integer

        '更新時間
        lbTime.Text = Now

        '接收學生機的回傳值
        If UDPS.Available > 0 Then
            recv_buf = UDPS.Receive(dist)
            strRecv = Encoding.Default.GetString(recv_buf, 0, recv_buf.Length)
            Try
                If Mid(strRecv, 1, 2) = "#," Then
                    If Mid(strRecv, 3, 10) = "Teacher" Then
                        number = 55 '教師機
                    Else
                        number = Int(Mid(strRecv, 3, 2))
                    End If
                End If
            Catch ex As Exception
                Exit Sub
            End Try

            For i = 1 To 54
                If number = PC(i).Text Then
                    PC(i).BackColor = Color.Coral
                    Exit For
                End If
            Next
        End If

    End Sub

    '全選 or 取消全選
    Private Sub select_all(ByVal boolChecked As Boolean)

        '先處理縱向及橫向群組
        If boolChecked Then '全選
            For i = 1 To 6
                Col(i).Enabled = False
                Col(i).Checked = True
            Next
            For i = 1 To 9
                Row(i).Enabled = False
                Row(i).Checked = True
            Next
        Else '取消全選
            For i = 1 To 6
                Col(i).Enabled = True
                Col(i).Checked = False
            Next
            For i = 1 To 9
                Row(i).Enabled = True
                Row(i).Checked = False
            Next
        End If

        For i = 1 To 54
            PC(i).Checked = boolChecked
        Next

    End Sub

    '傳送指令給學生機
    Sub send(ByVal cmd As Integer, ByVal message As String)

        Try
            Dim pc_count As Integer = 0
            Dim pc_string As String = ""

            '格式：@,機器數量,機器代號,訊息代號,命令總長度,命令內容

            '計算機器數量 pc_string
            For i = 1 To 55
                If PC(i).Checked = True Then
                    pc_string = pc_string + "-" + PC(i).Text
                    pc_count = pc_count + 1
                End If
            Next

            '檢查數量，除了scan功能(cmd=0)
            If pc_count = 0 And cmd <> 0 Then
                MsgBox("未設定被控制之電腦數量")
                Exit Sub
            End If

            '紀錄機器代號(全部 or 有勾選的，格式01-02-03- )
            pc_string = Mid(pc_string, 2) '將第一個"-"排除
            If pc_count = 55 Then '不包含教師機
                pc_string = "ALL1"
            ElseIf (pc_count = 54) And PC(55).Checked = False Then
                pc_string = "ALL2"
            End If

            '依照訊息代號指定send_buf內容
            If cmd = 0 Then
                '掃描電腦，格式： #,Scan,PC_NAME,receive_port
                send_buf = Encoding.Default.GetBytes("@" + ",Scan," + PC_Name + "," + receive_port.ToString) 'PC_NAME-->讓電腦找到回傳目標
            ElseIf cmd < 30 Then
                '編號指令，格式：@,pc_count,pc_string,cmd
                send_buf = Encoding.Default.GetBytes("@" + "," + pc_count.ToString + "," + pc_string + "," + cmd.ToString)
            Else
                '文字指令，格式：@,pc_count,pc_string,cmd,message
                send_buf = Encoding.Default.GetBytes("@" + "," + pc_count.ToString + "," + pc_string + "," + cmd.ToString + "," + message)
            End If

            '送出UDP Broadcast封包
            UDPC.Send(send_buf, send_buf.Length, dist)

        Catch ex As Exception
        End Try

    End Sub

    '選擇紅色標記的學生機
    Private Sub btnSelectMarked_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectMarked.Click

        '先取消所有的勾選項目
        For i = 1 To 9
            Row(i).Checked = False
        Next
        For i = 1 To 6
            Col(i).Checked = False
        Next

        '根據顏色做挑選
        For i = 1 To 55
            If PC(i).BackColor = Color.Coral Then
                PC(i).Checked = True
            End If
        Next

    End Sub

    '勾選所有電腦
    Private Sub btnSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectAll.Click
        select_all(True)
    End Sub

    '清除勾選所有電腦
    Private Sub btnDeselectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeselectAll.Click 'Scan
        select_all(False)
    End Sub

    '橫向群組勾選 (勾選後凍結縱向群組的checkbox)
    Private Sub check_row() Handles CheckBox56.CheckedChanged, CheckBox57.CheckedChanged, CheckBox58.CheckedChanged, CheckBox59.CheckedChanged, CheckBox60.CheckedChanged, CheckBox61.CheckedChanged, CheckBox62.CheckedChanged, CheckBox63.CheckedChanged, CheckBox64.CheckedChanged

        Dim intSelected As Integer = 0

        '先凍結縱向群組
        For i = 1 To 6
            Col(i).Enabled = False
        Next

        '檢查被更動過的CheckBox--> Row(i)
        For i = 0 To 8
            If Row(i + 1).Checked <> last_row_checked(i + 1) Then
                If Row(i + 1).Checked Then
                    For j = 1 To 6
                        PC(i * 6 + j).Checked = True
                    Next
                Else
                    For j = 1 To 6
                        PC(i * 6 + j).Checked = False
                    Next
                End If
                last_row_checked(i + 1) = Row(i + 1).Checked
            End If
        Next

        '如果橫向沒有勾選任何群組，將縱向群組解除凍結
        For i = 1 To 9
            If Row(i).Checked Then
                intSelected = intSelected + 1
            End If
        Next
        If intSelected = 0 Then
            For i = 1 To 6
                Col(i).Enabled = True
            Next
        End If

    End Sub

    '縱向群組勾選 (勾選後凍結橫向群組的 checkbox)
    Private Sub check_col() Handles CheckBox65.CheckedChanged, CheckBox66.CheckedChanged, CheckBox67.CheckedChanged, CheckBox68.CheckedChanged, CheckBox69.CheckedChanged, CheckBox70.CheckedChanged

        Dim intSelected As Integer = 0

        '先凍結橫向群組
        For i = 1 To 9
            Row(i).Enabled = False
        Next

        '檢查被更動過的CheckBox--> Col(i)
        For i = 1 To 6
            If Col(i).Checked <> last_col_checked(i) Then
                If Col(i).Checked Then
                    intSelected = intSelected + 1
                    For j = 0 To 8
                        PC(j * 6 + i).Checked = True
                    Next
                Else
                    For j = 0 To 8
                        PC(j * 6 + i).Checked = False
                    Next
                End If
                last_col_checked(i) = Col(i).Checked
            End If
        Next

        '如果縱向沒有勾選任何群組，將橫向群組解除凍結
        For i = 1 To 6
            If Col(i).Checked Then
                intSelected = intSelected + 1
            End If
        Next
        If intSelected = 0 Then
            For i = 1 To 9
                Row(i).Enabled = True
            Next
        End If

    End Sub

    Private Sub btnVNC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVNC.Click

        Dim pc_count As Integer = 0

        For i = 1 To 55
            If PC(i).Checked = True Then
                pc_count = pc_count + 1
            End If
        Next
        If (pc_count > 0) Then
            If MsgBox("被控制電腦之數量為" + pc_count.ToString + "台，要繼續嗎？", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                For i = 1 To 55
                    If PC(i).Checked = True Then
                        Shell(vnc_path + " " + strNamePrefix + PC(i).Text, AppWinStyle.NormalFocus, False)
                    End If
                Next
            Else
                Exit Sub
            End If
        End If

    End Sub

    Public Sub WOL(ByVal Mac As String)

        Dim buf(101) As Byte
        Try
            Dim tmp(6) As String

            For i = 0 To 5
                buf(i) = &HFF '先放6個FF
                tmp(i) = Mid(Mac, 2 * i + 1, 2) ' 將一整串的MAC切開
            Next

            For i = 0 To 15 '放入連續 16 次目標MAC Address
                For j = 0 To 5
                    buf(6 + i * 6 + j) = Val("&H" & tmp(j))
                Next
            Next

            Dim Sock As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            Dim EndPoint = New IPEndPoint(IPAddress.Broadcast, 9)
            Sock.Connect(EndPoint)
            Sock.Send(buf)
            Sock.Close()

        Catch ex As Exception
        End Try

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        send(1, "") '關機
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        send(2, "") '重開機
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        send(3, "") '關機(倒數)
    End Sub
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        send(4, "") '重開機(倒數)
    End Sub
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        send(5, "") '顯示桌面
    End Sub
    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        send(6, "") '開啟檔案總管
    End Sub
    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        send(7, "") '顯示電腦資訊
    End Sub
    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        send(8, "") '隱藏電腦資訊
    End Sub
    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        send(9, "") '更新電腦名稱
    End Sub
    Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
        send(10, "") '更新名稱列表
    End Sub
    Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
        send(11, "") 'ipconfig
    End Sub
    Private Sub Button12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button12.Click
        send(12, "") 'ping google
    End Sub
    Private Sub Button13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button13.Click
        send(13, "") 'trace dns
    End Sub
    Private Sub Button14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button14.Click
        send(14, "") 'trace google
    End Sub
    Private Sub Button15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button15.Click
        send(15, "") '設定為固定IP
    End Sub
    Private Sub Button16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button16.Click
        send(16, "") '設定為DHCP
    End Sub
    Private Sub Button17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button17.Click
        send(17, "") '開啟IE
    End Sub
    Private Sub Button18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button18.Click
        send(18, "") 'Firefox
    End Sub
    Private Sub Button24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button24.Click
        send(24, "")
    End Sub
    Private Sub Button23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button23.Click
        send(23, "") 'Debug Mode
    End Sub
    Private Sub btnScanPC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnScanPC.Click
        send(0, "") '掃描
    End Sub
    Private Sub Button45_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button45.Click

        Dim Data As String
        Dim tmp() As String
        Dim MAC(55) As String

        If File.Exists(strListPath) = False Then
            MsgBox("缺少 " + strListPath + " 檔案")
            Exit Sub
        End If

        Dim SR As StreamReader = New StreamReader(strListPath, False)
        For i = 1 To 55
            Data = SR.ReadLine      '文字檔逐行取出
            tmp = Split(Data, ",")  '依據逗點切割成兩部份  tmp(0)為名稱  tmp(1)為MAC位址
            If PC(i).Checked Then
                WOL(tmp(1))
            End If
        Next
        SR.Close()

    End Sub
    Private Sub Button46_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button46.Click
        '檔案下載 step1. 預覽
        Shell("C:\Program Files\Internet Explorer\iexplore.exe " + TextBox1.Text, , False)
    End Sub
    Private Sub Button52_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button52.Click
        send(52, TextBox1.Text + "*" + TextBox2.Text) '檔案下載 step2. 
    End Sub
    Private Sub Button47_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button47.Click
        Me.Hide()
        Login.Show()
    End Sub
    Private Sub Button54_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button54.Click
        If CheckBox71.Checked Then
            send(54, TextBox5.Text) 'CMD指令(隱藏)
        Else
            send(55, TextBox5.Text) 'CMD指令
        End If
        MsgBox("指令已送出")
    End Sub

    Private Sub TextBox2_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.DoubleClick
        SFD1.ShowDialog()
        TextBox2.Text = SFD1.FileName
    End Sub

    Private Sub Teacher_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        End
    End Sub
End Class