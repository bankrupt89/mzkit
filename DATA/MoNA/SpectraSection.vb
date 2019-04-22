﻿Imports System.Collections.Specialized
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports SMRUCC.MassSpectrum.DATA.MetaLib

Public Class SpectraSection : Inherits MetaInfo

    Public Property xref As xref
    Public Property SpectraInfo As Dictionary(Of String, String)
    Public Property Comment As NameValueCollection
    Public Property MassPeaks As DataSet()

    ''' <summary>
    ''' MoNA里面都主要是讲注释的信息放在<see cref="Comment"/>字段里面的。
    ''' 物质的注释信息主要是放在这个结构体之中，这个属性是对<see cref="Comment"/>
    ''' 属性的解析结果
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MetaDB As MetaData
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Comment.FillData
        End Get
    End Property

    Public ReadOnly Property MetaReader As UnionReader
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New UnionReader(MetaDB, msp:=Me)
        End Get
    End Property
End Class
