Imports System.Data.SqlClient
Imports System.Timers
Imports DingTalk.Api
Imports DingTalk.Api.Request
Imports DingTalk.Api.Response
Imports Microsoft.AppCenter.Analytics

Class MainWindow

    Private SendTimer As Timer

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        Me.Title = $"{My.Application.Info.Title} V{AppSettingHelper.Instance.ProductVersion}"

        Dim tmpAppCenterSparkle As New AppCenterSparkle(AppSettingHelper.AppKey, Me)
        tmpAppCenterSparkle.CheckUpdateAsync()

        StartAutoRun.IsChecked = AppSettingHelper.Instance.StartAutoRun

        SendTimer = New Timer With {
            .Interval = 60 * 1000
        }
        AddHandler SendTimer.Elapsed, AddressOf SendTimerElapsed

        'If Debugger.IsAttached Then
        '    Exit Sub
        'End If

        SendTimer.Start()

        ' 开机自启后最小化
        If AppSettingHelper.Instance.StartAutoRun Then
            Me.WindowState = WindowState.Minimized
        End If

    End Sub

    ''' <summary>
    ''' 定时处理
    ''' </summary>
    Private Sub SendTimerElapsed(sender As Object, e As ElapsedEventArgs)

        Analytics.TrackEvent("自动查找数据")
        AppSettingHelper.Instance.Logger.Info("自动查找数据")

        Me.Dispatcher.Invoke(Sub()
                                 WorkFunction(Nothing, Nothing)
                             End Sub)

    End Sub

    Public Sub Shutdown()

        SendTimer.Stop()
        RemoveHandler SendTimer.Elapsed, AddressOf SendTimerElapsed

        AppSettingHelper.SaveToLocaltion()

        System.Windows.Application.Current.Shutdown()

        End

    End Sub

    Private Sub UpdateInfoMenuItem_Click(sender As Object, e As RoutedEventArgs)

        FileHelper.Open("https://install.appcenter.ms/orgs/hunan-yestech/apps/erpbom-bian4-geng1-ti2-xing3-gong1-ju4/distribution_groups/public")

    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As RoutedEventArgs)

        Dim tmpWindow As New AboutWindow With {
          .Owner = Me
        }
        tmpWindow.ShowDialog()

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

        e.Cancel = True

        Me.WindowState = WindowState.Minimized

    End Sub

    Private Sub WorkFunction(sender As Object, e As RoutedEventArgs)

        If AppSettingHelper.Instance.Sending Then
            Exit Sub
        End If
        AppSettingHelper.Instance.Sending = True

        Dim tmpWindow As New Wangk.ResourceWPF.BackgroundWork(Me) With {
            .Title = "初始化"
        }

        tmpWindow.Run(Sub(uie)
                          Dim stepCount = 4

                          Using SqlConn As New SqlConnection(AppSettingHelper.Instance.ERPSqlServerConnStr)
                              SqlConn.Open()

#Region "获取上次搜索日期后BOM更改信息"
                              uie.Write("获取上次搜索日期后BOM更改信息", 0 * 100 / stepCount)

                              AppSettingHelper.Instance.DocumentItems.Clear()

                              Using tmpSqlCommand = SqlConn.CreateCommand
                                  tmpSqlCommand.CommandText = $"select
rtrim(CMSMQ.MQ002)+'('+CMSMQ.MQ001+')' as 变更单别,
tempBOMTB.变更单号,
tempBOMTB.变更序号,
tempBOMTB.主件品号,
BOMTC.TC004 as BOM序号,
BOMTC.TC005 as 元件品号,
BOMTC.TC008 as 组成用量,
BOMTC.TCC02 as 插件位置,
BOMTC.TC039 as 备注,
BOMTC.TC104 as 原BOM序号,
BOMTC.TC105 as 原元件品号,
BOMTC.TC108 as 原组成用量,
BOMTC.TCC01 as 原插件位置,
BOMTC.TC040 as 原备注,
INVMB.MB002 as 主件品名,
INVMB1.MB002 as 元件品名,
INVMB2.MB002 as 原元件品名,
tempBOMTB.变更原因

from
    (select
    TB001 as 变更单别,
    TB002 as 变更单号,
    TB003 as 变更序号,
    TB004 as 主件品号,
    tempBOMTA.变更原因

    from
        (select
        TA001 as 变更单别,
        TA002 as 变更单号,
        TA005 as 变更原因

        from BOMTA
        where TA003>='{AppSettingHelper.Instance.LastSearchDate:yyyyMMdd}') as tempBOMTA

    inner join BOMTB
    on BOMTB.TB001=tempBOMTA.变更单别
    and BOMTB.TB002=tempBOMTA.变更单号
    
    where BOMTB.TB012='Y') as tempBOMTB

inner join BOMTC
on BOMTC.TC001=tempBOMTB.变更单别
and BOMTC.TC002=tempBOMTB.变更单号
and BOMTC.TC003=tempBOMTB.变更序号

left join CMSMQ
on CMSMQ.MQ001=tempBOMTB.变更单别
        
left join INVMB
on INVMB.MB001=tempBOMTB.主件品号

left join INVMB as INVMB1
on INVMB1.MB001=BOMTC.TC005

left join INVMB as INVMB2
on INVMB2.MB001=BOMTC.TC105"

                                  Using tmpSqlDataReader = tmpSqlCommand.ExecuteReader

                                      While tmpSqlDataReader.Read

                                          Dim tmpDocumentInfo = New DocumentInfo With {
                                          .BGDB = $"{tmpSqlDataReader(0)}".Trim,
                                          .BGDH = $"{tmpSqlDataReader(1)}".Trim,
                                          .BGXH = $"{tmpSqlDataReader(2)}".Trim,
                                          .ZJPH = $"{tmpSqlDataReader(3)}".Trim,
                                          .BOMIndexNew = $"{tmpSqlDataReader(4)}".Trim,
                                          .YJPHNew = $"{tmpSqlDataReader(5)}".Trim,
                                          .ZCYLNew = tmpSqlDataReader(6),
                                          .CJWZNew = $"{tmpSqlDataReader(7)}".Trim,
                                          .BOMIndexOld = $"{tmpSqlDataReader(9)}".Trim,
                                          .YJPHOld = $"{tmpSqlDataReader(10)}".Trim,
                                          .ZCYLOld = tmpSqlDataReader(11),
                                          .CJWZOld = $"{tmpSqlDataReader(12)}".Trim,
                                          .ZJPM = $"{tmpSqlDataReader(14)}".Trim,
                                          .YJPMNew = $"{tmpSqlDataReader(15)}".Trim,
                                          .YJPMOld = $"{tmpSqlDataReader(16)}".Trim
                                          }

                                          ' 忽略发送过的
                                          If AppSettingHelper.Instance.SendDocumentIDItems.Contains(tmpDocumentInfo.KeyStr) Then
                                              Continue While
                                          End If

                                          AppSettingHelper.Instance.DocumentItems.Add(tmpDocumentInfo)

                                      End While

                                  End Using

                              End Using
#End Region

#Region "获取BOM更改信息影响的工单列表"
                              uie.Write("获取BOM更改信息影响的工单列表", 1 * 100 / stepCount)

                              For Each item In AppSettingHelper.Instance.DocumentItems
                                  item.GDItems = GetGDByPH(item.ZJPH, SqlConn)
                              Next

#End Region

                              SqlConn.Close()
                          End Using

#Region "清空昨天的发送记录"
                          uie.Write("清空昨天的发送记录", 2 * 100 / stepCount)

                          If (Now.Year <> AppSettingHelper.Instance.LastSearchDate.Year OrElse
                          Now.Month <> AppSettingHelper.Instance.LastSearchDate.Month OrElse
                          Now.Day <> AppSettingHelper.Instance.LastSearchDate.Day) AndAlso
                          AppSettingHelper.Instance.DocumentItems.Count = 0 Then

                              AppSettingHelper.Instance.SendDocumentIDItems.Clear()
                              Analytics.TrackEvent("清空昨天的发送记录")
                              AppSettingHelper.Instance.Logger.Info("清空昨天的发送记录")

                          End If

                          AppSettingHelper.Instance.LastSearchDate = Now
                          AppSettingHelper.SaveToLocaltion()
#End Region

#Region "发送群通知消息"
                          uie.Write("发送群通知消息", 3 * 100 / stepCount)

                          Dim tmpID = 1
                          For Each item In AppSettingHelper.Instance.DocumentItems

                              uie.Write($"发送群通知消息 {tmpID}/{AppSettingHelper.Instance.DocumentItems.Count}")
                              tmpID += 1

                              AppSettingHelper.Instance.SendDocumentIDItems.Add(item.KeyStr)

                              ' 钉钉限制发送频率 20/min
                              Threading.Thread.Sleep(3000)

                              ' 发送消息
                              SendDingTalkGroupMessage(item)
                              AppSettingHelper.Instance.Logger.Info($"单据编号 {String.Join("-",
                                                                                        {
                                                                                        item.BGDB,
                                                                                        item.BGDH,
                                                                                        item.BGXH,
                                                                                        item.ZJPH,
                                                                                        item.BOMIndexOld,
                                                                                        item.YJPHOld,
                                                                                        item.BOMIndexNew,
                                                                                        item.YJPHNew
                                                                                        })}")

                          Next

                          AppSettingHelper.SaveToLocaltion()

#End Region

                      End Sub)

        AppSettingHelper.Instance.Sending = False

        If tmpWindow.Error IsNot Nothing Then
            Wangk.ResourceWPF.Toast.ShowError(Me, tmpWindow.Error.Message)
            Exit Sub
        End If

        If tmpWindow.IsCancel Then
            Wangk.ResourceWPF.Toast.ShowInfo(Me, $"操作已取消")
        Else
            Wangk.ResourceWPF.Toast.ShowSuccess(Me, $"操作完毕")
        End If

    End Sub

#Region "根据品号获取工单列表"
    ''' <summary>
    ''' 根据品号获取工单列表
    ''' </summary>
    Private Function GetGDByPH(ph As String,
                               SqlConn As SqlConnection) As List(Of String())

        Dim tmplist As New List(Of String())

        Using tmpSqlCommand = SqlConn.CreateCommand
            tmpSqlCommand.CommandText = $"select
rtrim(CMSMQ.MQ002)+'('+CMSMQ.MQ001+')' as 工单单别,
tempMOCTA.工单单号,
tempMOCTA.生产通知单号

from
    (select
    TA001 as 工单单别,
    TA002 as 工单单号,
    UDF01 as 生产通知单号

    from MOCTA
    where TA011 <> 'Y'
    and TA011 <> 'y'
    and TA006='{ph}') as tempMOCTA

left join CMSMQ
on CMSMQ.MQ001=tempMOCTA.工单单别"

            Using tmpSqlDataReader = tmpSqlCommand.ExecuteReader

                While tmpSqlDataReader.Read

                    tmplist.Add({
                                $"{tmpSqlDataReader(0)}",
                                $"{tmpSqlDataReader(1)}",
                                $"{tmpSqlDataReader(2)}"
                                })

                End While

            End Using

        End Using

        Return tmplist

    End Function
#End Region

#Region "发送群通知消息"
    ''' <summary>
    ''' 发送群通知消息
    ''' </summary>
    Private Sub SendDingTalkGroupMessage(doc As DocumentInfo)

        Dim client As New DefaultDingTalkClient(AppSettingHelper.Instance.DingTalkWebhook)
        Dim req As New OapiRobotSendRequest With {
            .Msgtype = "markdown"
        }
        Dim obj1 As New OapiRobotSendRequest.MarkdownDomain With {
            .Text = $"**<font color=#1296DB>{doc.BGDB} - {doc.BGDH}</font>**

------
变更操作 : {doc.OperationStr}  
主件品号 : <font color=#1296DB>{doc.ZJPH} ({doc.ZJPM})</font>  
物料品号 : <font color=#1296DB>{doc.YJPHOld} ({doc.YJPMOld})</font> {If(doc.YJPHOld = doc.YJPHNew, "", $"-> <font color=#FF0000>{doc.YJPHNew} ({doc.YJPMNew})</font>")}  
组成用量 : <font color=#1296DB>{doc.ZCYLOld:n4}</font> {If(doc.ZCYLOld = doc.ZCYLNew AndAlso doc.YJPHOld = doc.YJPHNew, "", $"-> <font color=#FF0000>{doc.ZCYLNew:n4}</font>")}  
插件位置 : <font color=#1296DB>{doc.CJWZOld}</font> {If(doc.CJWZOld = doc.CJWZNew AndAlso doc.YJPHOld = doc.YJPHNew, "", $"-> <font color=#FF0000>{doc.CJWZNew}</font>")}  
影响工单 :  
{If(doc.GDItems.Count = 0,
"> 无",
String.Join(vbCrLf, From item In doc.GDItems
                    Select $"> {item(0)} - {item(1)} - {item(2)}  "))}",
            .Title = $"{doc.BGDB} - {doc.BGDH}"
        }
        req.Markdown_ = obj1
        Dim rsp = client.Execute(req)

        AppSettingHelper.Instance.Logger.Info($"消息TaskId {rsp.RequestId} {rsp.Errcode}-{rsp.Errmsg}")

    End Sub
#End Region

#Region "保存修改"
    Private Sub SaveChange(sender As Object, e As RoutedEventArgs)

        Try

            If AppSettingHelper.Instance.StartAutoRun <> StartAutoRun.IsChecked Then

                If StartAutoRun.IsChecked Then

                    Dim shortcutPath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Startup) }\{My.Application.Info.ProductName}.lnk"
                    Dim tmpWshShell = New IWshRuntimeLibrary.WshShell()
                    Dim tmpIWshShortcut As IWshRuntimeLibrary.IWshShortcut = tmpWshShell.CreateShortcut(shortcutPath)
                    With tmpIWshShortcut
                        .TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location
                        .WorkingDirectory = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                        .WindowStyle = 1
                        .Description = My.Application.Info.ProductName
                        .IconLocation = .TargetPath
                        .Save()
                    End With

                Else
                    Dim shortcutPath As String = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Startup) }\{My.Application.Info.ProductName}.lnk"
                    Try
                        IO.File.Delete(shortcutPath)
#Disable Warning CA1031 ' Do not catch general exception types
                    Catch ex As Exception
#Enable Warning CA1031 ' Do not catch general exception types
                    End Try

                End If
            End If

            AppSettingHelper.Instance.StartAutoRun = StartAutoRun.IsChecked

            AppSettingHelper.Instance.ERPSqlServerConnStr = ERPSqlServerConnStr.Value

            AppSettingHelper.Instance.DingTalkWebhook = DingTalkWebhook.Value

        Catch ex As Exception
            Wangk.ResourceWPF.Toast.ShowError(Me, ex.Message)
            Exit Sub
        End Try

        ERPSqlServerConnStr.AddHistoryValue()
        DingTalkWebhook.AddHistoryValue()

        AppSettingHelper.SaveToLocaltion()

        Wangk.ResourceWPF.Toast.ShowSuccess(Me, "修改成功")

    End Sub
#End Region

    Private Sub NotSaveChange(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

End Class
