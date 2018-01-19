﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace MarkupData.mzXML

    Public Module Extensions

        ''' <summary>
        ''' 解析出色谱峰数据
        ''' </summary>
        ''' <param name="peaks"></param>
        ''' <returns></returns>
        <Extension> Public Function ExtractMzI(peaks As peaks) As List(Of MSMSPeak)
            Dim floats#() = peaks.Base64Decode
            Dim data As New List(Of MSMSPeak)

            For Each signal As Double() In floats.Split(2)
                data += New MSMSPeak With {
                    .mz = signal(Scan0),   'm/z质核比数据
                    .intensity = signal(1)        ' 信号强度, 归一化为 0-100 之间的数值
                }
            Next

            Return data
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="scan">
        ''' 可以依据扫描信号结果数据<see cref="scan.msLevel"/>来获取相应的质谱信号数据
        ''' 
        ''' + 1, MS1   一级质谱信号数据
        ''' + 2, MS/MS 二级质谱信号数据
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function ExtractMzI(scan As scan) As (name$, peaks As MSMSPeak())
            Dim name$ = scan.__getName
            Dim peaks As MSMSPeak() = scan.peaks.ExtractMzI(name)
            Return (name, peaks)
        End Function

        <Extension> Private Function __getName(scan As scan) As String
            Dim level$ = If(scan.msLevel = 1, "MS1", "MS/MS")
            Return $"[{level}] {scan.scanType} Scan, ({scan.polarity}) mz={scan.precursorMz} / retentionTime={scan.retentionTime}"
        End Function
    End Module
End Namespace