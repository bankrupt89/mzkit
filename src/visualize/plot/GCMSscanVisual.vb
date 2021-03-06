﻿#Region "Microsoft.VisualBasic::0037f75575db4d81aca42d7cfde3dd57, src\visualize\plot\GCMSscanVisual.vb"

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

' Module GCMSscanVisual
' 
'     Function: PlotScans
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.GCMS
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module GCMSscanVisual

    ''' <summary>
    ''' 这个是使用三维图表的方式进行绘制GC_MS的实验数据
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PlotScans(data As Raw,
                              Optional size$ = "8000,5000",
                              Optional padding$ = g.DefaultPadding,
                              Optional bg$ = "white",
                              Optional colors$ = "clusters",
                              Optional massFragmentStrokeCSS$ = "stroke: skyblue; stroke-width: 5px; stroke-dash: solid;",
                              Optional axisLabelFontCSS$ = CSSFont.Win7VeryVeryLargeNormal,
                              Optional axisStrokeCss$ = Stroke.AxisStroke,
                              Optional arrowFactor$ = "2,2",
                              Optional sn_threshold# = 5,
                              Optional angleCutoff# = 6,
                              Optional peakwidth As DoubleRange = Nothing,
                              Optional viewDistance% = 14000,
                              Optional viewAngle$ = "-90,-90,-30",
                              Optional fov% = 800) As GraphicsData

        Dim camera As New Camera(viewAngle) With {
            .fov = fov,
            .screen = size.SizeParser,
            .viewDistance = viewDistance
        }
        Dim plotRegion As New GraphicsRegion With {
            .Size = camera.screen,
            .Padding = padding
        }
        Dim ROIlist As ROI() = data _
            .ExportROI(angle:=angleCutoff, peakwidth:=peakwidth) _
            .Where(Function(region)
                       Return region.snRatio >= sn_threshold
                   End Function) _
            .ToArray
        '
        ' intensity
        '  ^ y   z
        '  |   / m/z
        '  |  /  
        '  | /
        '  |/          retention time
        '  ------------------> X 
        Dim X, Y, Z As Vector

        X = data.times.Range.CreateAxisTicks ' 时间
        Y = data.tic.Range.CreateAxisTicks   ' TIC
        ' Z axis is mass scans
        Z = data.ms _
            .Select(Function(i)
                        Return i.Select(Function(scan) -scan.mz)
                    End Function) _
            .IteratesALL _
            .Range _
            .CreateAxisTicks

        ' X
        Dim timeScaler As New YScaler(reversed:=False) With {
            .region = plotRegion.PlotRegion,
            .Y = d3js.scale.linear _
                .domain(X) _
                .range(integers:={0, plotRegion.Width})
        }
        ' Y
        ' 因为intensity可能会达到上十万
        ' 与几百秒放在一起会非常不协调
        ' 所以所有的intensity值都会需要经过这个对象进行比例缩放
        Dim intensityScaler As New YScaler(reversed:=False) With {
            .region = plotRegion.PlotRegion,
            .Y = d3js.scale.linear _
                .domain(Y) _
                .range(integers:={0, plotRegion.Height})
        }
        ' Z
        Dim massScaler As New YScaler(reversed:=False) With {
            .region = plotRegion.PlotRegion,
            .Y = d3js.scale.linear _
                .domain(Z) _
                .range(integers:={0, plotRegion.Height})
        }

        Dim model As New List(Of Element3D)
        Dim TICArea As New Polygon With {
            .brush = Brushes.Yellow,
            .Path = data _
                .times _
                .Select(Function(time, i)
                            Return New Point3D With {
                                .X = timeScaler.TranslateY(time),
                                .Y = intensityScaler.TranslateY(data.tic(i)),
                                .Z = massScaler.TranslateY(0)
                            }
                        End Function) _
                .ToArray
        }

        X = timeScaler.TranslateY(X).ToArray
        ' Y intensity 坐标轴需要重新scale一下
        ' 重新缩放到可以绘制的范围内
        Y = intensityScaler.TranslateY(Y).ToArray
        Z = massScaler.TranslateY(Z).ToArray

        ' 添加坐标轴模型
        model += AxisDraw.Axis(
            X, Y, Z, CSSFont.TryParse(axisLabelFontCSS),
            ("retention time(s)", "intensity", "m/z"),
            axisStrokeCss,
            arrowFactor
        )
        ' 添加TIC曲线多边形
        model += TICArea

        Dim axisStroke As Pen = Stroke.TryParse(axisStrokeCss)
        Dim massFragmentStroke As Pen = Stroke.TryParse(massFragmentStrokeCSS)
        Dim massZ#

        ' 添加ms scan信号柱模型
        For Each region As ROI In ROIlist

            ' 每一个ROI的ms scan的X都是相同的
            Dim rtX# = timeScaler.TranslateY(region.rt)
            Dim msScans = data.GetMsScan(region).GroupByMz().CreateLibraryMatrix.Trim(0.05)
            Dim A As New Point3D(rtX, Y.Min, Z.Min)
            Dim B As New Point3D(rtX, Y.Min, Z.Max)

            ' 添加绘制基础的线轴的模型
            model += New Line(A, B) With {
                .Stroke = axisStroke
            }

            For Each mz As ms2 In msScans
                massZ = massScaler.TranslateY(-mz.mz)
                A = New Point3D(rtX, Y.Min, massZ)
                B = New Point3D(rtX, intensityScaler.TranslateY(mz.intensity), massZ)

                model += New Line(A, B) With {
                    .Stroke = massFragmentStroke
                }
            Next
        Next

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                ' 要先绘制三维图形，要不然会将图例遮住的
                Call model.RenderAs3DChart(g, camera, region, Nothing)
            End Sub

        Return plotRegion _
            .Size _
            .GraphicsPlots(padding, bg, plotInternal)
    End Function
End Module
