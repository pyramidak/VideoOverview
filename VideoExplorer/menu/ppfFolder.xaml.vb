Class ppfFolder

    Private wSetting As wpfSetting = Application.SettingWindow()

    Private Sub ppfFolder_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        btnRemoveSlozka.IsEnabled = False : btnRemoveDruh.IsEnabled = False : btnAddSlozka.IsEnabled = False
        ltbDruhy.ItemsSource = Nastaveni.Druhy
        If Nastaveni.DruhSelected IsNot Nothing AndAlso Not Nastaveni.DruhSelected = "" Then
            Dim Druh As clsSetting.clsDruh = Nastaveni.Druhy.FirstOrDefault(Function(x) x.Druh = Nastaveni.DruhSelected)
            If Druh IsNot Nothing Then ltbDruhy.SelectedItem = Druh
        End If
    End Sub

    Private Sub LtbDruhy_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ltbDruhy.SelectionChanged
        If ltbDruhy.SelectedItem Is Nothing Then
            btnAddSlozka.IsEnabled = False
            btnRemoveSlozka.IsEnabled = False
            btnRemoveDruh.IsEnabled = False
        Else
            btnAddSlozka.IsEnabled = True
            btnRemoveDruh.IsEnabled = True
            Nastaveni.DruhSelected = CType(ltbDruhy.SelectedItem, clsSetting.clsDruh).Druh
            txtTyp.Text = CType(ltbDruhy.SelectedItem, clsSetting.clsDruh).Pripony
            ltbSlozky.ItemsSource = Nastaveni.Slozky.Where(Function(x) x.Druh = Nastaveni.DruhSelected)
        End If
    End Sub

    Private Sub btnAddDruh_Click(sender As Object, e As RoutedEventArgs) Handles btnAddDruh.Click
        Dim wDialog As New wpfDialog(wSetting, "Vložte pojmenování druhu", wSetting.Title, wpfDialog.Ikona.tuzka, "OK", "Zavřít", True)
        If wDialog.ShowDialog = False OrElse wDialog.Input = "" Then Exit Sub

        Dim Druh As clsSetting.clsDruh = Nastaveni.Druhy.FirstOrDefault(Function(x) x.Druh = wDialog.Input)
        If Druh Is Nothing Then
            Nastaveni.Druhy.Add(wDialog.Input, txtTyp.Text)
            ltbDruhy.ScrollIntoView(Nastaveni.Druhy.Last)
            ltbDruhy.SelectedItem = Nastaveni.Druhy.Last
            wSetting.ReloadSlozky = True
        End If
    End Sub

    Private Sub btnRemoveDruh_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveDruh.Click
        If ltbDruhy.SelectedItem Is Nothing Then Exit Sub
        Dim Druh As clsSetting.clsDruh = CType(ltbDruhy.SelectedItem, clsSetting.clsDruh)
        Nastaveni.Slozky.Where(Function(x) x.Druh = Druh.Druh).ToList.ForEach(Sub(x) Nastaveni.Slozky.Remove(x))
        Nastaveni.Druhy.Remove(Druh)
        wSetting.ReloadSlozky = True
    End Sub

    Private Sub ltbSlozky_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ltbSlozky.SelectionChanged
        If ltbSlozky.SelectedItem Is Nothing Then
            btnRemoveSlozka.IsEnabled = False
        Else
            btnRemoveSlozka.IsEnabled = True
        End If
    End Sub

    Private Sub btnAddSlozka_Click(sender As Object, e As RoutedEventArgs) Handles btnAddSlozka.Click
        Dim wDialogFolder As New wpfDialogFolder
        wDialogFolder.Owner = wSetting
        wDialogFolder.SystemFolders = False
        wDialogFolder.UserFolders = False
        If wDialogFolder.ShowDialog() Then
            Dim Folder As clsSetting.clsFolder = Nastaveni.Slozky.FirstOrDefault(Function(x) x.Folder = wDialogFolder.SelectFolder)
            If Folder Is Nothing Then
                Nastaveni.Slozky.Add(wDialogFolder.SelectFolder, Nastaveni.DruhSelected)
            Else
                Folder.Druh = Nastaveni.DruhSelected
            End If
            ltbSlozky.ItemsSource = Nastaveni.Slozky.Where(Function(x) x.Druh = Nastaveni.DruhSelected)
            ltbSlozky.ScrollIntoView(Folder)
            ltbSlozky.SelectedItem = Folder
            wSetting.ReloadSlozky = True
        End If
    End Sub

    Private Sub btnRemoveSlozka_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveSlozka.Click
        If ltbSlozky.SelectedItem Is Nothing Then Exit Sub
        Nastaveni.Slozky.Remove(CType(ltbSlozky.SelectedItem, clsSetting.clsFolder))
        ltbSlozky.ItemsSource = Nastaveni.Slozky.Where(Function(x) x.Druh = Nastaveni.DruhSelected)
        wSetting.ReloadSlozky = True
    End Sub

    Private Sub txtTyp_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtTyp.LostFocus
        Dim Druh As clsSetting.clsDruh = Nastaveni.Druhy.FirstOrDefault(Function(x) x.Druh = Nastaveni.DruhSelected)
        If Druh IsNot Nothing Then
            If Not txtTyp.Text = Druh.Pripony AndAlso myFile.isSearchSafe(txtTyp.Text) Then
                Druh.Pripony = txtTyp.Text
                wSetting.ReloadSlozky = True
            End If
        End If
    End Sub

    Private Sub txtTyp_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtTyp.TextChanged
        If myFile.isSearchSafe(txtTyp.Text) Then
            txtTyp.Background = Brushes.White
        Else
            txtTyp.Background = Brushes.LightPink
        End If
    End Sub
End Class
