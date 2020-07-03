﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.Linq

Namespace MarkupData.mzML

    ''' <summary>
    ''' helper module for read UV data
    ''' </summary>
    Public Module ExtractUVData

        ''' <summary>
        ''' electromagnetic radiation spectrum
        ''' </summary>
        ''' <param name="instrumentConfigurationId">
        ''' <see cref="GetPhotodiodeArrayDetectorInstrumentConfigurationId"/>
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function PopulatesElectromagneticRadiationSpectrum(spectrums As IEnumerable(Of spectrum), instrumentConfigurationId As String) As IEnumerable(Of GeneralSignal)
            For Each rawScan As spectrum In spectrums
                If rawScan.scanList.scans.Any(Function(scan) scan.instrumentConfigurationRef = instrumentConfigurationId) Then
                    Yield rawScan.CreateGeneralSignal(instrumentConfigurationId)
                End If
            Next
        End Function

        <Extension>
        Private Function CreateGeneralSignal(rawScan As spectrum, instrumentConfigurationId As String) As GeneralSignal
            Const type As String = "electromagnetic radiation spectrum"

            Dim descriptor As Double() = rawScan.binaryDataArrayList.list(Scan0).Base64Decode
            Dim intensity As Double() = rawScan.binaryDataArrayList.list(1).Base64Decode
            Dim info As New Dictionary(Of String, String)
            Dim UVscan As scan = rawScan.scanList.scans.First(Function(a) a.instrumentConfigurationRef = instrumentConfigurationId)

            info.Add("total_ion_current", rawScan.cvParams.KeyItem("total ion current")?.value)
            info.Add("lowest_wavelength", rawScan.cvParams.KeyItem("lowest observed wavelength")?.value)
            info.Add("highest_wavelength", rawScan.cvParams.KeyItem("highest observed wavelength")?.value)
            info.Add("title", XmlEntity.UnescapingXmlEntity(rawScan.cvParams.KeyItem("spectrum title")?.value))
            info.Add("scan_time", Val(UVscan.cvParams.KeyItem("scan start time")?.value) * 60)

            Return New GeneralSignal With {
                .description = type,
                .meta = info,
                .measureUnit = "wavelength(nanometer)",
                .Measures = descriptor,
                .Strength = intensity
            }
        End Function

        Public Function GetPhotodiodeArrayDetectorInstrumentConfigurationId(rawdata As String) As String
            For Each configuration As instrumentConfiguration In rawdata.LoadXmlDataSet(Of instrumentConfiguration)(, xmlns:=Xml.xmlns)
                If configuration.componentList.detector.Any(Function(dev) dev.cvParams.Any(Function(a) a.name = "photodiode array detector")) Then
                    Return configuration.id
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace