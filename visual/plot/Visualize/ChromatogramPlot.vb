﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.MassSpectrum.Math

''' <summary>
''' time -> into
''' </summary>
Public Module ChromatogramPlot

    Public Const DefaultPadding$ = "padding: 350px 100px 250px 200px"

    <Extension>
    Public Function Plot(chromatogram As ChromatogramTick(),
                         Optional size$ = "2100,1600",
                         Optional padding$ = DefaultPadding,
                         Optional bg$ = "white",
                         Optional title$ = "NULL",
                         Optional curveStyle$ = Stroke.AxisStroke,
                         Optional titleFontCSS$ = CSSFont.Win7VeryLarge) As GraphicsData

        Dim timeTicks#() = chromatogram.TimeArray.CreateAxisTicks
        Dim intoTicks#() = chromatogram.IntensityArray.CreateAxisTicks
        Dim curvePen As Pen = Stroke.TryParse(curveStyle).GDIObject
        Dim titleFont As Font = CSSFont.TryParse(titleFontCSS)
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim rect As Rectangle = region.PlotRegion
                Dim X = d3js.scale.linear.domain(timeTicks).range(integers:={rect.Left, rect.Right})
                Dim Y = d3js.scale.linear.domain(intoTicks).range(integers:={rect.Top, rect.Bottom})
                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .Region = rect,
                    .AxisTicks = (timeTicks, intoTicks)
                }
                Dim ZERO = scaler.TranslateY(0)

                Call g.DrawAxis(
                    region, scaler, showGrid:=False,
                    xlabel:="Time (s)",
                    ylabel:="Intensity",
                    htmlLabel:=False
                )

                For Each signal As SlideWindow(Of PointF) In chromatogram _
                    .Select(Function(c)
                                Return New PointF(c.Time, c.Intensity)
                            End Function) _
                    .SlideWindows(slideWindowSize:=2)

                    Dim A = scaler.Translate(signal.First)
                    Dim B = scaler.Translate(signal.Last)

                    Call g.DrawLine(curvePen, A, B)
                Next

                Dim left = rect.Left + (rect.Width - g.MeasureString(title, titleFont).Width) / 2
                Dim top = rect.Top + 10

                Call g.DrawString(title, titleFont, Brushes.Black, left, top)
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding,
            bg,
            plotInternal)
    End Function
End Module
