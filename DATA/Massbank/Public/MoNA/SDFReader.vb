﻿#Region "Microsoft.VisualBasic::0e1f911df52e6eb3a2a3260432390c8e, Massbank\Public\MoNA\MoNAJson.vb"

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

' Class MoNAJson
' 
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.MassSpectrum.DATA.File

Namespace MoNA

    ''' <summary>
    ''' Reader for file ``MoNA-export-LC-MS-MS_Spectra.sdf``
    ''' </summary>
    Public Module SDFReader

        Public Iterator Function ParseFile(path$, Optional parseStruct As Boolean = False) As IEnumerable(Of SpectraSection)
            For Each mol As SDF In SDF.IterateParser(path, parseStruct:=parseStruct)

            Next
        End Function
    End Module
End Namespace