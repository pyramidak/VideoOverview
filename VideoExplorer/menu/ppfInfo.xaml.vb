Class ppfInfo

    Private wMain As WpfMain = CType(Application.Current.MainWindow, WpfMain)
    Private wSetting As wpfSetting = Application.SettingWindow()

    Private Sub ppfInfo_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded

    End Sub

    Private Sub Hyperlink_MKV(sender As Object, e As RequestNavigateEventArgs)
        myLink.Start(wSetting, e.Uri.ToString)
        e.Handled = True
    End Sub
End Class
