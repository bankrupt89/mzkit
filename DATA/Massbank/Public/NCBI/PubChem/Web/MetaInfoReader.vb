﻿Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.MassSpectrum.DATA.MetaLib.Models
Imports MetaInfo = SMRUCC.MassSpectrum.DATA.MetaLib.Models.MetaLib

Namespace NCBI.PubChem

    <HideModuleName>
    Public Module MetaInfoReader

        ''' <summary>
        ''' 如果<paramref name="path"/>的末端是使用索引语法,则索引的起始下标是从零开始的
        ''' </summary>
        ''' <param name="view"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetInform(view As PugViewRecord, path$) As Information
            Dim parts = path.Trim("/"c).Split("/"c)
            Dim section = view.navigateView(parts)

            Return section.GetInformation(parts.Last, multipleInfo:=False)
        End Function

        <Extension>
        Private Function navigateView(view As PugViewRecord, parts As String()) As Section
            If parts.Length = 1 OrElse view Is Nothing Then
                Return Nothing
            End If

            Dim sec As Section = view(parts(Scan0))

            For Each part As String In parts.Skip(1).Take(parts.Length - 2)
                If sec Is Nothing Then
                    Return Nothing
                Else
                    sec = sec(part)
                End If
            Next

            Return sec
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="view"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetInformList(view As PugViewRecord, path$) As Information()
            Dim parts = path.Trim("/"c).Split("/"c)
            Dim section = view.navigateView(parts)

            Return section.GetInformation(parts.Last, multipleInfo:=True)
        End Function

        ReadOnly nameDatabase As Index(Of String) = {
            "Human Metabolome Database (HMDB)",
            "ChEBI",
            "DrugBank",
            "European Chemicals Agency (ECHA)",
            "MassBank of North America (MoNA)"
        }

        ''' <summary>
        ''' 从pubchem数据库之中提取注释所需要的必须基本信息
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetMetaInfo(view As PugViewRecord) As MetaInfo
            Dim identifier = view("Names and Identifiers")
            Dim formula = view.GetInform("/Names and Identifiers/Molecular Formula/#0")
            Dim descriptors = identifier("Computed Descriptors")
            Dim SMILES = view.GetInform("/Names and Identifiers/Computed Descriptors/Canonical SMILES/#0")
            Dim InChIKey = descriptors("InChI Key").GetInformationString("#0")
            Dim InChI = descriptors("InChI").GetInformationString("#0")
            Dim otherNames = identifier("Other Identifiers")
            Dim synonyms = identifier("Synonyms")("Depositor-Supplied Synonyms").GetInformationStrings(Nothing)
            Dim computedProperties = view("Chemical and Physical Properties")("Computed Properties")
            ' Dim properties = Table.ToDictionary(computedProperties)
            Dim CASNumber$()

            If synonyms Is Nothing Then
                synonyms = {}
            End If

            If otherNames Is Nothing Then
                CASNumber = synonyms _
                    .Where(Function(id) id.IsPattern("\d+([-]\d+)+")) _
                    .ToArray
            Else
                CASNumber = otherNames("CAS")?.GetInformationStrings("CAS", True)
            End If

            Dim exact_mass# = computedProperties("Exact Mass").GetInformationNumber(Nothing)
            Dim xref As New xref With {
                .InChI = InChI,
                .CAS = CASNumber,
                .InChIkey = InChIKey,
                .pubchem = view.RecordNumber,
                .chebi = synonyms.FirstOrDefault(Function(id) id.IsPattern("CHEBI[:]\d+")),
                .KEGG = synonyms.FirstOrDefault(Function(id)
                                                    ' KEGG编号是C开头,后面跟随5个数字
                                                    Return id.IsPattern("C\d{5}", RegexOptions.Singleline)
                                                End Function),
                .HMDB = view.Reference.GetHMDBId,
                .SMILES = SMILES.InfoValue
            }
            Dim commonName$ = view.RecordTitle

            If commonName.StringEmpty Then
                commonName = view _
                    .Reference _
                    .FirstOrDefault(Function(r) r.SourceName Like nameDatabase) _
                   ?.Name
            End If

            ' 20190618 formula可能会存在多个的情况
            Dim formulaStr As String = ""

            If Not formula Is Nothing Then
                If formula.InfoType Is GetType(String) Then
                    formulaStr = formula.InfoValue
                Else
                    formulaStr = DirectCast(formula.InfoValue, String()).FirstOrDefault
                End If
            End If

            Return New MetaInfo With {
                .formula = formulaStr,
                .xref = xref,
                .name = commonName,
                .exact_mass = exact_mass,
                .ID = view.RecordNumber
            }
        End Function
    End Module
End Namespace