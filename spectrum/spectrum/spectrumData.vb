﻿Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Data
    Public Property url As String
    Public Property Data As spectrumData()

    Public Shared Function Load(json$) As Data
        Return New Data With {
            .url = json,
            .Data = json.ReadAllText.LoadObject(Of spectrumData())
        }
    End Function
End Class

Public Class spectrumData
    Public Property name As String
    Public Property data As MSSignal()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class MSSignal
    <XmlAttribute> Public Property x As Double
    <XmlAttribute> Public Property y As Double
    <XmlAttribute> Public Property fragment As Boolean

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class