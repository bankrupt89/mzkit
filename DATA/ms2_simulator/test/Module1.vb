﻿Imports ms2_simulator

Module Module1

    Sub Main()
        Dim beta As New EnergyModel(Function(x, y) Microsoft.VisualBasic.Math.Distributions.Beta.beta(x, 2, 5), 0, 1)


        Call beta.PercentageGreater(0).__DEBUG_ECHO

        Call beta.PercentageGreater(1).__DEBUG_ECHO

        Call beta.PercentageGreater(0.2).__DEBUG_ECHO

        Call beta.PercentageGreater(0.4).__DEBUG_ECHO

        Call beta.PercentageGreater(0.6).__DEBUG_ECHO

        Call beta.PercentageGreater(0.8).__DEBUG_ECHO

        Pause()
    End Sub

End Module
