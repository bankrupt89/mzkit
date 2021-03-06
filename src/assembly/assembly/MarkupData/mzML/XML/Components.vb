﻿
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language.Default

Namespace MarkupData.mzML

    Public Class indexList : Inherits List
        <XmlElement(NameOf(index))>
        Public Property index As index()
    End Class

    Public Class index
        <XmlAttribute>
        Public Property name As String
        <XmlElement(NameOf(offset))>
        Public Property offsets As offset()
    End Class

    Public Class offset
        <XmlAttribute>
        Public Property idRef As String
        <XmlText>
        Public Property value As Long

        Public Overrides Function ToString() As String
            Return $"{idRef}: {value}"
        End Function
    End Class

    <XmlType(NameOf(mzML), [Namespace]:=mzML.Xmlns)>
    Public Class mzML

        Public Const Xmlns$ = Extensions.Xmlns

        Public Property cvList As cvList
        Public Property run As run

    End Class

    Public Class cvList : Inherits List

        <XmlElement(NameOf(cv))>
        Public Property list As cv()
    End Class

    Public Class List
        <XmlAttribute> Public Property count As Integer
    End Class

    Public Class DataList : Inherits List
        <XmlAttribute>
        Public Property defaultDataProcessingRef As String
    End Class

    Public Structure cv
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property fullName As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property URI As String
    End Structure

    Public Class Params

        <XmlElement(NameOf(cvParam))>
        Public Property cvParams As cvParam()

        <XmlElement(NameOf(userParam))>
        Public Property userParams As userParam()

    End Class

    Public Class userParam : Implements INamedValue

        <XmlAttribute> Public Property name As String Implements IKeyedEntity(Of String).Key
        <XmlAttribute> Public Property value As String
        <XmlAttribute> Public Property type As String

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type} = {value}"
        End Function
    End Class

    Public Class cvParam : Implements INamedValue

        <XmlAttribute> Public Property cvRef As String
        <XmlAttribute> Public Property accession As String
        <XmlAttribute> Public Property name As String Implements IKeyedEntity(Of String).Key
        <XmlAttribute> Public Property value As String
        <XmlAttribute> Public Property unitName As String
        <XmlAttribute> Public Property unitCvRef As String
        <XmlAttribute> Public Property unitAccession As String

        Shared ReadOnly Unknown As [Default](Of String) = NameOf(Unknown)

        Public Overrides Function ToString() As String
            Return $"[{accession}] Dim {name} As <{unitName Or Unknown}> = {value}"
        End Function
    End Class

    Public Class run
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property defaultInstrumentConfigurationRef As String
        <XmlAttribute> Public Property startTimeStamp As String
        <XmlAttribute> Public Property defaultSourceFileRef As String

        Public Property chromatogramList As chromatogramList
        Public Property spectrumList As spectrumList
    End Class
End Namespace