﻿Imports System.Collections.Specialized
Imports System.Data.Linq.Mapping
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Parsers
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace ASCII.MSP

    Public Module Comments

        ''' <summary>
        ''' 解析放置于注释之中的代谢物注释元数据
        ''' </summary>
        ''' <param name="comments$"></param>
        ''' <returns></returns>
        <Extension> Public Function ToTable(comments$) As NameValueCollection
            Dim tokens$() = CLIParser.GetTokens(comments)
            Dim data = tokens _
                .Select(Function(s)
                            Return s.GetTagValue("=", trim:=True)
                        End Function) _
                .GroupBy(Function(o) o.Name)
            Dim table As New NameValueCollection  ' 为了兼容两个SMILES结构

            For Each g In data
                For Each s As NamedValue(Of String) In g
                    Call table.Add(g.Key, s.Value)
                Next
            Next

            Return table
        End Function

        ReadOnly names As Dictionary(Of String, String)

        Sub New()
            names = Mappings.FieldNameMappings(Of MetaData)(explict:=True)
        End Sub

        <Extension> Public Function FillData(comments$) As MetaData
            Dim table As NameValueCollection = comments.ToTable
            Dim meta As Object = New MetaData
            Dim fields = Mappings.GetFields(Of MetaData)

            For Each field As BindProperty(Of ColumnAttribute) In fields
                Dim name$ = field.Identity

                If field.Type.IsInheritsFrom(GetType(Array)) Then
                    Dim value As String()

                    value = table.GetValues(name)
                    If value.IsNullOrEmpty Then
                        value = table.GetValues(names(name))
                    End If

                    Call field.SetValue(meta, value)
                Else
                    Dim value As String

                    value = table(name)
                    If value.StringEmpty Then
                        value = table(names(name))
                    End If

                    Call field.SetValue(meta, Scripting.CTypeDynamic(value, field.Type))
                End If
            Next

            Return DirectCast(meta, MetaData)
        End Function
    End Module

    Public Class MetaData

        Public Property accession As String
        Public Property author As String
        Public Property license As String
        <Column(Name:="exact mass")>
        Public Property exact_mass As Double
        Public Property instrument As String
        <Column(Name:="instrument type")>
        Public Property instrument_type As String
        <Column(Name:="ms level")>
        Public Property ms_level As String
        <Column(Name:="ionization energy")>
        Public Property ionization_energy As String
        <Column(Name:="ion type")>
        Public Property ion_type As String
        <Column(Name:="ionization mode")>
        Public Property ionization_mode As String
        <Column(Name:="Last Auto-Curation")>
        Public Property Last_AutoCuration As String
        Public Property SMILES As String()
        Public Property InChI As String
        <Column(Name:="molecular formula")>
        Public Property molecular_formula As String
        <Column(Name:="total exact mass")>
        Public Property total_exact_mass As Double
        Public Property InChIKey As String
        Public Property copyright As String
        Public Property ionization As String
        <Column(Name:="fragmentation mode")>
        Public Property fragmentation_mode As String
        Public Property resolution As String
        Public Property column As String
        <Column(Name:="flow gradient")>
        Public Property flow_gradient As String
        <Column(Name:="flow rate")>
        Public Property flow_rate As String
        <Column(Name:="retention time")>
        Public Property retention_time As String
        <Column(Name:="solvent a")>
        Public Property solvent_a As String
        <Column(Name:="solvent b")>
        Public Property solvent_b As String
        <Column(Name:="precursor m/z")>
        Public Property precursor_mz As String
        <Column(Name:="precursor type")>
        Public Property precursor_type As String
        <Column(Name:="mass accuracy")>
        Public Property mass_accuracy As Double
        <Column(Name:="mass error")>
        Public Property mass_error As Double
        Public Property cas As String
        <Column(Name:="pubchem cid")>
        Public Property pubchem_cid As String
        Public Property chemspider As String
        <Column(Name:="charge state")>
        Public Property charge_state As Integer
        <Column(Name:="compound source")>
        Public Property compound_source As String
        <Column(Name:="source file")>
        Public Property source_file As String
        Public Property origin As String
        Public Property adduct As String
        <Column(Name:="ion source")>
        Public Property ion_source As String

        Public Overrides Function ToString() As String
            Return accession
        End Function
    End Class
End Namespace