﻿
Imports System.Drawing
Imports System.Threading
Imports BioNovoGene.BioDeep.Chemistry.MetaLib.Models
Imports BioNovoGene.BioDeep.Chemistry.NCBI
Imports BioNovoGene.BioDeep.Chemistry.NCBI.PubChem
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("pubchem_kit")>
Module PubChemToolKit

    <ExportAPI("image_fly")>
    Public Function ImageFlyGetImages(<RRawVectorArgument>
                                      cid As Object,
                                      <RRawVectorArgument>
                                      Optional size As Object = "500,500",
                                      Optional ignoresInvalidCid As Boolean = False,
                                      Optional env As Environment = Nothing) As Object

        Dim ids As String() = REnv.asVector(Of String)(cid)
        Dim invalids = ids.Where(Function(id) Not id.IsPattern("\d+")).ToArray
        Dim images As New list
        Dim sizeVector As Double()

        If TypeOf size Is String OrElse TypeOf size Is String() Then
            With DirectCast(REnv.asVector(Of String)(size), String()).First.SizeParser
                sizeVector = { .Width, .Height}
            End With
        ElseIf TypeOf size Is Double() Then
            sizeVector = size
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(Double), size.GetType, env), env)
        End If

        Dim img As Image

        For Each id As String In ids
            img = ImageFly.GetImage(id, sizeVector(0), sizeVector(1), doBgTransparent:=False)

            Call Thread.Sleep(1000)
            Call images.slots.Add(id, img)
        Next

        Return images
    End Function

    ''' <summary>
    ''' query of the pubchem database
    ''' </summary>
    ''' <param name="id"></param>
    ''' <param name="cache$"></param>
    ''' <returns></returns>
    <ExportAPI("query")>
    Public Function queryPubChem(<RRawVectorArgument> id As Object, Optional cache$ = "./", Optional env As Environment = Nothing) As list
        Dim ids As String() = REnv.asVector(Of String)(id)
        Dim cid As String()
        Dim query As New Dictionary(Of String, PugViewRecord)
        Dim result As New list With {
            .slots = New Dictionary(Of String, Object)
        }
        Dim meta As Dictionary(Of String, MetaLib)

        For Each term As String In ids.Distinct.ToArray
            query = PubChem.QueryPugViews(term, cacheFolder:=cache)
            cid = query.Keys.ToArray
            meta = query _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  If a.Value Is Nothing Then
                                      Return Nothing
                                  Else
                                      Return a.Value.GetMetaInfo
                                  End If
                              End Function)

            Call result.slots.Add(term, meta)
        Next

        Return result
    End Function
End Module
