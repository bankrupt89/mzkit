﻿#Region "Microsoft.VisualBasic::5f385079524925684213dea1ccc76cf2, Rscript\Library\mzkit.quantify\MRMkit.vb"

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

' Module MRMkit
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: ExtractIonData, ExtractPeakROI, GetLinearPoints, GetQuantifyResult, GetRawX
'               Linears, printIonPairs, printIS, printLineModel, printStandards
'               readCompoundReference, readIonPairs, readIS, SampleQuantify, ScanPeakTable
'               ScanWiffRaw, WiffRawFile, writeMRMpeaktable
' 
' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.ASCII.MSL
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Data
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Models
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rlist = SMRUCC.Rsharp.Runtime.Internal.Object.list
Imports RRuntime = SMRUCC.Rsharp.Runtime
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File

''' <summary>
''' MRM Targeted Metabolomics
''' </summary>
<Package("mzkit.mrm")>
Module MRMkit

    Public Class MRMDataSet

        Public StandardCurve As StandardCurve()
        Public Samples As QuantifyScan()

    End Class

    Sub New()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of StandardCurve)(AddressOf printLineModel)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of IonPair())(AddressOf printIonPairs)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Standards())(AddressOf printStandards)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of [IS]())(AddressOf printIS)

        ' create linear regression report
        REnv.Internal.htmlPrinter.AttachHtmlFormatter(Of StandardCurve())(AddressOf MRMLinearReport.CreateHtml)
        REnv.Internal.htmlPrinter.AttachHtmlFormatter(Of MRMDataSet)(AddressOf MRMLinearReport.CreateHtml)

        Dim toolkit As AssemblyInfo = GetType(MRMkit).Assembly.FromAssembly

        Call VBDebugger.WaitOutput()
        Call toolkit.AppSummary(Nothing, Nothing, App.StdOut)
    End Sub

    Private Function printStandards(obj As Object) As String
        Dim csv = DirectCast(obj, Standards()).ToCsvDoc.ToMatrix.RowIterator.ToArray
        Dim printContent = csv.Print(addBorder:=False)

        Return printContent
    End Function

    Private Function printIS(obj As Object) As String
        Dim csv = DirectCast(obj, [IS]()).ToCsvDoc.ToMatrix.RowIterator.ToArray
        Dim printContent = csv.Print(addBorder:=False)

        Return printContent
    End Function

    Private Function printIonPairs(obj As Object) As String
        Dim csv = DirectCast(obj, IonPair()).ToCsvDoc.ToMatrix.RowIterator.ToArray
        Dim printContent = csv.Print(addBorder:=False)

        Return printContent
    End Function

    Private Function printLineModel(line As Object) As String
        If line Is Nothing Then
            Return "NULL"
        Else
            With DirectCast(line, StandardCurve)
                Return $"{ .name}: { .linear.ToString}"
            End With
        End If
    End Function

    ''' <summary>
    ''' Extract ion peaks
    ''' </summary>
    ''' <param name="mzML">A mzML raw file</param>
    ''' <param name="ionpairs">metabolite targets</param>
    ''' <returns></returns>
    <ExportAPI("extract.ions")>
    Public Function ExtractIonData(mzML$, ionpairs As IonPair(), Optional tolerance$ = "ppm:20") As NamedCollection(Of ChromatogramTick)()
        Return MRMSamples.ExtractIonData(
            ion_pairs:=ionpairs,
            mzML:=mzML,
            assignName:=Function(i) i.accession,
            tolerance:=interop_arguments.GetTolerance(tolerance)
        )
    End Function

    ''' <summary>
    ''' Exact ``regions of interested`` based on the given ion pair as targets.
    ''' </summary>
    ''' <param name="mzML">A mzML raw data file</param>
    ''' <param name="ionpairs">MRM ion pairs</param>
    ''' <param name="TPAFactors">Peak factors</param>
    ''' <param name="baselineQuantile#"></param>
    ''' <param name="integratorTicks%"></param>
    ''' <param name="peakAreaMethod"></param>
    ''' <returns></returns>
    <ExportAPI("extract.peakROI")>
    Public Function ExtractPeakROI(mzML$, ionpairs As IonPair(),
                                   Optional tolerance$ = "ppm:20",
                                   Optional TPAFactors As Dictionary(Of String, Double) = Nothing,
                                   Optional baselineQuantile# = 0.65,
                                   Optional integratorTicks% = 5000,
                                   Optional peakAreaMethod As PeakArea.Methods = PeakArea.Methods.NetPeakSum) As IonTPA()
        If TPAFactors Is Nothing Then
            TPAFactors = New Dictionary(Of String, Double)
        End If

        Return ScanOfTPA.ScanTPA(
            raw:=mzML,
            ionpairs:=ionpairs,
            TPAFactors:=TPAFactors,
            tolerance:=interop_arguments.GetTolerance(tolerance),
            baselineQuantile:=baselineQuantile,
            integratorTicks:=integratorTicks,
            peakAreaMethod:=peakAreaMethod
        )
    End Function

    ''' <summary>
    ''' Get ion pair definition data from a given table file.
    ''' </summary>
    ''' <param name="file">A csv file or xlsx Excel data sheet</param>
    ''' <param name="sheetName">The sheet name in excel tables.</param>
    ''' <returns></returns>
    <ExportAPI("read.ion_pairs")>
    Public Function readIonPairs(file$, Optional sheetName$ = "Sheet1") As IonPair()
        If file.ExtensionSuffix("xlsx") Then
            Return Xlsx.Open(path:=file) _
                .GetTable(sheetName) _
                .AsDataSource(Of IonPair)(silent:=True) _
                .ToArray
        Else
            Return file.LoadCsv(Of IonPair)(mute:=True).ToArray
        End If
    End Function

    <ExportAPI("isomerism.ion_pairs")>
    Public Function IsomerismIonPairs(ions As IonPair(), Optional tolerance$ = "ppm:20") As IsomerismIonPairs()
        Return IonPair.GetIsomerism(ions, interop_arguments.GetTolerance(tolerance)).ToArray
    End Function

    <ExportAPI("as.ion_pairs")>
    <RApiReturn(GetType(IonPair()))>
    Public Function asIonPair(<RRawVectorArgument> mz As Object, Optional env As Environment = Nothing) As Object
        If mz Is Nothing Then
            Return Nothing
        End If

        Dim type As Type = mz.GetType

        If type.IsArray Then
            type = REnv.MeasureArrayElementType(mz)

            Select Case type
                Case GetType(MSLIon)
                    Return DirectCast(REnv.asVector(Of MSLIon)(mz), MSLIon()) _
                        .Select(Function(ion)
                                    Return New IonPair With {
                                        .accession = ion.Name,
                                        .name = ion.Name,
                                        .precursor = ion.MW,
                                        .product = ion.Peaks(Scan0).mz,
                                        .rt = ion.RT
                                    }
                                End Function) _
                        .ToArray
            End Select
        End If

        Return REnv.Internal.debug.stop(New NotImplementedException(mz.GetType.FullName), env)
    End Function

    ''' <summary>
    ''' Read reference points
    ''' </summary>
    ''' <param name="file$"></param>
    ''' <param name="sheetName$"></param>
    ''' <returns></returns>
    <ExportAPI("read.reference")>
    Public Function readCompoundReference(file$, Optional sheetName$ = "Sheet1") As Standards()
        Dim reference As Standards()

        If file.ExtensionSuffix("xlsx") Then
            reference = Xlsx.Open(path:=file) _
                .GetTable(sheetName) _
                .AsDataSource(Of Standards)(silent:=True) _
                .ToArray
        Else
            reference = file.LoadCsv(Of Standards)(mute:=True).ToArray
        End If

        Return reference
    End Function

    ''' <summary>
    ''' Read the definition of internal standards
    ''' </summary>
    ''' <param name="file">A csv file or xlsx file</param>
    ''' <param name="sheetName">
    ''' The sheet name that contains data of the IS data if the given file is a xlsx file.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.IS")>
    Public Function readIS(file$, Optional sheetName$ = "Sheet1", Optional env As Environment = Nothing) As [IS]()
        If file.ExtensionSuffix("xlsx") Then
            Dim table = Xlsx.Open(path:=file).GetTable(sheetName)

            If table Is Nothing Then
                ' probably no used of any IS for data calibration
                env.AddMessage($"No IS data was found in MRM information table file '{file.FileName}', where the sheet name is '{sheetName}'...", MSG_TYPES.WRN)
                Return {}
            Else
                Return table _
                    .AsDataSource(Of [IS])(silent:=True) _
                    .ToArray
            End If
        Else
            Return file.LoadCsv(Of [IS])(mute:=True).ToArray
        End If
    End Function

    ''' <summary>
    ''' Create model of the MRM raw files
    ''' </summary>
    ''' <param name="convertDir">
    ''' A directory data object for read MRM sample raw files. If the parameter value is
    ''' a ``list``, then it should contains at least two fields: ``samples`` and ``reference``.
    ''' The balnks raw data should be contains in reference files directory.
    ''' </param>
    ''' <param name="patternOfRef">File name pattern for filter reference data.</param>
    ''' <param name="patternOfBlank">File name pattern for filter blank controls.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("wiff.rawfiles")>
    Public Function WiffRawFile(<RRawVectorArgument>
                                convertDir As Object,
                                Optional patternOfRef$ = ".+[-]CAL[-]?\d+",
                                Optional patternOfBlank$ = "KB[-]?(\d+)?",
                                Optional env As Environment = Nothing) As Object

        If REnv.Internal.Invokes.isEmpty(convertDir) Then
            Return REnv.Internal.debug.stop("No raw files data provided!", env)
        End If

        Dim dataType As Type = convertDir.GetType

        If dataType Is GetType(String) Then
            Return New RawFile(convertDir, patternOfRef, patternOfBlank)
        ElseIf dataType Is GetType(String()) Then
            With DirectCast(convertDir, String())
                If .Length = 1 Then
                    Return New RawFile(.GetValue(Scan0), patternOfRef, patternOfBlank)
                Else
                    Return New RawFile(.GetValue(0), .GetValue(1), patternOfRef, patternOfBlank)
                End If
            End With
        ElseIf dataType Is GetType(Rlist) Then
            ' samples/reference
            With DirectCast(convertDir, Rlist)
                Dim samples As String = RRuntime.getFirst(!samples)
                Dim reference As String = RRuntime.getFirst(!reference)

                Return New RawFile(samples, reference, patternOfRef, patternOfBlank)
            End With
        Else
            Return Message.InCompatibleType(GetType(String()), dataType, env)
        End If
    End Function

    <ExportAPI("MRM.peaks")>
    Public Function ScanPeakTable(mzML$, ions As IonPair(),
                                  Optional peakAreaMethod As PeakArea.Methods = Methods.NetPeakSum,
                                  Optional tolerance$ = "ppm:20",
                                  Optional TPAFactors As Dictionary(Of String, Double) = Nothing) As DataSet()
        If TPAFactors Is Nothing Then
            TPAFactors = New Dictionary(Of String, Double)
        End If

        Return WiffRaw.ScanPeakTable(mzML, ions, interop_arguments.GetTolerance(tolerance), peakAreaMethod, TPAFactors)
    End Function

    ''' <summary>
    ''' # Scan the raw file data
    ''' 
    ''' Get the peak area data of the metabolites in each given sample 
    ''' data files
    ''' </summary>
    ''' <param name="wiffConverts">
    ''' A directory that contains the mzML files which are converts from 
    ''' the given wiff raw file.
    ''' </param>
    ''' <param name="ions">Ion pairs definition data.</param>
    ''' <param name="peakAreaMethod"></param>
    ''' <param name="TPAFactors"></param>
    ''' <param name="removesWiffName"></param>
    ''' <returns></returns>
    <ExportAPI("wiff.scans")>
    Public Function ScanWiffRaw(wiffConverts As String(), ions As IonPair(),
                                Optional peakAreaMethod As PeakArea.Methods = PeakArea.Methods.NetPeakSum,
                                Optional tolerance$ = "ppm:20",
                                Optional TPAFactors As Dictionary(Of String, Double) = Nothing,
                                Optional removesWiffName As Boolean = True) As DataSet()

        If TPAFactors Is Nothing Then
            TPAFactors = New Dictionary(Of String, Double)
        End If

        If wiffConverts Is Nothing Then
            Throw New ArgumentNullException(NameOf(wiffConverts))
        End If

        'If wiffConverts Is Nothing Then
        '    Throw New ArgumentNullException(NameOf(wiffConverts))
        'ElseIf RRuntime.isVector(Of String)(wiffConverts) Then
        '    Dim stringVec As Array = RRuntime.asVector(Of String)(wiffConverts)

        '    If stringVec.Length = 1 Then
        '        wiffConverts = stringVec.GetValue(Scan0) _
        '                .ToString _
        '                .ListFiles("*.mzML") _
        '                .ToArray _
        '                .DoCall(Function(files)
        '                            Return RawFile.WrapperForStandards(files, "CAL[-]?\d+")
        '                        End Function)
        '    Else
        '        wiffConverts = RawFile.WrapperForStandards(stringVec, "CAL[-]?\d+")
        '    End If
        'End If

        'Dim raw As RawFile = DirectCast(wiffConverts, RawFile)

        Return WiffRaw.Scan(
            mzMLRawFiles:=wiffConverts,
            ions:=ions,
            peakAreaMethod:=peakAreaMethod,
            TPAFactors:=TPAFactors,
            refName:=Nothing,
            removesWiffName:=removesWiffName,
            tolerance:=interop_arguments.GetTolerance(tolerance)
        )
    End Function

    ''' <summary>
    ''' Create linear fitting based on the wiff raw scan data.
    ''' </summary>
    ''' <param name="rawScan">The wiff raw scan data which are extract by function: ``wiff.scans``.</param>
    ''' <param name="calibrates"></param>
    ''' <param name="[ISvector]"></param>
    ''' <param name="autoWeighted">
    ''' If the unweighted R2 value of target standard curve is less than 0.99, 
    ''' then the quantify program will try weighted linear fitting. 
    ''' </param>
    ''' <param name="maxDeletions">
    ''' Max number of the reference points that delete automatically by 
    ''' the linear modelling program.
    ''' 
    ''' + negative value means auto
    ''' + zero means no deletion
    ''' + positive means the max allowed point numbers for auto deletion by the program
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 20200106 checked, test success
    ''' </remarks>
    <ExportAPI("linears")>
    Public Function Linears(rawScan As DataSet(), calibrates As Standards(), [ISvector] As [IS](),
                            Optional autoWeighted As Boolean = True,
                            Optional blankControls As DataSet() = Nothing,
                            Optional maxDeletions As Integer = 1,
                            Optional isWorkCurveMode As Boolean = True) As StandardCurve()

        Return rawScan.ToDictionary _
            .Regression(
                calibrates:=calibrates,
                ISvector:=ISvector,
                weighted:=autoWeighted,
                blankControls:=blankControls,
                maxDeletions:=maxDeletions,
                isWorkCurveMode:=isWorkCurveMode
            ) _
            .ToArray
    End Function

    ''' <summary>
    ''' Get reference input points
    ''' </summary>
    ''' <param name="linears"></param>
    ''' <param name="name">The metabolite id</param>
    ''' <returns></returns>
    <ExportAPI("points")>
    Public Function GetLinearPoints(linears As StandardCurve(), name$) As MRMStandards()
        Dim line As StandardCurve = linears _
            .Where(Function(l)
                       Return l.name = name
                   End Function) _
            .FirstOrDefault

        If line Is Nothing Then
            Return Nothing
        Else
            Return line.points
        End If
    End Function

    ''' <summary>
    ''' Do sample quantify
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file">The sample raw file its file path.</param>
    ''' <param name="ions"></param>
    ''' <param name="peakAreaMethod"></param>
    ''' <param name="TPAFactors"></param>
    ''' <returns></returns>
    <ExportAPI("sample.quantify")>
    Public Function SampleQuantify(model As StandardCurve(), file$, ions As IonPair(),
                                   Optional peakAreaMethod As PeakArea.Methods = Methods.NetPeakSum,
                                   Optional tolerance$ = "ppm:20",
                                   Optional TPAFactors As Dictionary(Of String, Double) = Nothing) As QuantifyScan

        Return MRMSamples.SampleQuantify(
            model:=model,
            file:=file,
            ions:=ions,
            tolerance:=interop_arguments.GetTolerance(tolerance),
            peakAreaMethod:=peakAreaMethod,
            TPAFactors:=TPAFactors
        )
    End Function

    ''' <summary>
    ''' Write peak data which is extract from the raw file with given ion pairs data
    ''' </summary>
    ''' <param name="MRMPeaks"></param>
    ''' <param name="file">The output csv file path</param>
    ''' <returns></returns>
    <ExportAPI("write.MRMpeaks")>
    Public Function writeMRMpeaktable(MRMPeaks As MRMPeakTable(), file$) As Boolean
        Return MRMPeaks.SaveTo(file, silent:=True)
    End Function

    <ExportAPI("lines.table")>
    Public Function StandardCurveDataSet(lines As StandardCurve()) As EntityObject()
        Return lines _
            .Select(Function(line)
                        Return New EntityObject With {
                            .ID = line.name,
                            .Properties = New Dictionary(Of String, String) From {
                                {"name", line.points(Scan0).Name},
                                {"equation", "f(x)=" & line.linear.Polynomial.ToString("G5", False)},
                                {"R2", line.linear.CorrelationCoefficient},
                                {"is.weighted", line.isWeighted},
                                {"IS.calibration", line.requireISCalibration},
                                {"IS", line.IS.name}
                            }
                        }
                    End Function) _
            .ToArray
    End Function

    <ExportAPI("write.points")>
    Public Function writeStandardCurve(points As MRMStandards(), file$) As Boolean
        Return points.SaveTo(file, silent:=True)
    End Function

    ''' <summary>
    ''' Get quantify result
    ''' </summary>
    ''' <param name="fileScans"></param>
    ''' <returns></returns>
    <ExportAPI("result")>
    Public Function GetQuantifyResult(fileScans As QuantifyScan()) As DataSet()
        Return fileScans.Select(Function(file) file.quantify).ToArray
    End Function

    ''' <summary>
    ''' Get result of ``AIS/At``
    ''' </summary>
    ''' <param name="fileScans"></param>
    ''' <returns></returns>
    <ExportAPI("scans.X")>
    Public Function GetRawX(fileScans As QuantifyScan()) As DataSet()
        Return fileScans.Select(Function(file) file.rawX).ToArray
    End Function

    ''' <summary>
    ''' Create MRM dataset object for do MRM quantification data report.
    ''' </summary>
    ''' <param name="standardCurve"></param>
    ''' <param name="samples"></param>
    ''' <returns></returns>
    <ExportAPI("mrm.dataset")>
    Public Function CreateMRMDataSet(standardCurve As StandardCurve(), samples As QuantifyScan()) As MRMDataSet
        Return New MRMDataSet With {
            .StandardCurve = standardCurve,
            .Samples = samples
        }
    End Function
End Module
