﻿Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.Signal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports list = SMRUCC.Rsharp.Runtime.Internal.Object.list

''' <summary>
''' helper package module for read ``electromagnetic radiation spectrum`` data
''' </summary>
''' 
<Package("mzML.ERS", Category:=APICategories.UtilityTools, Publisher:="gg.xie@bionovogene.com")>
Module ERS

    Sub New()

    End Sub

    ''' <summary>
    ''' Get photodiode array detector instrument configuration id
    ''' </summary>
    ''' <param name="mzml"></param>
    ''' <returns></returns>
    <ExportAPI("get_instrument")>
    Public Function GetPhotodiodeArrayDetectorInstrumentConfigurationId(mzml As String) As String
        Return ExtractUVData.GetPhotodiodeArrayDetectorInstrumentConfigurationId(rawdata:=mzml)
    End Function

    <ExportAPI("extract_UVsignals")>
    <RApiReturn(GetType(GeneralSignal))>
    Public Function ExtractERSUVData(<RRawVectorArgument> rawscans As Object, instrumentId As String, Optional env As Environment = Nothing) As Object
        Dim raw As pipeline = pipeline.TryCreatePipeline(Of spectrum)(rawscans, env)

        If raw.isError Then
            Return raw.getError
        End If

        Return raw _
            .populates(Of spectrum)(env) _
            .PopulatesElectromagneticRadiationSpectrum(instrumentId) _
            .DoCall(AddressOf pipeline.CreateFromPopulator)
    End Function

    <ExportAPI("as.UVtime_signals")>
    Public Function translateToTimeSignals(<RRawVectorArgument> rawscans As Object, Optional rawfile As String = "UVraw", Optional env As Environment = Nothing) As Object
        Dim raw As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(rawscans, env)

        If raw.isError Then
            Return raw.getError
        End If

        Return raw _
            .populates(Of GeneralSignal)(env) _
            .CreateTimeSignals(rawfile) _
            .DoCall(AddressOf pipeline.CreateFromPopulator)
    End Function

    ''' <summary>
    ''' write UV signal data into a text file or netCDF4 data file
    ''' </summary>
    ''' <param name="signals">a vector or pipeline of <see cref="GeneralSignal"/></param>
    ''' <param name="file">the file path of the data file that will be write signal data to it.</param>
    ''' <param name="enable_CDFextension">
    ''' only available for sciBASIC.NET product when this option is set to ``TRUE``. not supports for the 
    ''' standard netcdf library on linux platform or other cdf file reader software like NASA Panoply, etc. 
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write.UVsignals")>
    <RApiReturn(GetType(Boolean))>
    Public Function WriteSignal(<RRawVectorArgument> signals As Object, file$, Optional enable_CDFextension As Boolean = False, Optional env As Environment = Nothing) As Object
        Dim raw As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(signals, env)

        If raw.isError Then
            Return raw.getError
        End If

        If file.ExtensionSuffix("cdf", "netcdf", "nc") Then
            Call raw.populates(Of GeneralSignal)(env).WriteCDF(file, "electromagnetic radiation spectrum ERS_UVsignal", enable_CDFextension)
        Else
            ' write text
            Using writer As StreamWriter = file.OpenWriter
                For Each scan As GeneralSignal In raw.populates(Of GeneralSignal)(env)
                    Call writer.WriteLine(scan.GetText)
                Next
            End Using
        End If

        Return True
    End Function

    <ExportAPI("read.UVsignals")>
    Public Function ReadSignals(file As Object, Optional env As Environment = Nothing) As Object
        Dim filestream As [Variant](Of Stream, Message) = GetFileStream(file, FileAccess.Read, env)

        If filestream Like GetType(Message) Then
            Return filestream.TryCast(Of Message)
        End If

        Dim allSignals = New netCDFReader(filestream.TryCast(Of Stream)).ReadCDF.ToArray
        Dim list As New list With {.slots = New Dictionary(Of String, Object)}

        For Each signal As GeneralSignal In allSignals
            list.slots.Add(signal.reference, signal)
        Next

        Return list
    End Function
End Module
