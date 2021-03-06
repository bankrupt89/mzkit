﻿#Region "Microsoft.VisualBasic::1ebb160bb8143b8232e1232994b33bb8, src\mzmath\TargetedMetabolomics\MRM\RawFile.vb"

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

    '     Class RawFile
    ' 
    '         Properties: allSamples, blanks, hasBlankControls, numberOfStandardReference, patternOfRefer
    '                     samples, standards
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetLinearGroups, getLinearsGroup, GetRawFileList, hasPatternOf, ToString
    '                   WrapperForStandards
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace MRM.Models

    ''' <summary>
    ''' mzML raw files from wiff converts output
    ''' </summary>
    Public Class RawFile

        Public Property samples As String()
        Public Property standards As String()
        ''' <summary>
        ''' Blank controls of the <see cref="standards"/> reference
        ''' </summary>
        ''' <returns></returns>
        Public Property blanks As String()

        Public ReadOnly Property allSamples As String()
            Get
                Return standards.AsList + samples
            End Get
        End Property

        Public ReadOnly Property hasBlankControls As Boolean
            Get
                Return Not blanks.IsNullOrEmpty
            End Get
        End Property

        Public ReadOnly Property numberOfStandardReference As Integer
            Get
                Return getLinearsGroup(standards, patternOfRefer).Count
            End Get
        End Property

        Public Property patternOfRefer As String

        Private Sub New()
            samples = {}
            standards = {}
            blanks = {}
        End Sub

        Sub New(directory$, Optional patternOfRefer$ = ".+[-]CAL[-]?\d+", Optional patternOfBlanks$ = "KB[-]?\d+")
            Dim mzML As String() = directory _
                .ListFiles("*.mzML") _
                .ToArray

            standards = mzML _
                .Where(Function(path)
                           Return hasPatternOf(path, patternOfRefer)
                       End Function) _
                .ToArray
            blanks = mzML _
                .Where(Function(path)
                           Return hasPatternOf(path, patternOfBlanks)
                       End Function) _
                .ToArray
            samples = mzML _
                .Where(Function(path)
                           Return Not hasPatternOf(path, patternOfRefer) AndAlso
                                  Not hasPatternOf(path, patternOfBlanks)
                       End Function) _
                .ToArray

            Me.patternOfRefer = patternOfRefer
        End Sub

        Sub New(sampleDir$, referenceDir$, Optional patternOfRefer$ = ".+[-]CAL[-]?\d+", Optional patternOfBlanks$ = "KB[-]?\d+")
            Dim mzML = referenceDir.ListFiles("*.mzML").ToArray

            samples = sampleDir.ListFiles("*.mzML").ToArray
            standards = mzML _
               .Where(Function(path)
                          Return hasPatternOf(path, patternOfRefer)
                      End Function) _
               .ToArray
            blanks = mzML _
                .Where(Function(path)
                           Return hasPatternOf(path, patternOfBlanks)
                       End Function) _
                .ToArray

            Me.patternOfRefer = patternOfRefer
        End Sub

        Public Function GetLinearGroups() As Dictionary(Of String, String())
            Return getLinearsGroup(standards, patternOfRefer) _
                .ToDictionary _
                .FlatTable
        End Function

        ''' <summary>
        ''' Get raw file list
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRawFileList() As Dictionary(Of String, String())
            Return New Dictionary(Of String, String()) From {
                {NameOf(standards), standards},
                {NameOf(blanks), blanks},
                {NameOf(samples), samples}
            }
        End Function

        Private Shared Function hasPatternOf(path$, pattern As String) As Boolean
            Return Not path _
                .BaseName _
                .Match(pattern, RegexICSng) _
                .StringEmpty
        End Function

        Private Shared Iterator Function getLinearsGroup(standards As IEnumerable(Of String), patternOfRefer$) As IEnumerable(Of NamedValue(Of String()))
            Dim groups = standards _
                .GroupBy(Function(fileName)
                             Return fileName.BaseName.StringReplace(patternOfRefer, "")
                         End Function) _
                .ToArray

            For Each group As IGrouping(Of String, String) In groups
                Yield New NamedValue(Of String()) With {
                    .Name = group.Key,
                    .Value = group.ToArray
                }
            Next
        End Function

        Public Shared Function WrapperForStandards(standards As String(), patternOfRefer$) As RawFile
            Return New RawFile With {
                .patternOfRefer = patternOfRefer,
                .standards = standards
            }
        End Function

        Public Overrides Function ToString() As String
            If blanks.IsNullOrEmpty Then
                Return $"{samples.Length} samples with {standards.Length} reference point."
            Else
                Return $"{samples.Length} samples with {standards.Length} reference point and {blanks.Length} blanks."
            End If
        End Function
    End Class
End Namespace
