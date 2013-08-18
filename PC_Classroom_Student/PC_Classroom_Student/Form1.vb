Imports System.Net
Imports System.Net.Dns
Imports System.Net.NetworkInformation
Imports System.net.sockets
Imports System.Text
Imports System.IO
Imports Microsoft.Win32

Public Class Form1

    Private Declare Function SetComputerName Lib "kernel32" Alias "SetComputerNameA" (ByVal lpComputerName As String) As Long
    Dim boolAutoChangeName As Boolean = False '啟動時自動檢查&更改電腦名稱
    Dim strListPath As String = "C:\Program Files\MMLab\MAC.txt" '存放學生機資料的地方
    Dim strIP_Prefix As String = "192.168.55"  '固定IP網段
    Dim strNetMask As String = "255.255.255.0" '網樂遮罩
    Dim strGateway As String = "192.168.55.1"  '預設Gateway
    Dim strListURL As String = "http://"       'MAC文字檔下載位址
    Dim strNamePrefix = "S"                    '學生機編號前段
    Dim PC_Name, PC_MAC, PC_FNo As String      '儲存資料：電腦名稱、MAC、編號

    Dim UDPS, UDPC As UdpClient
    Dim recvive_port As Integer = 5566
    Dim send_port As Integer = 5567

    Dim source, teacher As IPEndPoint
    Dim SendB, RecvB As Byte()
    Dim strRecv, message As String
    Dim lastRecv As String
    Dim addr() As IPAddress
    Dim WC As New WebClient

    Dim strNAME(55) As String
    Dim strMAC(55) As String
    Dim strNo(55) As String
    Dim boolInList As String = False

    '隱藏視窗
    Private Sub Form1_Layout(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LayoutEventArgs) Handles MyBase.Layout
        Me.Hide()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim nics() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces
        Dim properties As IPInterfaceProperties
        Dim adapter As NetworkInterface
        Dim strLine As String = ""
        Dim tmpMAC As String = ""
        Dim tmp() As String

        Control.CheckForIllegalCrossThreadCalls = False
        Try
            'UDP Server: 接收指令用
            UDPS = New UdpClient(recvive_port)
            UDPS.EnableBroadcast = True
            source = New IPEndPoint(IPAddress.Any, recvive_port)

        Catch ex As Exception

            End

        End Try

        Timer1.Start()

        If My.Computer.FileSystem.FileExists(strListPath) = False Then
            Exit Sub
        End If

        Try
            '讀取學生機資料檔
            Dim SR As StreamReader = New StreamReader(strListPath, False)
            For i = 1 To 55
                strLine = SR.ReadLine      '文字檔逐行取出
                tmp = Split(strLine, ",")  '依據逗點切割成兩部份  SS1(0)為名稱  SS1(1)為MAC位址
                strNAME(i) = tmp(0)
                strMAC(i) = tmp(1)
                strNo(i) = tmp(2)
            Next

            SR.Close()

            '取得MAC Address
            For Each adapter In nics
                properties = adapter.GetIPProperties()
                If adapter.NetworkInterfaceType = NetworkInterfaceType.Ethernet Then
                    tmpMAC = adapter.GetPhysicalAddress.ToString

                    '過濾錯誤的MAC位址：長度判斷、去除虛擬網卡
                    If tmpMAC.Length = 12 Then
                        For i = 1 To 55
                            If tmpMAC.ToUpper = strMAC(i).ToUpper Then
                                PC_MAC = strMAC(i)
                                PC_Name = strNAME(i)
                                PC_FNo = strNo(i)
                                boolInList = True '代表在List檔案找到對應的項目

                                '自動檢查&更改電腦名稱
                                If boolAutoChangeName And boolInList Then
                                    change_pc_name()
                                End If
                                Exit Sub

                            End If
                        Next
                    End If

                End If
            Next adapter

            '沒有找到就隨便拿一個替換上去，不讓教室內有重複名稱的PC即可
            PC_MAC = tmpMAC.ToUpper
            PC_Name = strNamePrefix + Mid(PC_MAC, 7, 6)

        Catch ex As Exception

        End Try
    End Sub

    '更改電腦名稱
    Sub change_pc_name()
        If Environment.MachineName <> PC_Name Then
            SetComputerName(PC_Name)
            MsgBox("電腦名稱已變更為""" + PC_Name + """，請重新開機以便生效!!")
        End If
    End Sub

    'Timer功能：接收來自教師機的指令
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        '收到來自教師端的命令
        If UDPS.Available > 0 Then
            Try
                RecvB = UDPS.Receive(source)
                strRecv = Encoding.Default.GetString(RecvB, 0, RecvB.Length)

                '將訊息放到 Debug form 上面
                debug.ListBox1.Items.Add("Received: " + strRecv)
                If debug.ListBox1.Items.Count > 0 Then
                    debug.ListBox1.SelectedIndex = debug.ListBox1.Items.Count - 1
                End If

            Catch ex As Exception
                Exit Sub
            End Try

            check_recv(strRecv)
        End If

    End Sub

    '分析接收到的指令
    Private Sub check_recv(ByVal strRecv As String)

        Dim Count, cmd As Integer
        Dim i As Integer
        Dim temp1(), temp2() As String

        '檢查是否重複收到
        If lastRecv = strRecv Then
            Exit Sub
        End If

        lastRecv = strRecv

        If Mid(strRecv, 1, 6) = "@,Scan" Then

            UDPC = New UdpClient()

            Try
                temp1 = Split(strRecv, ",")
                addr = Dns.GetHostAddresses(temp1(2))
                send_port = Int(temp1(3))
            Catch ex As Exception
                addr = Dns.GetHostAddresses("teacher")
                teacher = New IPEndPoint(addr(1), send_port)
            End Try

            ' 取得IPv4位址
            For i = 1 To addr.Length.ToString
                If addr(i).AddressFamily = AddressFamily.InterNetwork Then
                    teacher = New IPEndPoint(addr(i), send_port)
                    Exit For
                End If
            Next

            Try
                If PC_Name = "Teacher" Then
                    SendB = Encoding.Default.GetBytes("#," + PC_Name) '傳送電腦名稱
                Else
                    SendB = Encoding.Default.GetBytes("#," + Mid(PC_Name, strNamePrefix.ToString.Length + 1, 2)) '傳送電腦名稱
                End If

                UDPC.Send(SendB, SendB.Length, teacher)
                debug.ListBox1.Items.Add("Sent:      " + "#," + Mid(PC_Name, strNamePrefix.ToString.Length, 2))
            Catch ex As Exception

            End Try

        ElseIf Mid(strRecv, 1, 1) = "@" Then
            Try
                temp1 = Split(strRecv, ",") 'temp1(1):數量  temp1(2):電腦   temp1(3):cmd  temp1(4):message長度  temp1(5): Message
                temp2 = Split(temp1(2), "-") '電腦編號
                Count = Int(temp1(1)) '       電腦數量
                cmd = Int(temp1(3))  '        cmd編號
                If cmd > 50 Then
                    message = temp1(4) '          訊息
                End If

            Catch ex As Exception
                Exit Sub
            End Try

            For i = 1 To Count
                '編號與本機相同 or ALL1(包含Teacher) or ALL2
                If (Mid(PC_Name, 2, 2) = temp2(i - 1)) Or (temp1(2) = "ALL1") Or ((temp1(2) = "ALL2") And PC_Name <> "Teacher") Then
                    do_cmd(cmd)
                    Exit For
                End If
            Next

        End If
    End Sub

    '執行命令
    Sub do_cmd(ByVal cmd As Integer)

        Select Case cmd

            Case 1 '關機
                Shell("shutdown -s -t 0 ", AppWinStyle.Hide, False)

            Case 2 '重開機
                Shell("shutdown -r -t 0 ", AppWinStyle.Hide, False)

            Case 3 '關機(倒數30s)
                Shell("shutdown -s -t 30 -c ""由老師電腦傳送關機指令""", AppWinStyle.Hide, False)
                If MsgBox("系統將在30秒內關機，取消「關機」請按「確定」", MsgBoxStyle.OkOnly) = MsgBoxResult.Ok Then
                    Shell("shutdown -a", AppWinStyle.Hide, False)
                End If

            Case 4 '重開機(倒數30s)
                Shell("shutdown -r -t 30 -c ""由老師電腦傳送重新開機指令""", AppWinStyle.Hide, False)
                If MsgBox("系統將在30秒內重新關機，不重開機請按""確定""", MsgBoxStyle.OkOnly) = MsgBoxResult.Cancel Then
                    Shell("shutdown -a", AppWinStyle.Hide, False)
                End If

            Case 5 '顯示桌面
                Dim clsidShell As New Guid("13709620-C279-11CE-A49E-444553540000")
                Dim shell As Object = Activator.CreateInstance(Type.GetTypeFromCLSID(clsidShell))
                shell.ToggleDesktop()

            Case 6 '開啟檔案總管
                Shell("c:\windows\explorer.exe", AppWinStyle.NormalFocus, False)

            Case 7 '顯示電腦資訊
                Information.Label1.Text = "Computer Name: " + PC_Name
                Information.Label2.Text = "MAC Address: " + Mid(PC_MAC, 1, 2) + "-" + Mid(PC_MAC, 3, 2) + "-" + Mid(PC_MAC, 5, 2) + "-" + Mid(PC_MAC, 7, 2) + "-" + Mid(PC_MAC, 9, 2) + "-" + Mid(PC_MAC, 11, 2)
                Information.Label3.Text = "Case Number: " + PC_FNo
                Information.Visible = True
                Information.Show()

            Case 8 '隱藏電腦資訊
                Information.Hide()

            Case 9 '更新電腦名稱
                change_pc_name()

            Case 10 '更新名稱列表
                Try
                    WC.DownloadFile(strListURL, "tempListFile.txt")
                    My.Computer.FileSystem.MoveFile("tempListFile", strListPath, True)
                Catch ex As Exception

                End Try

            Case 11  'ipconfig
                Shell("cmd /K ipconfig", AppWinStyle.NormalFocus, False)

            Case 12  'ping google
                Shell("cmd /K ping www.google.com.tw", AppWinStyle.NormalFocus, False)

            Case 13  'trace dns
                Shell("cmd /K tracert 168.95.1.1", AppWinStyle.NormalFocus, False)

            Case 14  'trace google
                Shell("cmd /K tracert www.google.com.tw", AppWinStyle.NormalFocus, False)

            Case 15  '設定為固定IP
                If Mid(PC_Name, 1, strNamePrefix.ToString.Length) = strNamePrefix And PC_Name.Length = 3 Then
                    Shell("netsh interface ip set address ""區域連線"" static " + strIP_Prefix + "." + Int(Mid(PC_Name, 2, 2)).ToString + " " + strNetMask + " " + strGateway + " 1", AppWinStyle.NormalFocus, False)
                End If

            Case 16  '設定為DHCP
                Shell("netsh interface ip set address ""區域連線"" dhcp", AppWinStyle.NormalFocus, False)

            Case 17 '開啟IE
                Shell("C:\Program Files\Internet Explorer\iexplore.exe", AppWinStyle.NormalFocus, False)

            Case 18 '啟動Firefox
                If File.Exists("C:\Program Files\Mozilla Firefox\firefox.exe") Then
                    Shell("C:\Program Files\Mozilla Firefox\firefox.exe", AppWinStyle.NormalFocus, False)
                End If

            Case 23  '顯示Debug
                debug.Show()

            Case 24 '隱藏Debug
                debug.Hide()

            Case 52 '檔案下載
                'message=網址*儲存點
                Dim tmp() As String
                tmp = Split(message, "*")
                WC.DownloadFile(tmp(0), tmp(1))

            Case 54 'CMD指令(執行後隱藏)
                Shell(message, AppWinStyle.NormalFocus, False)

            Case 55 'CMD指令
                Shell("cmd /k " + message, AppWinStyle.NormalFocus, False)

        End Select
    End Sub
End Class
