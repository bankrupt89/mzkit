﻿Imports Microsoft.VisualBasic.Text
Imports SMRUCC.MassSpectrum.DATA.MetaLib

Namespace NCBI.PubChem

    ''' <summary>
    ''' These are listings of all names associated with a CID. The
    ''' unfiltered list are names aggregated from all SIDs whose 
    ''' standardized form Is that CID, sorted by weight With the "best"
    ''' names first. The filtered list has some names removed that are
    ''' considered inconsistend With the Structure. Both are gzipped text
    ''' files with CID, tab, And name on each line. Note that the
    ''' names may be composed Of more than one word, separated by spaces.
    ''' </summary>
    ''' <remarks>Data for ``CID-Synonym-filtered.gz`` file</remarks>
    Public Class CIDSynonym

        Public Property CID As String
        Public Property Synonym As String

        Public ReadOnly Property IsChEBI As Boolean
            Get
                Return xref.IsChEBI(Synonym)
            End Get
        End Property

        Public ReadOnly Property IsCAS As Boolean
            Get
                Return xref.IsCAS(Synonym)
            End Get
        End Property

        Public ReadOnly Property IsHMDB As Boolean
            Get
                Return xref.IsHMDB(Synonym)
            End Get
        End Property

        Public ReadOnly Property IsKEGG As Boolean
            Get
                Return xref.IsKEGG(Synonym)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Synonym
        End Function

        Public Shared Iterator Function LoadNames(file As String) As IEnumerable(Of CIDSynonym)
            Dim cid$ = 0
            Dim name As String
            Dim t As String()

            ' 第一个名称是权重最好的名称
            For Each line As String In file.IterateAllLines
                t = line.Split(ASCII.TAB)
                name = t(1)

                If t(0) <> cid Then
                    ' 这个是新的物质行
                    ' 第一个名称是权重最高的名称, 直接返回
                    Yield New CIDSynonym With {
                        .CID = t(0),
                        .Synonym = name
                    }

                    cid = t(0)
                Else
                    ' 只返回kegg/hmdb/chebi/cas编号名称
                    If xref.IsCAS(name) OrElse
                        xref.IsChEBI(name) OrElse
                        xref.IsHMDB(name) OrElse
                        xref.IsKEGG(name) Then

                        Yield New CIDSynonym With {
                            .CID = cid,
                            .Synonym = name
                        }
                    End If
                End If
            Next
        End Function
    End Class
End Namespace