Class ppfSave

    Private wMain As WpfMain = CType(Application.Current.MainWindow, WpfMain)
    Private wSetting As WpfSetting = Application.SettingWindow()
    Private myCloud As New clsCloud

#Region " Loaded "

    Private Sub ppfSave_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        Select Case selCloud
            Case Cloud.Documents
                rbtDocuments.IsChecked = True
            Case Cloud.OneDrive
                rbtOneDrive.IsChecked = True
            Case Cloud.DropBox
                rbtDropBox.IsChecked = True
            Case Cloud.GoogleDisk
                rbtGoogleDrive.IsChecked = True
            Case Cloud.Sync
                rbtSync.IsChecked = True
        End Select
        rbtDropBox.IsEnabled = myCloud.DropBoxExist
        rbtOneDrive.IsEnabled = myCloud.OneDriveExist
        rbtGoogleDrive.IsEnabled = myCloud.GoogleDriveExist
        rbtSync.IsEnabled = myCloud.SyncExist
    End Sub

#End Region

#Region " Save "

    Private Sub btnSave_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles btnSave.Click
        btnSave.IsEnabled = False
        If rbtDocuments.IsChecked Then
            selCloud = Cloud.Documents
        ElseIf rbtOneDrive.IsChecked Then
            selCloud = Cloud.OneDrive
        ElseIf rbtDropBox.IsChecked Then
            selCloud = Cloud.DropBox
        ElseIf rbtGoogleDrive.IsChecked Then
            selCloud = Cloud.GoogleDisk
        ElseIf rbtSync.IsChecked Then
            selCloud = Cloud.Sync
        End If

        xmlNastaveni = myCloud.NewAppPath(selCloud)
        xmlMovies = myFile.Join(myFile.Path(xmlNastaveni), xmlMoviesFilename)
        If myFile.Exist(xmlNastaveni) Then
            Dim wDialog As New wpfDialog(wSetting, If(mySystem.LgeCzech,
                "V novém umístění již datový soubor existuje." + NR + NR + "Chcete použít nalezený soubor nebo ho nahradit aktuálním?",
                "In new location data file already exists." + NR + NR + "Do you want to load this found file or replace it with actual one?"),
                "Umístění datového souboru", wpfDialog.Ikona.dotaz, If(mySystem.LgeCzech, "Použít", "Load"), If(mySystem.LgeCzech, "Nahradit", "Replace"))
            If wDialog.ShowDialog() Then
                wMain.LoadSetting(wSetting)
                wSetting.ReloadSlozky = True
            End If
        End If
        wMain.SaveSetting(wSetting)
        myRegister.WriteCloudMyApp(selCloud)
        Dim xmlStream = New clsSerialization(myMovies, wMain).WriteXml()
        Call (New clsEncryption(passMovies, wMain)).EncryptStream(xmlStream, xmlMovies)
    End Sub

#End Region

#Region " Ostatní "

    Private Sub rbtCloud_Checked(sender As Object, e As RoutedEventArgs) Handles rbtDropBox.Checked, rbtDocuments.Checked, rbtOneDrive.Checked, rbtGoogleDrive.Checked, rbtSync.Checked
        btnSave.IsEnabled = True
        If rbtDocuments.IsChecked And selCloud = Cloud.Documents Then
            btnSave.IsEnabled = False
        ElseIf rbtOneDrive.IsChecked And selCloud = Cloud.OneDrive Then
            btnSave.IsEnabled = False
        ElseIf rbtDropBox.IsChecked And selCloud = Cloud.DropBox Then
            btnSave.IsEnabled = False
        ElseIf rbtGoogleDrive.IsChecked And selCloud = Cloud.GoogleDisk Then
            btnSave.IsEnabled = False
        ElseIf rbtSync.IsChecked And selCloud = Cloud.Sync Then
            btnSave.IsEnabled = False
        End If
    End Sub

#End Region


End Class
