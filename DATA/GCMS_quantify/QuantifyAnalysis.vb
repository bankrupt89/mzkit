﻿#Region "Microsoft.VisualBasic::b11c1deec689c8403a107f6412a184a9, GCMS_quantify\QuantifyAnalysis.vb"

' Author:
' 
'       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
' 
' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
' 
' 
' MIT License
' 
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all
' copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.



' /********************************************************************************/

' Summaries:

' Module QuantifyAnalysis
' 
'     Function: ExportReferenceROITable, ExportROI, ReadData
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.application.netCDF
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.MassSpectrum.Math
Imports SMRUCC.MassSpectrum.Math.Chromatogram
Imports Microsoft.VisualBasic.Net.Http

''' <summary>
''' GCMS自动化定量分析模块
''' 
''' https://github.com/cheminfo-js/netcdf-gcms
''' </summary>
Public Module QuantifyAnalysis

    ''' <summary>
    ''' 读取CDF文件然后读取原始数据
    ''' </summary>
    ''' <param name="cdfPath"></param>
    ''' <returns></returns>
    Public Function ReadData(cdfPath$, Optional vendor$ = "agilentGCMS", Optional showSummary As Boolean = True) As GCMSJson
        Dim cdf As New netCDFReader(cdfPath)

        If showSummary Then
            Call Console.WriteLine(cdf.ToString)
        End If

        Select Case vendor
            Case "agilentGCMS" : Return agilentGCMS.Read(cdf)
            Case Else
                Throw New NotImplementedException(vendor)
        End Select
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ExportROI(gcms As GCMSJson) As ROI()
        Return gcms.GetTIC _
            .Shadows _
            .PopulateROI _
            .ToArray
    End Function

    ''' <summary>
    ''' 利用标准品的信息从GCMS的实验数据之中找出对应的检测物质的检测结果
    ''' </summary>
    ''' <param name="metabolites">标准品数据</param>
    ''' <param name="data">实验数据</param>
    ''' <param name="sn#">信噪比阈值，低于这个阈值的信号都将会被抛弃</param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ScanContents(metabolites As ROITable(), data As GCMSJson, Optional sn# = 3) As IEnumerable(Of ROITable)
        Dim ROIlist As ROI() = data.ExportROI _
            .Where(Function(ROI) ROI.snRatio >= sn) _
            .ToArray
        Dim riA = (metabolites(0).rt, metabolites(0).ri)
        Dim riB = (metabolites.Last.rt, metabolites.Last.ri)

    End Function

    ''' <summary>
    ''' 导出标准品参考的ROI区间列表，用于``GC/MS``自动化定性分析
    ''' </summary>
    ''' <param name="regions"></param>
    ''' <param name="sn">
    ''' 信噪比筛选阈值
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 保留指数的计算：在标准化流程之中，GCMS的出峰顺序保持不变，但是保留时间可能会在不同批次实验间有变化
    ''' 这个时候如果定量用的标准品混合物和样本之中的所检测物质的出峰顺序一致，则可以将标准品混合物之中的
    ''' 第一个出峰的物质和最后一个出峰的物质作为保留指数的参考，在这里假设第一个出峰的物质的保留指数为零，
    ''' 最后一个出峰的物质的保留指数为1000，则可以根据这个区间和rt之间的线性关系计算出保留指数
    ''' </remarks>
    <Extension> Public Function ExportReferenceROITable(regions As ROI(), raw As GCMSJson,
                                                        Optional sn# = 5,
                                                        Optional names$() = Nothing,
                                                        Optional RImax# = 1000) As ROITable()

        With regions.Where(Function(ROI) ROI.snRatio >= sn).ToArray
            Dim refA = .First, refB = .Last
            Dim A = (refA.rt, 0)
            Dim B = (refB.rt, RImax)
            Dim getTitle As Func(Of ROI, Integer, String)

            If names.IsNullOrEmpty Then
                getTitle = Function(ROI, i) $"#{i + 1}={Fix(ROI.rt)}s"
            Else
                getTitle = Function(ROI, i) names.ElementAtOrDefault(i, $"#{i + 1}={Fix(ROI.rt)}s")
            End If

            Return .Select(Function(ROI, i)
                               Dim spectra = raw.GetMsScan(ROI.Time).GroupByMz
                               Dim base64 As String = spectra _
                                   .Select(Function(mz) $"{mz.mz} {mz.intensity}") _
                                   .JoinBy(ASCII.TAB) _
                                   .Base64String

                               Return New ROITable With {
                                   .sn = ROI.snRatio,
                                   .baseline = ROI.Baseline,
                                   .ID = getTitle(ROI, i),
                                   .integration = ROI.Integration,
                                   .maxInto = ROI.MaxInto,
                                   .ri = ROI.RetentionIndex(A, B),
                                   .rt = ROI.rt,
                                   .rtmax = ROI.Time.Max,
                                   .rtmin = ROI.Time.Min,
                                   .mass_spectra = base64
                               }
                           End Function) _
                   .ToArray
        End With
    End Function
End Module
