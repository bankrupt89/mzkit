﻿Imports System.ComponentModel
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports SMRUCC.MassSpectrum.DATA.NCBI
Imports SMRUCC.MassSpectrum.DATA.NCBI.PubChem

Partial Module CLI

    <ExportAPI("/image.fly")>
    <Usage("/image.fly /cid <cid> [/transparent /size <w,h default=300,300> /out <save.png>]")>
    <Group(Program.WebApiCli)>
    <Argument("/transparent", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="Make the image transparent?")>
    Public Function ImageFlyCLI(args As CommandLine) As Integer
        Dim cid$ = args("/cid")
        Dim size$ = args("/size") Or "300,300"
        Dim out$ = args("/out") Or $"./{cid}_size={size}.png"
        Dim doTransparent As Boolean = args("/transparent")

        Return ImageFly.GetImage(cid, size, doTransparent) _
            .SaveAs(out) _
            .CLICode
    End Function

    <ExportAPI("/query")>
    <Description("Do pubchem database query.")>
    <Usage("/query /terms <list.txt/json/xml> [/include.image /out <directory>]")>
    <Group(Program.WebApiCli)>
    <Argument("/terms", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String())},
              Extensions:="*.txt, *.json, *.xml",
              Description:="This parameter should be a string list of names")>
    <Argument("/include.image", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="Also includes 2D structure images.")>
    Public Function QueryPubchem(args As CommandLine) As Integer
        Dim in$ = args <= "/terms"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.query_result/"
        Dim terms$() = [in].SolveListStream.ToArray
        Dim includeImage As Boolean = args("/include.image")
        Dim cid$()

        For Each term As String In terms
            cid = PubChem _
                .QueryPugViews(term, cacheFolder:=$"{out}/.cache/") _
                .Keys _
                .ToArray

            If includeImage Then
                For Each id As String In cid
                    Call ImageFly _
                        .GetImage(id, 500, 500, doBgTransparent:=False) _
                        .SaveAs($"{out}/{id}.png")
                    Call Thread.Sleep(1000)
                Next
            End If
        Next

        Return 0
    End Function
End Module