﻿#Region "Microsoft.VisualBasic::0c847a26e2f36f3868d376499211f99c, src\assembly\assembly\MarkupData\mzML\IonPair.vb"

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

'     Class IonPair
' 
'         Properties: accession, name, precursor, product
' 
'         Function: Assert, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Data.Linq.Mapping
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports mzchromatogram = BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.chromatogram

Namespace MRM.Models

    Public Class IonPair : Implements INamedValue

        ''' <summary>
        ''' The database accession ID
        ''' </summary>
        ''' <returns></returns>
        <Column(Name:="ID")>
        Public Property accession As String
        ''' <summary>
        ''' The display title name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String Implements IKeyedEntity(Of String).Key
        Public Property precursor As Double
        Public Property product As Double
        Public Property rt As Double

        Public Overrides Function ToString() As String
            If name.StringEmpty Then
                Return $"{precursor}/{product}"
            Else
                Return $"Dim {name} As [{precursor}, {product}]"
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="chromatogram"></param>
        ''' <param name="tolerance">less than 0.3da or 20ppm??</param>
        ''' <returns></returns>
        Public Function Assert(chromatogram As mzchromatogram, tolerance As Tolerance) As Boolean
            Dim pre = chromatogram.precursor.MRMTargetMz
            Dim pro = chromatogram.product.MRMTargetMz

            If tolerance.Assert(Val(pre), precursor) AndAlso tolerance.Assert(Val(pro), product) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Produce an <see cref="IsomerismIonPairs"/> object sequence with 
        ''' element length equals to the input ions data 
        ''' <paramref name="ionpairs"/> 
        ''' </summary>
        ''' <param name="ionpairs"></param>
        ''' <param name="tolerance"></param>
        ''' <returns></returns>
        Public Shared Iterator Function GetIsomerism(ionpairs As IonPair(), tolerance As Tolerance) As IEnumerable(Of IsomerismIonPairs)
            Dim iso As New List(Of IonPair)

            For Each ion As IonPair In ionpairs
                For Each can As IonPair In ionpairs.Where(Function(a) Not a Is ion)
                    If tolerance.Assert(can.precursor, ion.precursor) AndAlso tolerance.Assert(can.product, ion.product) Then
                        iso += ion
                    End If
                Next

                Yield New IsomerismIonPairs With {
                    .ions = iso.PopAll,
                    .target = ion
                }
            Next
        End Function
    End Class

    Public Class IsomerismIonPairs

        Public Property ions As IonPair()
        Public Property target As IonPair

        ''' <summary>
        ''' Get the chromatogram overlaps index by rt/ri order
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property index As Integer
            Get
                If ions.IsNullOrEmpty Then
                    Return Scan0
                Else
                    Dim vec = ions.Join(target).OrderBy(Function(i) i.rt).ToArray

                    For i As Integer = 0 To vec.Length - 1
                        If vec(i).accession = target.accession Then
                            Return i
                        End If
                    Next

                    Throw New InvalidProgramException
                End If
            End Get
        End Property

        Public ReadOnly Property hasIsomerism As Boolean
            Get
                Return Not ions.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            If hasIsomerism Then
                Return $"[{index}, rt:{target.rt} (sec)] {target}"
            Else
                Return target.ToString
            End If
        End Function
    End Class
End Namespace
