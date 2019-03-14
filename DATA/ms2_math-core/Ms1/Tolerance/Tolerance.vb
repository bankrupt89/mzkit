﻿#Region "Microsoft.VisualBasic::a9c9c4a534837c546fb1ac391417f6d5, ms2_math-core\Ms1\Tolerance.vb"

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

    ' Class Tolerance
    ' 
    '     Properties: [Interface], DefaultTolerance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AddPPM, DeltaMass, PPM, SubPPM
    ' 
    ' Class PPMmethod
    ' 
    '     Properties: ppmValue
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: AsScore, Assert, ppm, ToString
    ' 
    ' Class DAmethod
    ' 
    '     Properties: da
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AsScore, Assert, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace Ms1

    ''' <summary>
    ''' The m/z tolerance methods.
    ''' (可以直接使用这个对象的索引属性来进行计算判断)
    ''' </summary>
    Public MustInherit Class Tolerance

        Public ReadOnly Property [Interface] As Tolerance
            Get
                Return Me
            End Get
        End Property

        Default Public ReadOnly Property IsEquals(mz1#, mz2#) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Assert(mz1, mz2)
            End Get
        End Property

        Default Public ReadOnly Property IsEquals(mz1 As Spectra.ms2, mz2 As Spectra.ms2) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Assert(mz1.mz, mz2.mz)
            End Get
        End Property

        ''' <summary>
        ''' 默认的误差计算是小于0.3个道尔顿以内
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DefaultTolerance As DefaultValue(Of Tolerance)

        Shared Sub New()
            DefaultTolerance = New DAmethod(0.3).Interface
        End Sub

        Public MustOverride Function Assert(mz1#, mz2#) As Boolean
        Public MustOverride Function AsScore(mz1#, mz2#) As Double
        Public MustOverride Function MassError(mz1#, mz2#) As Double

        Public Shared Function DeltaMass(da#) As DAmethod
            Return New DAmethod(da)
        End Function

        Public Shared Function PPM(value#) As PPMmethod
            Return New PPMmethod(value)
        End Function

        ''' <summary>
        ''' 将当前的质量值减去给定的ppm质量差
        ''' </summary>
        ''' <param name="mass#"></param>
        ''' <param name="ppm#"></param>
        ''' <returns></returns>
        Public Shared Function SubPPM(mass#, ppm#) As Double
            Dim ppmd As Double = ppm / 1000000
            ' sys.Abs(measured - actualValue) 
            ppmd = ppmd * mass
            mass = mass - ppmd

            Return mass
        End Function

        ''' <summary>
        ''' 将当前的质量值加上给定的ppm质量差
        ''' </summary>
        ''' <param name="mass#"></param>
        ''' <param name="ppm#"></param>
        ''' <returns></returns>
        Public Shared Function AddPPM(mass#, ppm#) As Double
            Dim ppmd As Double = ppm / 1000000
            ' sys.Abs(measured - actualValue) 
            ppmd = ppmd * mass
            mass = mass + ppmd

            Return mass
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(tolerance As Tolerance) As Comparison(Of Double)
            Return Function(mz1, mz2) As Integer
                       If tolerance(mz1, mz2) Then
                           Return 0
                       ElseIf mz1 > mz2 Then
                           Return 1
                       Else
                           Return -1
                       End If
                   End Function
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(tolerance As Tolerance) As GenericLambda(Of Double).IEquals
            Return AddressOf tolerance.Assert
        End Operator
    End Class
End Namespace