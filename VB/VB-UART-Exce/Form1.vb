'
' RS-232C経由で受信したデータをExcelに表示する
'
' Microsoft Visual Basic 2005
' Microsoft Excel2003
'
' 【注意】
' ・「プロジェクト」→「参照の追加」→「COM」→「Microsoft Excel 11.0 ObjectLibrary」を選択する。
'
' 2017.06.07 shiguanghu 
'

'
' RS-232Cライブラリ
'
Imports System
Imports System.IO.Ports
Imports System.Threading
Imports Microsoft.Office.Interop

Public Class Form1
    ' Excelの行番号
    Private LineNo As Long

    ' 起動時にExcelを起動する
    Dim xlApp As New Excel.Application
    Dim xlBooks As Excel.Workbooks = xlApp.Workbooks

    ' Excelの新規のファイルを開く場合
    Dim xlBook As Excel.Workbook = xlBooks.Add
    Dim xlSheets As Excel.Sheets = xlBook.Worksheets
    Dim xlSheet As Excel.Worksheet = xlSheets.Item(1)

    ' RS-232Cのイベント
    Private WithEvents _RS232C As New System.IO.Ports.SerialPort

    Delegate Sub AddMessageDelegate(ByVal str As String)

    ' .ImeMode.Disable

    '
    ' テキストボックスにメッセージを表示する
    '
    Private Sub AddMessage(ByVal str As String)
        TextBox4.Text = DateTime.Now.ToString("HH:mm:ss") + " " + str + ControlChars.CrLf + TextBox4.Text
    End Sub

    '
    ' 起動時の処理
    '
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LineNo = 1

        Button1.Enabled = True
        Button2.Enabled = False
        Button3.Enabled = False
        Button4.Enabled = False
        Button5.Enabled = True

        ' COM port
        With ComboBox1.Items
            .Clear()
            .Add("COM1")
            .Add("COM2")
            .Add("COM3")
            .Add("COM4")
            .Add("COM5")
            .Add("COM6")
            .Add("COM7")
            .Add("COM8")
            .Add("COM9")
            .Add("COM10")
            .Add("COM11")
            .Add("COM12")
            .Add("COM13")
        End With
        ComboBox1.Text = "COM9"

        ' 速度
        With ComboBox2.Items
            .Clear()
            .Add("2400")
            .Add("4800")
            .Add("9600")
            .Add("19200")
            .Add("38400")
            .Add("57600")
            .Add("115200")
        End With
        ComboBox2.Text = "115200"

        'パリティ
        With ComboBox3.Items
            .Add("Nothing") '0:Parity.None
            .Add("奇数")    '1:Parity.Odd
            .Add("偶数")    '2:Parity.Even
            .Add("Mark")    '3:Parity.Mark
            .Add("Space")   '4:Parity.Space
        End With
        ComboBox3.Text = "Nothing"

        ' データ長
        With ComboBox4.Items
            .Add("4")
            .Add("5")
            .Add("6")
            .Add("7")
            .Add("8")
        End With
        ComboBox4.Text = "8"

        ' ストップビット
        With ComboBox5.Items
            .Add("なし")    '0:StopBits.None
            .Add("1")       '1:StopBits.One
            .Add("2")       '2:StopBits.Two
            .Add("1.5")     '3:StopBits.OnePointFive
        End With
        ComboBox5.Text = "1"

        ' 受信バッファサイズ
        TextBox1.Text = "2048"

        ' 送信バッファサイズ
        TextBox2.Text = "1024"

        ' パリティエラー時の置換文字
        TextBox3.Text = "?"

        ' Null文字を破棄
        CheckBox1.Checked = False

        ' RTSラインを有効
        CheckBox1.Checked = False

        ' DTRラインを有効
        CheckBox1.Checked = True
    End Sub

    '
    ' 通信の開始
    '
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ' Excelの行番号の初期化
        ' LineNo = 1

        If (txtSendData.Text.Length = 0) Then
            MessageBox.Show("制御CMDを入力してください！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            txtSendData.Focus()
            Exit Sub
        End If

        ' Excelの表示
        xlApp.Visible = True

        RS232C_Open()

        RS232C_Send()

        Me.SerialPortx.BaudRate = 115200

        '_RS232C_Receive(sender, e)   'Private Sub _RS232C_Receive(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles _RS232C.DataReceived

        Button1.Enabled = False
        Button2.Enabled = True
        Button3.Enabled = False
        Button4.Enabled = False
        Button5.Enabled = False
    End Sub

    '
    ' プログラムの終了
    '
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Me.Close()
    End Sub

    '
    ' プログラムの終了イベント
    '
    Private Sub Form1_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        ' RS-232Cの終了処理
        RS232C_Close()

        ' Excelの終了処理
        MRComObject(xlSheet)  ' xlSheet の解放
        MRComObject(xlSheets) ' xlSheets の解放
        xlBook.Close(False)   ' xlBook を閉じる
        MRComObject(xlBook)   ' xlBook の解放
        MRComObject(xlBooks)  ' xlBooks の解放
        xlApp.Quit()          ' Excelを閉じる
        MRComObject(xlApp)    ' xlApp を解放

        ' テスト用（終了時のExcelの起動数を確認）
        ' GC.Collect()
        ' 終了後すぐ再起動したい場合は、強制的にガベージ コレクションを実行する。
        System.Threading.Thread.Sleep(1000)
        Dim localByName As Process() = Process.GetProcessesByName("Excel")
        If localByName.Length > 0 Then
            MessageBox.Show("まだ EXCEL.EXE が " & localByName.Length & " 個 起動しています。")
        End If

        Button1.Enabled = True
        Button2.Enabled = False
        Button3.Enabled = True
        Button4.Enabled = True
        Button5.Enabled = True

        Environment.Exit(0)
    End Sub

    '
    ' Excelの全てのセルを消去する
    '
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ' Excelの行番号の追加
        LineNo = 1

        ' Excelの全てのセルを消去
        xlSheet.Cells.Clear()
    End Sub

    '
    ' 記録停止
    '
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        ' RS-232Cの終了処理
        RS232C_Close()

        Button1.Enabled = True
        Button2.Enabled = True
        Button3.Enabled = True
        Button4.Enabled = True
        Button5.Enabled = True
    End Sub

    '
    ' Excelファイルの保存処理
    '
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim ofd As New OpenFileDialog()

        ofd.FileName = "Auto_Focus_Test_Data_" + DateTime.Now.ToString("HHmmss") + ".xls"
        ofd.InitialDirectory = "C:\"
        ofd.Filter = "Excelファイル(*.xls;.XLS)|*.xls;*.XLS|すべてのファイル(*.*)|*.*"
        ofd.FilterIndex = 0
        ofd.Title = "保存ファイル名の指定"
        ofd.RestoreDirectory = True
        ofd.CheckFileExists = False
        ofd.CheckPathExists = True

        If ofd.ShowDialog() = DialogResult.OK Then
            Console.WriteLine(ofd.FileName)
        End If

        ' ファイルの保存処理
        Dim xlFilePath As String = ofd.FileName  '保存ファイル名
        xlApp.DisplayAlerts = False              '保存時の問合せのダイアログを非表示に設定
        xlSheet.SaveAs(xlFilePath)
        xlApp.DisplayAlerts = True
    End Sub

    '
    ' Excelにデータを書き込む（確認用ルーチン）実際には未使用。
    '
    Private Sub Excel_Write()
        ' サンプルデータ入力
        Dim xlCells As Excel.Range
        Dim xlRange As Excel.Range

        xlCells = xlSheet.Cells
        xlRange = xlCells(LineNo, 1)
        xlRange.Value = 1 + LineNo
        MRComObject(xlRange) 'xlRange の解放(都度解放しないとだめ）
        xlRange = xlCells(LineNo, 2)
        xlRange.Value = 2 + LineNo
        MRComObject(xlRange) 'xlRange の解放(都度解放しないとだめ）
        xlRange = xlCells(LineNo, 3)
        xlRange.Value = 3 + LineNo
        MRComObject(xlCells) 'xlCells の解放(こちらは１回でOK)
        MRComObject(xlRange) 'xlRange の解放

        LineNo = LineNo + 1

        ' System.Threading.Thread.Sleep(1000)
    End Sub

    '
    ' RS-232Cの接続
    '
    Private Sub RS232C_Open()
        Try
            With _RS232C
                ''プロパティセット
                .PortName = ComboBox1.Text
                .BaudRate = CInt(ComboBox2.Text)
                .Parity = CType(ComboBox3.SelectedIndex, Parity)
                .DataBits = CInt(ComboBox4.Text)
                .StopBits = CType(ComboBox5.SelectedIndex, StopBits)
                .ReadBufferSize = CInt(TextBox1.Text)
                .WriteBufferSize = CInt(TextBox2.Text)
                .ParityReplace = System.Text.Encoding.GetEncoding("Shift_JIS").GetBytes(TextBox3.Text)(0)
                '.ParityReplace = System.Text.Encoding.GetEncoding("ASCII").GetBytes(TextBox3.Text)(0)
                .DiscardNull = CheckBox1.Checked
                .RtsEnable = CheckBox2.Checked
                .DtrEnable = CheckBox3.Checked

                If (.IsOpen = True) Then
                    MessageBox.Show("Already open COM Port" & .PortName, "m(_ _)m",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End If

                ' COM portを開く
                Call .Open()
                Call AddMessage(.PortName + " Port Open!")
            End With

        Catch ex As Exception
            MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '
    ' RS-232Cの切断
    '
    Private Sub RS232C_Close()
        If (_RS232C.IsOpen = True) Then
            Call _RS232C.Close()
            Call AddMessage(_RS232C.PortName + " Port Close!")
        End If
    End Sub

    '
    ' RS-232Cの受信
    '
    Private Sub _RS232C_Receive(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles _RS232C.DataReceived
        Dim xlCells As Excel.Range
        Dim xlRange As Excel.Range
        Dim addmsg As New AddMessageDelegate(AddressOf AddMessage)
        Dim rData(16) As Byte
        Dim r As Integer
        Dim st As String
        ' Dim strDataReceived As String
        ' Dim iLength As Integer

        ' データ受信
        Try
            ' strDataReceived = _RS232C.ReadLine
            r = _RS232C.Read(rData, 0, 16)
        Catch ex As Exception
            ' strDataReceived = ex.Message
        End Try

        ' 受信した文字列の長さ
        ' iLength = strDataReceived.Length

        If r <> False Then
            ' TextBoxに受信文字を表示する
            ' この処理を入れると正常にCOMが停止出来ずにフリーズするのでコメントする。
            ' TextBox4.Invoke(addmsg, New Object() {"RCV: " + strDataReceived})

            ' 必ず決められたフォーマットで送信されると仮定。
            ' xlCells = xlSheet.Cells

            ' Excelに入力する
            If Chr(rData(0)) = "A" Then
                xlCells = xlSheet.Cells
                xlRange = xlCells(LineNo, 1)
                st = Chr(rData(1))
                xlRange.Value = st
                MRComObject(xlRange) 'xlRange の解放(毎回解放する）
            End If

            If Chr(rData(2)) = "B" Then
                xlRange = xlCells(LineNo, 2)
                st = Chr(rData(3))
                xlRange.Value = st
                MRComObject(xlRange) 'xlRange の解放(毎回解放する）
            End If

            If Chr(rData(4)) = "C" Then
                xlRange = xlCells(LineNo, 3)
                st = Chr(rData(5))
                xlRange.Value = st
                MRComObject(xlCells) 'xlCells の解放(こちらは１回でOK)
                MRComObject(xlRange) 'xlRange の解放(毎回解放する）

                LineNo = LineNo + 1
            End If

            ' MessageBox.Show(Chr(rData(5)), "", MessageBoxButtons.OK, MessageBoxIcon.Error)

            'xlCells = xlSheet.Cells
            'xlRange = xlCells(LineNo, 1)
            'xlRange.Value = (strDataReceived(0))
            'MRComObject(xlRange) 'xlRange の解放(毎回解放する）
            'xlRange = xlCells(LineNo, 2)
            'xlRange.Value = strDataReceived(1)
            'MRComObject(xlRange) 'xlRange の解放(毎回解放する）
            'xlRange = xlCells(LineNo, 3)
            'xlRange.Value = strDataReceived(2)
            'MRComObject(xlCells) 'xlCells の解放(こちらは１回でOK)
            'MRComObject(xlRange) 'xlRange の解放

            Exit Sub
        End If
    End Sub

    '
    ' RS-232Cの送信
    '
    Private Sub RS232C_Send()
        If (txtSendData.Text.Length = 0) Then
            MessageBox.Show("送信文字列を入力してください", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
            txtSendData.Focus()
            Exit Sub
        End If

        Try
            _RS232C.WriteLine("Direct " + txtSendData.Text)
            '_RS232C.WriteLine(Str)
            'Call AddMessage("Direct " + txtSendData.Text)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '
    ' RS-232Cのエラーイベント
    '
    Private Sub _RS232C_ErrorReceived(ByVal sender As Object,
                                   ByVal e As System.IO.Ports.SerialErrorReceivedEventArgs) _
                                   Handles _RS232C.ErrorReceived

        Dim addmsg As New AddMessageDelegate(AddressOf AddMessage)
        Dim strErrorMessage As String = "ErrorReceived"

        Select Case e.EventType
            Case SerialError.Frame
                strErrorMessage = "The hardware detected a framing error."
            Case SerialError.Overrun
                strErrorMessage = "A character-buffer overrun has occurred. The next character is lost."
            Case SerialError.RXOver
                strErrorMessage = "An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character."
            Case SerialError.RXParity
                strErrorMessage = "The hardware detected a parity error."
            Case SerialError.TXFull
                strErrorMessage = "The application tried to transmit a character, but the output buffer was full."
        End Select
        TextBox4.Invoke(addmsg, New Object() {"ERR: " + strErrorMessage})

    End Sub

    '
    ' RS-232CのPin変更イベント
    '
    Private Sub _RS232C_PinChanged(ByVal sender As Object,
                                ByVal e As System.IO.Ports.SerialPinChangedEventArgs) _
                                Handles _RS232C.PinChanged

        ' 以下はDebug時のみ有効にする
        '
        '       Dim addmsg As New AddMessageDelegate(AddressOf AddMessage)
        '       Dim strPinMessage As String = "PinChanged"
        '       Select Case e.EventType
        '           Case SerialPinChange.Break
        '       strPinMessage = "A break was detected on input."
        '            Case SerialPinChange.CDChanged
        '        strPinMessage = "The Receive Line Signal Detect (RLSD) signal changed state."
        '            Case SerialPinChange.CtsChanged
        '        strPinMessage = "The Clear to Send (CTS) signal changed state."
        '            Case SerialPinChange.DsrChanged
        '        strPinMessage = "The Data Set Ready (DSR) signal changed state."
        '            Case SerialPinChange.Ring
        '        strPinMessage = "A ring indicator was detected."
        '        End Select
        '        TextBox4.Invoke(addmsg, New Object() {"[PIN]" + strPinMessage})
    End Sub


    '
    ' COM オブジェクトへの参照を解放するプロシージャ（既存のファイルを開く場合も共用）
    '
    Private Sub MRComObject(ByRef objCom As Object)
        ' COM オブジェクトの使用後、明示的に COM オブジェクトへの参照を解放する
        Try
            ' 提供されたランタイム呼び出し可能ラッパーの参照カウントをデクリメントする
            If Not objCom Is Nothing AndAlso System.Runtime.InteropServices.
                                                      Marshal.IsComObject(objCom) Then
                Dim I As Integer
                Do
                    I = System.Runtime.InteropServices.Marshal.ReleaseComObject(objCom)
                Loop Until I <= 0
            End If
        Catch
        Finally
            ' 参照を解除する
            objCom = Nothing
        End Try
    End Sub
End Class
