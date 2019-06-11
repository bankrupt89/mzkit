﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.foundation.OBO_Foundry
Imports SMRUCC.genomics.foundation.OBO_Foundry.IO.Models
Imports SMRUCC.genomics.foundation.OBO_Foundry.Tree

Public Class ChemOntClassify

    ReadOnly oboNodeList As GenericTree()
    ReadOnly oboTable As Dictionary(Of String, GenericTree)

    ''' <summary>
    ''' level 1
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property kingdom As GenericTree()
        Get
            Return termsByLevel(1)
        End Get
    End Property

    ''' <summary>
    ''' level 2
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property superClass As GenericTree()
        Get
            Return termsByLevel(2)
        End Get
    End Property

    ''' <summary>
    ''' level 3
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property [class] As GenericTree()
        Get
            Return termsByLevel(3)
        End Get
    End Property

    ''' <summary>
    ''' level 4
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property subClass As GenericTree()
        Get
            Return termsByLevel(4)
        End Get
    End Property

    Public ReadOnly Property molecularFramework As GenericTree()
        Get
            Return termsByLevel(5)
        End Get
    End Property

    Default Public ReadOnly Property Item(id As String) As GenericTree
        Get
            Return oboTable(id)
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="obo">The file path of ``ChemOnt_2_1.obo``</param>
    Sub New(obo As String)
        oboTable = New OBOFile(file:=obo) _
            .GetRawTerms _
            .BuildTree
        oboNodeList = oboTable _
            .Values _
            .ToArray
    End Sub

    ''' <summary>
    ''' 将对应分类层次等级的注释分类信息取出来
    ''' </summary>
    ''' <param name="anno"></param>
    ''' <param name="level%"></param>
    ''' <returns></returns>
    Public Iterator Function FilterByLevel(anno As IEnumerable(Of ClassyfireAnnotation), level%) As IEnumerable(Of ClassyfireAnnotation)
        Dim levelIndex As Index(Of String) = termsByLevel(level).TermIndex

        For Each item As ClassyfireAnnotation In anno
            If item.ChemOntID Like levelIndex Then
                Yield item
            End If
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetLineages(term_id As String) As NamedCollection(Of GenericTree)()
        Return oboTable(term_id).TermLineages.ToArray
    End Function

    ''' <summary>
    ''' Get terms by level on tree
    ''' </summary>
    ''' <param name="level"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function termsByLevel(level As Integer) As GenericTree()
        Return oboNodeList _
            .Select(Function(node)
                        Return node.GetTermsByLevel(level)
                    End Function) _
            .Where(Function(a) Not a.IsNullOrEmpty) _
            .IteratesALL _
            .Distinct _
            .OrderBy(Function(o) o.name) _
            .ToArray
    End Function
End Class
