﻿Namespace ASCII.MSP

    Public Class UnionReader

        ReadOnly meta As MetaData
        ReadOnly msp As MspData

#Region "Reader Properties"

        Public ReadOnly Property collision_energy As String
            Get
                Return meta.Read_collision_energy
            End Get
        End Property

        Public ReadOnly Property CAS As String
            Get
                Return meta.Read_CAS
            End Get
        End Property

        Public ReadOnly Property precursor_type As String
            Get
                Return meta.Read_precursor_type
            End Get
        End Property

        ''' <summary>
        ''' 单位已经统一为秒
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property retention_time As Double
            Get
                Return meta.read_retention_time
            End Get
        End Property

        Public ReadOnly Property instrument_type As String
            Get
                Return meta.Read_instrument_type
            End Get
        End Property

        Public ReadOnly Property exact_mass As Double
            Get
                Dim mass# = meta.Read_exact_mass

                If mass = 0R Then
                    Return msp.MW
                Else
                    Return mass
                End If
            End Get
        End Property

        Public ReadOnly Property formula As String
            Get
                If msp.Formula.StringEmpty Then
                    Return meta.molecular_formula
                Else
                    Return msp.Formula
                End If
            End Get
        End Property

        Public ReadOnly Property pubchem As String
            Get
                Return meta.Read_pubchemID
            End Get
        End Property

        Public ReadOnly Property ionMode As String
            Get
                If msp.Ion_mode.StringEmpty Then
                    Return meta.ionization_mode _
                        ?.Split(","c) _
                        ?.FirstOrDefault
                Else
                    Return msp.Ion_mode
                End If
            End Get
        End Property

        Public ReadOnly Property sourcefile As String
            Get
                Return meta.Read_source_file
            End Get
        End Property

        Public ReadOnly Property compound_source As String
            Get
                Return meta.Read_compound_source
            End Get
        End Property
#End Region

        Sub New(meta As MetaData, Optional msp As MspData = Nothing)
            Me.meta = meta
            Me.msp = msp
        End Sub

        Public Overrides Function ToString() As String
            Return meta.name
        End Function
    End Class
End Namespace