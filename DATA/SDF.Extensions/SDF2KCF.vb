﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Chemistry.Model
Imports KCF_atom = SMRUCC.Chemistry.Model.Atom
Imports KCF_bound = SMRUCC.Chemistry.Model.Bound

Public Module SDF2KCF

    ''' <summary>
    ''' Convert the 3D mol structure as the <see cref="KCF"/> KEGG molecule 2D model
    ''' </summary>
    ''' <param name="sdf"></param>
    ''' <returns></returns>
    <Extension> Public Function ToKCF(sdf As SDF) As KCF
        Dim mol As [Structure] = sdf.Structure
        Dim atoms As KCF_atom() = mol.Atoms _
            .Select(Function(a, i)
                        Return New KCF_atom With {
                            .Atom = a.Atom,
                            .Atom2D_coordinates = New Coordinate With {
                                .X = a.Coordination.X,
                                .Y = a.Coordination.Y,
                                .ID = i
                            },
                            .Index = i,
                            .KEGGAtom = a.Atom
                        }
                    End Function) _
            .ToArray
        Dim bounds As KCF_bound() = mol.Bounds _
            .Select(Function(b)
                        Return New KCF_bound With {
                            .bounds = b.Type,
                            .from = b.i,
                            .to = b.j,
                            .dimentional_levels = b.Stereo.ToString
                        }
                    End Function) _
            .ToArray

        Return New KCF With {
            .Entry = New Entry With {
                .Id = sdf.ID,
                .Type = sdf.Comment
            },
            .Atoms = atoms,
            .Bounds = bounds
        }
    End Function
End Module