﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.MassSpectrum.Assembly.MarkupData.mzML
Imports SMRUCC.MassSpectrum.Math

Public Module ChromatogramPlot

    ''' <summary>
    ''' 从mzML文件之中解析出色谱数据之后，将所有的色谱峰都绘制在一张图之中进行可视化
    ''' </summary>
    ''' <param name="mzML$"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="bg$"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Plot(ions As IonPair(),
                         mzML$,
                         Optional size$ = "1600,900",
                         Optional margin$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional colorsSchema$ = "scibasic.category31()",
                         Optional penStyle$ = Stroke.ScatterLineStroke,
                         Optional labelFontStyle$ = CSSFont.Win7Normal,
                         Optional labelConnectorStroke$ = Stroke.StrongHighlightStroke) As GraphicsData

        Dim labelFont As Font = CSSFont.TryParse(labelFontStyle)
        Dim labelConnector As Pen = Stroke.TryParse(labelConnectorStroke)
        Dim ionData = LoadChromatogramList(mzML) _
            .MRMSelector(ions) _
            .Where(Function(ion) Not ion.chromatogram Is Nothing) _
            .Select(Function(ion)
                        Return New NamedCollection(Of ChromatogramTick) With {
                            .Name = ion.ion.name,
                            .Description = ion.ion.ToString,
                            .Value = ion.chromatogram.PeakArea
                        }
                    End Function) _
            .ToArray

        For Each ion As NamedCollection(Of ChromatogramTick) In ionData
            Dim base = ion.Value.Base(quantile:=0.65)
            Dim max# = ion.Value.Shadows!Intensity.Max

            Call $"{ion.Name}: {base}/{max} = {(100 * base / max).ToString("F2")}%".__DEBUG_ECHO
        Next

        Dim colors As Pen() = Designer _
            .GetColors(colorsSchema) _
            .Select(Function(c)
                        Dim style As Stroke = Stroke.TryParse(penStyle)
                        style.fill = c.ARGBExpression
                        Return style.GDIObject
                    End Function) _
            .ToArray
        Dim XTicks = ionData _
            .Select(Function(ion)
                        Return ion.Value.TimeArray
                    End Function) _
            .IteratesALL _
            .AsVector _
            .Range _
            .CreateAxisTicks  ' time
        Dim YTicks = ionData _
            .Select(Function(ion)
                        Return ion.Value.IntensityArray
                    End Function) _
            .IteratesALL _
            .AsVector _
            .Range _
            .CreateAxisTicks ' intensity

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim rect As Rectangle = region.PlotRegion
                Dim X = d3js.scale.linear.domain(XTicks).range(integers:={rect.Left, rect.Right})
                Dim Y = d3js.scale.linear.domain(YTicks).range(integers:={rect.Top, rect.Bottom})
                Dim scaler As New DataScaler With {
                    .AxisTicks = (XTicks, YTicks),
                    .Region = rect,
                    .X = X,
                    .Y = Y
                }

                Call g.DrawAxis(
                    region, scaler, showGrid:=False,
                    xlabel:="Time (s)",
                    ylabel:="Intensity",
                    htmlLabel:=False,
                    YtickFormat:="G2"
                )

                Dim legendColors As New List(Of NamedValue(Of Pen))
                Dim peakTimes As New List(Of NamedValue(Of ChromatogramTick))

                For i As Integer = 0 To ionData.Length - 1
                    Dim curvePen As Pen = colors(i)
                    Dim line = ionData(i)
                    Dim chromatogram = line.Value

                    legendColors += New NamedValue(Of Pen) With {
                        .Name = line.Name,
                        .Value = curvePen
                    }
                    peakTimes += New NamedValue(Of ChromatogramTick) With {
                        .Name = line.Name,
                        .Value = chromatogram(Which.Max(chromatogram.Shadows!Intensity))
                    }

                    Dim A, B As PointF

                    For Each signal As SlideWindow(Of PointF) In chromatogram _
                        .Select(Function(c)
                                    Return New PointF(c.Time, c.Intensity)
                                End Function) _
                        .SlideWindows(slideWindowSize:=2)

                        A = scaler.Translate(signal.First)
                        B = scaler.Translate(signal.Last)

                        Call g.DrawLine(curvePen, A, B)
                    Next
                Next

                ' labeling 
                Dim canvas = g
                Dim labels As Label() = peakTimes _
                    .Select(Function(ion)
                                Dim labelSize As SizeF = canvas.MeasureString(ion.Name, labelFont)
                                Dim location As PointF = scaler.Translate(ion.Value)

                                Return New Label With {
                                    .height = labelSize.Height,
                                    .width = labelSize.Width,
                                    .text = ion.Name,
                                    .X = location.X,
                                    .Y = location.Y
                                }
                            End Function) _
                    .ToArray
                Dim anchors As Anchor() = labels.GetLabelAnchors(r:=3)

                Call d3js.labeler _
                    .Labels(labels) _
                    .Anchors(anchors) _
                    .Width(rect.Width) _
                    .Height(rect.Height) _
                    .Start(showProgress:=False)

                For Each i As SeqValue(Of Label) In labels.SeqIterator
                    Call g.DrawLine(labelConnector, i.value, anchors(i))
                    Call g.DrawString(i.value.text, labelFont, Brushes.Black, i.value)
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotInternal)
    End Function
End Module