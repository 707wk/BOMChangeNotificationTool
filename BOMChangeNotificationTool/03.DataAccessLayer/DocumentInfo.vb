Public Class DocumentInfo

    ''' <summary>
    ''' 变更单别
    ''' </summary>
    Public BGDB As String
    ''' <summary>
    ''' 变更单号
    ''' </summary>
    Public BGDH As String
    ''' <summary>
    ''' 变更序号
    ''' </summary>
    Public BGXH As String
    ''' <summary>
    ''' 主件品号
    ''' </summary>
    Public ZJPH As String
    ''' <summary>
    ''' 主件品名
    ''' </summary>
    Public ZJPM As String

    '''' <summary>
    '''' 变更原因
    '''' </summary>
    'Public ChangeReason As String

    ''' <summary>
    ''' BOM序号(旧)
    ''' </summary>
    Public BOMIndexOld As String
    ''' <summary>
    ''' 元件品号(旧)
    ''' </summary>
    Public YJPHOld As String
    ''' <summary>
    ''' 元件品名(旧)
    ''' </summary>
    Public YJPMOld As String
    ''' <summary>
    ''' 组成用量(旧)
    ''' </summary>
    Public ZCYLOld As Decimal
    ''' <summary>
    ''' 插件位置(旧)
    ''' </summary>
    Public CJWZOld As String
    '''' <summary>
    '''' 备注(旧)
    '''' </summary>
    'Public BZOld As String

    ''' <summary>
    ''' BOM序号(新)
    ''' </summary>
    Public BOMIndexNew As String
    ''' <summary>
    ''' 元件品号(新)
    ''' </summary>
    Public YJPHNew As String
    ''' <summary>
    ''' 元件品名(新)
    ''' </summary>
    Public YJPMNew As String
    ''' <summary>
    ''' 组成用量(新)
    ''' </summary>
    Public ZCYLNew As Decimal
    ''' <summary>
    ''' 插件位置(新)
    ''' </summary>
    Public CJWZNew As String
    '''' <summary>
    '''' 备注(新)
    '''' </summary>
    'Public BZNew As String

    ''' <summary>
    ''' 受影响的工单列表,工单单别/工单单号/生产通知单号/业务人员
    ''' </summary>
    Public GDItems As List(Of String())

    ''' <summary>
    ''' 操作类型
    ''' </summary>
    Public ReadOnly Property OperationStr As String
        Get

            If String.IsNullOrWhiteSpace(YJPHNew) Then Return "删除"

            If String.IsNullOrWhiteSpace(YJPHOld) Then Return "新增"

            If YJPHOld <> YJPHNew Then Return "替换"

            Return "修改"

        End Get
    End Property

    ''' <summary>
    ''' 文档主键
    ''' </summary>
    Public ReadOnly Property KeyStr As String
        Get
            Return Wangk.Hash.SHAHelper.GetStrSHA512(String.Join("-",
                                                                 {
                                                                 BGDB,
                                                                 BGDH,
                                                                 BGXH,
                                                                 ZJPH,
                                                                 BOMIndexOld,
                                                                 YJPHOld,
                                                                 BOMIndexNew,
                                                                 YJPHNew
                                                                 }))
        End Get
    End Property

End Class
