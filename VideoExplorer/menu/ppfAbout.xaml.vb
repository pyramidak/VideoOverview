Class ppfAbout

    Private PicturePath As String
    Private wMain As WpfMain = CType(Application.Current.MainWindow, WpfMain)
    Private wSetting As wpfSetting = Application.SettingWindow()

#Region " Loaded "

    Private Sub ppfAbout_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        lblApp.Text = Application.CompanyName & "  " & Application.ProductName & "  verze " & Application.Version
        lblCop.Text = "copyright ©2016-" & mySystem.BuildYear.ToString & "  " & Application.LegalCopyright
        txtLicense.Text = myString.FromBytes(myFile.ReadEmbeddedResource("License.txt"))
    End Sub

#End Region

#Region " Hyperlinks "

    Private Sub lbl_MouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles lblMail.MouseLeftButtonUp, lblWeb.MouseLeftButtonUp
        Dim lbl As TextBlock = CType(sender, TextBlock)
        myLink.Start(wSetting, lbl.Text)
    End Sub

    Private Sub lbl_MouseEnter(sender As System.Object, e As System.Windows.Input.MouseEventArgs) Handles lblMail.MouseEnter, lblWeb.MouseEnter
        Me.Cursor = Cursors.Hand
    End Sub

    Private Sub lbl_MouseLeave(sender As System.Object, e As System.Windows.Input.MouseEventArgs) Handles lblMail.MouseLeave, lblWeb.MouseLeave
        Me.Cursor = Cursors.Arrow
    End Sub

#End Region

End Class
