﻿#Region "Microsoft.VisualBasic::2fe96072e6fb607f07b7d9f14337147b, src\mzmath\TargetedMetabolomics\QuantitativeAnalysis\TPAExtensions.vb"

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

    ' Class IonTPA
    ' 
    '     Properties: area, baseline, maxPeakHeight, name, peakROI
    ' 
    '     Function: ToString
    ' 
    ' Module TPAExtensions
    ' 
    '     Function: ionTPA, TPAIntegrator
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Models
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Scripting
Imports stdNum = System.Math

''' <summary>
''' ROI Peak data of the given ion
''' </summary>
Public Class IonTPA

    Public Property name As String
    Public Property peakROI As DoubleRange
    Public Property area As Double
    Public Property baseline As Double
    Public Property maxPeakHeight As Double

    Public Overrides Function ToString() As String
        Return $"{name}[{peakROI.Min}, {stdNum.Round(peakROI.Max)}] = {area}"
    End Function

End Class

Public Module TPAExtensions

    ''' <summary>
    ''' 对某一个色谱区域进行峰面积的积分计算
    ''' </summary>
    ''' <param name="ion"></param>
    ''' <param name="baselineQuantile#"></param>
    ''' <param name="peakAreaMethod"></param>
    ''' <param name="integratorTicks%"></param>
    ''' <param name="TPAFactor">
    ''' ``{<see cref="Standards.ID"/>, <see cref="Standards.Factor"/>}``，这个是为了计算亮氨酸和异亮氨酸这类无法被区分的物质的峰面积所需要的
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function ionTPA(ion As NamedCollection(Of ChromatogramTick),
                           baselineQuantile#,
                           peakAreaMethod As PeakArea.Methods,
                           Optional integratorTicks% = 5000,
                           Optional TPAFactor# = 1) As IonTPA

        Dim vector As IVector(Of ChromatogramTick) = ion.value.Shadows
        Dim ROIData = vector _
            .PopulateROI _
            .OrderByDescending(Function(ROI)
                                   ' 这个积分值只是用来查找最大的峰面积的ROI区域
                                   ' 并不是最后的峰面积结果
                                   ' 还需要在下面的代码之中做峰面积积分才可以得到最终的结果
                                   Return ROI.Integration
                               End Function) _
            .ToArray


        If ROIData.Length = 0 Then
            Return New IonTPA With {
                .name = ion.name,
                .peakROI = New DoubleRange(0, 0)
            }
        Else
            Dim peak As DoubleRange = ROIData _
                .First _
                .Time ' .MRMPeak(baselineQuantile:=baselineQuantile)
            Dim data As (peak As DoubleRange, area#, baseline#, maxPeakHeight#)

            With vector.TPAIntegrator(peak, baselineQuantile, peakAreaMethod, integratorTicks, TPAFactor)
                data = (peak, .Item1, .Item2, .Item3)
            End With

            Return New IonTPA With {
                .name = ion.name,
                .peakROI = data.peak,
                .area = data.area,
                .baseline = data.baseline,
                .maxPeakHeight = data.maxPeakHeight
            }
        End If
    End Function

    ''' <summary>
    ''' 对某一个色谱区域进行峰面积的积分计算
    ''' </summary>
    ''' <param name="vector">完整的色谱图，计算基线的时候会需要使用到的</param>
    ''' <param name="peak">
    ''' 目标峰的时间范围
    ''' </param>
    ''' <param name="baselineQuantile">推荐0.65</param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function TPAIntegrator(vector As IVector(Of ChromatogramTick), peak As DoubleRange,
                                  baselineQuantile#,
                                  Optional peakAreaMethod As PeakArea.Methods = PeakArea.Methods.Integrator,
                                  Optional resolution% = 5000,
                                  Optional TPAFactor# = 1) As (area#, baseline#, maxPeakHeight#)
        Dim area#
        Dim baseline# = vector.Baseline(quantile:=baselineQuantile)

        Select Case peakAreaMethod
            Case Methods.NetPeakSum
                area = vector.PeakArea(peak, baseline:=baselineQuantile)
            Case Methods.SumAll
                area = vector.SumAll
            Case Methods.MaxPeakHeight
                area = vector.MaxPeakHeight
            Case Else
                ' 默认是使用积分器方法
                area = vector.PeakAreaIntegrator(
                    peak:=peak,
                    baselineQuantile:=baselineQuantile,
                    resolution:=resolution
                )
        End Select

        area *= TPAFactor

        Return (area, baseline, vector.PickArea(range:=peak).MaxPeakHeight)
    End Function
End Module