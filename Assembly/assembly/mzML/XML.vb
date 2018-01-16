﻿Imports System.Xml.Serialization

Namespace mzML

    Public Class Xml
        Public Property mzML As mzML
        Public Property indexList As indexList
        Public Property indexListOffset As Long
        Public Property fileChecksum As String
    End Class

    Public Class indexList : Inherits List
        <XmlElement(NameOf(index))>
        Public Property index As Index()
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
    End Class

    <XmlType(NameOf(mzML), [Namespace]:="")>
    Public Class mzML
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

    Public Structure cv
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property fullName As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property URI As String
    End Structure

    Public Class chromatogramList : Inherits List
        <XmlAttribute>
        Public Property defaultDataProcessingRef As String
        <XmlElement(NameOf(chromatogram))>
        Public Property list As chromatogram()
    End Class

    Public Class chromatogram : Inherits Params

        <XmlAttribute> Public Property index As String
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property defaultArrayLength As String

        Public Property binaryDataArrayList As binaryDataArrayList
        Public Property precursor As precursor
        Public Property product As product

    End Class

    Public Class precursor
        Public Property isolationWindow As Params
        Public Property activation As Params
    End Class

    Public Class product
        Public Property isolationWindow As Params
        Public Property activation As Params
    End Class

    Public Class Params
        <XmlElement(NameOf(cvParam))>
        Public Property cvParams As cvParam()
        <XmlElement(NameOf(userParam))>
        Public Property userParams As userParam()
    End Class

    Public Class userParam
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property value As String
        <XmlAttribute> Public Property type As String
    End Class

    Public Class binaryDataArrayList : Inherits List
        <XmlElement(NameOf(binaryDataArray))>
        Public Property list As binaryDataArray()
    End Class

    Public Class binaryDataArray
        Public Property encodedLength As Integer
        <XmlElement(NameOf(cvParam))>
        Public Property cvParams As cvParam()
        Public Property binary As String
    End Class

    Public Class cvParam
        <XmlAttribute> Public Property cvRef As String
        <XmlAttribute> Public Property accession As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property value As String
    End Class

    Public Class run
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property defaultInstrumentConfigurationRef As String
        <XmlAttribute> Public Property startTimeStamp As String
        <XmlAttribute> Public Property defaultSourceFileRef As String

        Public Property chromatogramList As chromatogramList
    End Class
End Namespace