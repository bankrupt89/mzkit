﻿Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Math
Imports SMRUCC.MassSpectrum.Math.Chromatogram
Imports SMRUCC.MassSpectrum.Math.Ms1
Imports SMRUCC.MassSpectrum.Visualization

<CLI> Module CLI

    <ExportAPI("/TIC")>
    <Usage("/TIC /in <data.csv> [/out <plot.png>]")>
    <Description("Do TIC plot based on the given chromatogram table data.")>
    <Argument("/in", False, CLITypes.File,
              AcceptTypes:={GetType(TICPoint)},
              Extensions:="*.csv",
              Description:="The mzXML dump data.")>
    Public Function TICplot(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.TIC.png"
        Dim da03 = Tolerance.DeltaMass(0.3)
        Dim data = [in].LoadCsv(Of TICPoint) _
            .GroupBy(Function(p) p.mz, Function(a, b) True = da03(a, b)) _
            .AsParallel _
            .Select(Function(ion)
                        Return New NamedCollection(Of ChromatogramTick) With {
                            .name = $"m/z {ion.First.mz.ToString("F4")}",
                            .value = ion _
                                .Select(Function(t)
                                            Return New ChromatogramTick With {
                                                .Time = t.time,
                                                .Intensity = t.intensity
                                            }
                                        End Function) _
                                .OrderBy(Function(p) p.Time) _
                                .ToArray
                        }
                    End Function) _
            .ToArray

        Call "Do TIC plot...".__INFO_ECHO

        Return data.TICplot(
            showLabels:=False,
            showLegends:=False,
            fillCurve:=False
        ).Save(out) _
         .CLICode
    End Function
End Module
