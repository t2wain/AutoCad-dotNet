Imports AutoCadComShared
Imports Autodesk.AutoCAD.Interop

Public Class AcFactoryImpl
    Inherits AcFactory

    Public Overrides Function GetAutoCadApp() As AcadApplication
        Return CType(ComHelper.GetComObject("AutoCAD.Application"), AcadApplication)
        'Return CType(GetObject(Nothing, "AutoCAD.Application"), AcadApplication)
    End Function

    Public Overrides Function NewAutoCadApp() As AcadApplication
        'Return CType(CreateObject("AutoCAD.Application"), AcadApplication)
        Return New AcadApplicationClass
    End Function

End Class
