Imports System.Windows.Threading

Class Application

    Public Shared StartUpLocation As String = myFolder.Path(System.Reflection.Assembly.GetExecutingAssembly().Location)
    Public Shared VersionNo As Integer = CInt(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileMajorPart & System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileMinorPart & System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileBuildPart)
    Public Shared Version As String = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileMajorPart & "." & System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileMinorPart & "." & System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileBuildPart
    Public Shared LegalCopyright As String = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).LegalCopyright
    Public Shared CompanyName As String = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).CompanyName
    Public Shared ProductName As String = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).ProductName
    Public Shared ExeName As String = myFile.Name(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).InternalName, False)
    Public Shared ProcessName As String = Diagnostics.Process.GetCurrentProcess.ProcessName
    Public Shared selType As Integer = 1
    Public Shared winStore As Boolean = True
    Public Shared newName As String = "Video přehled"

#Region " Global "

    Public Class myGlobal
        Public Shared NR As String = Chr(13) & Chr(10)
        Public Shared mySystem As New clsSystem
        Public Shared xmlNastaveni As String
        Public Shared xmlRoaming As Boolean
        Public Shared xmlMovies As String
        Public Shared xmlMoviesFilename As String = "Videos.pmb"
        Public Shared myMovies As New clsMovies
        Public Shared Nastaveni As New clsSetting
        Public Shared selCloud As Cloud
        Public Shared passMovies As String = Chr(118) & Chr(105) & Chr(100) & Chr(101) & Chr(111) & Chr(107) & Chr(111) & Chr(110) & Chr(118) & Chr(101) & Chr(114) & Chr(116) & Chr(111) & Chr(114)

        Public Enum Quality
            low = 0
            DVD = 1
            HDready = 2
            FullHD = 3
            UltraHD = 4
            _3D = 9
        End Enum

        Public Enum Videl
            Nyní = 0
            Rokem = 1
            Dvěma = 2
            Třemi = 3
            Čtyřmi = 4
            Pěti = 5
        End Enum
    End Class

#End Region

#Region " Window "

    Public Shared ReadOnly Property Icon As ImageSource
        Get
            Return myBitmap.UriToImageSource(New Uri("/" + ExeName + ";component/" + ExeName + ".ico", UriKind.Relative))
        End Get
    End Property

    Public Shared Function Title() As String
        Return ProductName + " " + Version
    End Function

    Public Shared Function SettingWindow() As WpfSetting
        For Each wOne As Window In Application.Current.Windows
            If wOne.Name = "wndSetting" Then Return CType(wOne, WpfSetting)
        Next
        Return Nothing
    End Function

#End Region

#Region " Exception "

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private bError As Boolean

    Private Sub App_DispatcherUnhandledException(ByVal sender As Object, ByVal e As DispatcherUnhandledExceptionEventArgs) Handles MyClass.DispatcherUnhandledException
        ' Process unhandled exception
        If bError Then Exit Sub
        bError = True
        e.Handled = True

        Dim Form As New wpfError
        Form.myError = e
        Form.ShowDialog()

        End
    End Sub

#End Region

End Class
