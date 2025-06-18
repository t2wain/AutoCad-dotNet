Public NotInheritable Class ComHelper

    Private Sub New()

    End Sub

    Public Shared Function GetObject(pClassName As String) As Object
        Return Interaction.GetObject(Nothing, pClassName)
    End Function

    Public Shared Function CreateObject(pClassName As String) As Object
        Return Interaction.CreateObject(pClassName)
    End Function

End Class
