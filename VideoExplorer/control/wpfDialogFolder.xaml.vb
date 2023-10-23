Imports System.Windows.Threading

Public Class wpfDialogFolder

#Region " Properties "

    Private WithEvents timStart As DispatcherTimer
    Private sImgCesta As String = "/" + Application.ExeName + ";component/pics/"
    Public Property SystemFolders As Boolean
        Set(value As Boolean)
            mTreeView.SystemFolders = value
        End Set
        Get
            Return mTreeView.SystemFolders
        End Get
    End Property

    Public Property UserFolders As Boolean
        Set(value As Boolean)
            mTreeView.UserFolders = value
            btnUser.Visibility = If(value, Visibility.Collapsed, Visibility.Visible)
            If value = False Then
                imgUser.Source = New BitmapImage(New Uri(sImgCesta & "Documents128.png", UriKind.Relative))
            End If
        End Set
        Get
            Return mTreeView.UserFolders
        End Get
    End Property

    Public Property SelectFolder As String
        Set(value As String)
            mTreeView.SelectedPath = value
        End Set
        Get
            Return mTreeView.SelectedPath
        End Get
    End Property

#End Region

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        If SelectFolder = "" Then Exit Sub
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub wpfDialogFolder_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        mTreeView.MainWindow = Me
        imgUser.Source = New BitmapImage(New Uri("/" & Application.ExeName & ";component/pics/Documents128.png", UriKind.Relative))
        timStart = New DispatcherTimer
        timStart.Interval = TimeSpan.FromMilliseconds(1)
        timStart.Start()
    End Sub

    Private Sub timStart_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles timStart.Tick
        timStart.Stop()
        mTreeView.Reload()
    End Sub
    Private Sub btnUser_Click(sender As Object, e As RoutedEventArgs) Handles btnUser.Click
        UserFolders = True
        mTreeView.LoadUserFolder()
    End Sub
End Class
