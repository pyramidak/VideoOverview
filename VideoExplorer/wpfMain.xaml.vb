Imports System.Globalization
Imports System.Windows.Threading

Class WpfMain

    Private TempCover As String
    Private TempInfo As String
    Public MKVextract As String
    Public MKVexists As Boolean
    Private ZJShareMem As New clsSharedMemory
    Private WithEvents timZJS As DispatcherTimer
    Private NactenoZeSouboru As String
    Private MeTitle As String

#Region " Main_Events "

    Private Sub WpfMain_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        If mySystem.isAppRunning(Application.ProcessName, mySystem.User) Then End
    End Sub

    Private Sub WpfMain_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        MeTitle = Me.Title
        'MKVtoolnix
        TempCover = myFile.Join(My.Computer.FileSystem.SpecialDirectories.Temp, "mkvcover.jpg")
        TempInfo = myFile.Join(My.Computer.FileSystem.SpecialDirectories.Temp, "mkvinfo.txt")
        MKVextract = "mkvtoolnix"
        SearchUninstall()

        selCloud = myRegister.GetCloudMyApp
        xmlNastaveni = (New clsCloud).NewAppPath(selCloud)
        xmlMovies = myFile.Join(myFile.Path(xmlNastaveni), xmlMoviesFilename)
        LoadSetting(Me)
        LoadMovies()

        'Update
        ZJShareMem.Open("ZJS")
        timZJS = New DispatcherTimer
        timZJS.Interval = TimeSpan.FromSeconds(2)
        timZJS.IsEnabled = True
    End Sub

    Private Sub WpfMain_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If NactenoZeSouboru = "" Then SaveSetting(Me)
    End Sub

    Private Sub WpfMain_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.F1 Then OpenSetting(2)
    End Sub

#End Region

#Region " Load / Save setting "
    Public Sub SaveSetting(Wnd As Window)
        Call New clsSerialization(Nastaveni, Wnd).WriteXml(xmlNastaveni)
        Dim xmlStream = New clsSerialization(myMovies, Wnd).WriteXml()
        Call (New clsEncryption(passMovies, Wnd)).EncryptStream(xmlStream, xmlMovies)
    End Sub

    Public Sub LoadSetting(Wnd As Window)
        If myFile.Exist(xmlNastaveni) Then
            Nastaveni = CType(New clsSerialization(Nastaveni).ReadXml(xmlNastaveni), clsSetting)
        End If
        If myFile.Exist(xmlMovies) Then
            LoadFile(xmlMovies, passMovies)
        End If

        If Nastaveni.DruhSelected Is Nothing OrElse Nastaveni.DruhSelected = "" Then
            If Nastaveni.Druhy.Count = 0 Then
                If Wnd Is Me Then OpenSetting(2)
            Else
                Nastaveni.DruhSelected = Nastaveni.Druhy(0).Druh
            End If
        End If
    End Sub

#End Region

#Region " Load Movies "
    Private Sub LoadMovies()
        LoadFilters()
        Prefiltrovat()
        cbxDruh.IsEnabled = False
        If ListView1.Items.Count = 0 Then
            Me.Cursor = Cursors.Wait
        Else
            Me.Cursor = Cursors.AppStarting
        End If
        bgwMovies = New System.ComponentModel.BackgroundWorker
        bgwMovies.RunWorkerAsync()
    End Sub

    Private WithEvents bgwMovies As System.ComponentModel.BackgroundWorker
    Private Sub bgwMovies_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgwMovies.DoWork
        Nastaveni.Slozky.Where(Function(a) a.Druh = Nastaveni.DruhSelected).ToList.ForEach(Sub(x) LoadFolder(x.Folder))
        myMovies.Where(Function(x) x.RootDir = Nastaveni.DruhSelected And (x.Exist = False Or CheckFolder(x.Path) = False)).ToList.ForEach(Sub(y) myMovies.Remove(y))
    End Sub
    Private Sub bgwMovies_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgwMovies.RunWorkerCompleted
        cbxDruh.IsEnabled = True
        Me.Cursor = Cursors.Arrow
        LoadFilters()
        Prefiltrovat()
    End Sub

    Private Function CheckFolder(Folder As String) As Boolean
        For Each Slozka In Nastaveni.Slozky
            If Folder.Contains(Slozka.Folder) Then Return True
        Next
        Return False
    End Function

    Private Sub LoadFolder(slozka As String)
        Dim pripony As String = "*.mkv;*.mpg;*.mp4;*.wmv;*.avi;*.mov"
        Dim Druh As clsSetting.clsDruh = Nastaveni.Druhy.FirstOrDefault(Function(x) x.Druh = Nastaveni.DruhSelected)
        If Druh IsNot Nothing Then pripony = Druh.Pripony
        myMovies.Add(myFolder.Files(slozka, pripony, True), Druh.Druh)
    End Sub

    Private Sub LoadFilters()
        Filtrovat = False
        Dim arrDruhy As New ArrayList
        If NactenoZeSouboru = "" Then
            arrDruhy.AddRange(Nastaveni.Druhy.Select(Function(x) x.Druh).ToArray)
            cbxDruh.ItemsSource = arrDruhy
            cbxDruh.SelectedItem = Nastaveni.DruhSelected
            cbxDruh.IsEnabled = True
        Else
            arrDruhy.Add("externí")
            cbxDruh.ItemsSource = arrDruhy
            cbxDruh.SelectedIndex = 0
            cbxDruh.IsEnabled = False
        End If
        Dim arrKvalita As New ArrayList
        arrKvalita.Add("Vše")
        arrKvalita.AddRange(Quality.GetValues(GetType(Quality)).Cast(Of Quality)().ToArray)
        cbxRozliseni.ItemsSource = arrKvalita
        cbxRozliseni.SelectedIndex = 0
        Dim arrZanry As New ArrayList
        If NactenoZeSouboru = "" Then
            arrZanry.AddRange(myMovies.Where(Function(y) y.RootDir = Nastaveni.DruhSelected).Select(Function(x) x.Theme).Distinct.ToArray)
        Else
            arrZanry.AddRange(myMovies.Select(Function(x) x.Theme).Distinct.ToArray)
        End If
        arrZanry.Sort()
        arrZanry.Insert(0, "Vše")
        cbxZanr.ItemsSource = arrZanry
        cbxZanr.SelectedIndex = 0
        Dim arrLet As New ArrayList
        arrLet.Add("")
        arrLet.AddRange(Videl.GetValues(GetType(Videl)).Cast(Of Videl)().ToArray)
        cbxVidel.ItemsSource = arrLet
        cbxVidel.SelectedIndex = 0

        Dim Druh = Nastaveni.Druhy.FirstOrDefault(Function(a) a.Druh = Nastaveni.DruhSelected)
        If Druh IsNot Nothing Then
            If Druh.ZanrSelected IsNot Nothing AndAlso Not Druh.ZanrSelected = "" And Not Druh.ZanrSelected = "Vše" Then
                Dim found = cbxZanr.Items.Cast(Of String).FirstOrDefault(Function(x) x = Druh.ZanrSelected)
                If found IsNot Nothing Then
                    cbxZanr.SelectedItem = Druh.ZanrSelected
                Else
                    cbxZanr.SelectedIndex = 0
                End If
            End If
        End If

        Filtrovat = True
    End Sub

#End Region

#Region " ListViewItem "

    Private Sub ListView1_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim item As ListViewItem = TryCast(sender, ListViewItem)
        If item IsNot Nothing Then myFile.Launch(Me, CType(item.Content, clsMovie).Path)
    End Sub

    Private Sub ListView1_Selected(sender As Object, e As RoutedEventArgs)
        Dim item As ListViewItem = TryCast(sender, ListViewItem)
        If item IsNot Nothing Then
            imgCover.Source = Nothing
            SelectedItem = CType(item.Content, clsMovie)
            If SelectedItem.Extension = ".mkv" And Not MKVextract = "mkvtoolnix" Then
                bgwCover = New System.ComponentModel.BackgroundWorker
                bgwCover.RunWorkerAsync()
                If SelectedItem.Length Is Nothing Then
                    VisibleItems.Add(SelectedItem)
                    If bgwDelka IsNot Nothing AndAlso bgwDelka.IsBusy Then
                    Else
                        bgwDelka = New System.ComponentModel.BackgroundWorker
                        bgwDelka.RunWorkerAsync()
                    End If
                End If
            End If
        End If
    End Sub

#End Region

#Region " Drag and Drop "

    Private ColumnIcon As New Uri("VideoExplorer;component/VideoExplorer.ico", UriKind.Relative)
    'Toto funguje pouze pro draganddrop započatý drag v aplikaci
    Public Sub ListView1_GiveFeedback(sender As System.Object, e As System.Windows.GiveFeedbackEventArgs)
        e.UseDefaultCursors = False
        If e.Effects = DragDropEffects.Link Then
            Mouse.SetCursor(myBitmap.ToCursor(ColumnIcon))
            e.Handled = True
        End If
    End Sub

    Public Sub ListView1_DragEnter(sender As System.Object, e As System.Windows.DragEventArgs) Handles ListView1.DragEnter
        If e.Data.GetDataPresent("FileDrop") Then
            e.Effects = DragDropEffects.Link
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    Public Sub ListView1_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles ListView1.Drop
        If e.Data.GetDataPresent("FileDrop") Then
            Dim fileNames() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            For Each filename In fileNames
                If filename Like "*.pmb" Then
                    LoadFile(filename, "", True)
                    Exit For
                End If
            Next
        ElseIf e.Data.GetDataPresent(DataFormats.Text) Then
        End If
    End Sub

#End Region

#Region " Menu "
    Private Sub miReload_Click(sender As Object, e As RoutedEventArgs) Handles miReload.Click
        LoadSetting(Me)
        LoadMovies()
    End Sub
    Private Sub btnSetting_Click(sender As Object, e As RoutedEventArgs) Handles btnSetting.Click
        OpenSetting(0)
    End Sub

    Private Sub miClear_Click(sender As Object, e As RoutedEventArgs) Handles miClear.Click
        myMovies.Clear()
    End Sub

    Private Sub miSetting_Click(sender As Object, e As RoutedEventArgs) Handles miSetting.Click
        OpenSetting(0)
    End Sub

    Private Sub OpenSetting(index As Integer)
        Dim wSetting As New WpfSetting
        wSetting.Owner = Me
        wSetting.IndexPage = index
        wSetting.ShowDialog()
        If wSetting.ReloadSlozky Then
            LoadMovies()
            SaveSetting(Me)
        End If
    End Sub

    Private Sub miWatched_Click(sender As Object, e As RoutedEventArgs) Handles miWatched.Click
        ChangeWatched(ListView1.SelectedItems.Cast(Of clsMovie), Now)
    End Sub

    Private Sub miRok_Click(sender As Object, e As RoutedEventArgs) Handles miRok.Click
        ChangeWatched(ListView1.SelectedItems.Cast(Of clsMovie), Now.AddYears(-1))
    End Sub

    Private Sub miDvema_Click(sender As Object, e As RoutedEventArgs) Handles miDvema.Click
        ChangeWatched(ListView1.SelectedItems.Cast(Of clsMovie), Now.AddYears(-2))
    End Sub

    Private Sub miTremi_Click(sender As Object, e As RoutedEventArgs) Handles miTremi.Click
        ChangeWatched(ListView1.SelectedItems.Cast(Of clsMovie), Now.AddYears(-3))
    End Sub

    Private Sub ChangeWatched(movies As IEnumerable(Of clsMovie), cas As Date)
        If Not movies.Count = 0 Then movies.ToList.ForEach(Sub(x) x.SetWatched(cas))
    End Sub

    Private Sub miFolder_Click(sender As Object, e As RoutedEventArgs) Handles miFolder.Click
        If ListView1.SelectedItem IsNot Nothing Then
            myFile.Launch(Me, myFile.Path(CType(ListView1.SelectedItem, clsMovie).Path))
        End If
    End Sub

    Private Sub miName_Click(sender As Object, e As RoutedEventArgs) Handles miName.Click
        If ListView1.SelectedItem IsNot Nothing Then
            Try
                Clipboard.SetText(CType(ListView1.SelectedItem, clsMovie).Name)
            Catch
            End Try
        End If
    End Sub

    Private Sub miRefresh_Click(sender As Object, e As RoutedEventArgs) Handles miRefresh.Click
        If ListView1.SelectedItem IsNot Nothing Then
            VisibleItems.Clear()
            For Each item In ListView1.SelectedItems
                Dim movie = CType(item, clsMovie)
                movie.Refresh()
                movie.Length = Nothing
                VisibleItems.Add(movie)
            Next
            bgwDelka = New System.ComponentModel.BackgroundWorker
            bgwDelka.RunWorkerAsync()
        End If
    End Sub

#End Region

#Region " Style "

    Private Sub btnGrid_Click(sender As Object, e As RoutedEventArgs) Handles btnGrid.Click
        MainGrid.ColumnDefinitions(0).Width = New GridLength(780, GridUnitType.Pixel)
        ListView1.Style = DirectCast(Me.FindResource("GridViewStyle"), Style)
        ScrollHorizontaly = False
    End Sub

    Private Sub btnWrap_Click(sender As Object, e As RoutedEventArgs) Handles btnWrap.Click
        MainGrid.ColumnDefinitions(0).Width = New GridLength(90, GridUnitType.Star)
        MainGrid.ColumnDefinitions(2).Width = New GridLength(10, GridUnitType.Star)
        ListView1.Style = DirectCast(Me.FindResource("WrapPanelStyle"), Style)
        ScrollHorizontaly = True
    End Sub

    Private ScrollHorizontaly As Boolean
    Private Sub TS_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        If ScrollHorizontaly = False Then Exit Sub
        Dim Scroll As ScrollViewer = TryCast(sender, ScrollViewer)
        If e.Delta > 0 Then
            Scroll.LineLeft()
        Else
            Scroll.LineRight()
        End If
        e.Handled = True
    End Sub

#End Region

#Region " Filters "
    Private Filtrovat As Boolean
    Private Sub cbxDruh_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxDruh.SelectionChanged
        If Filtrovat = False Then Exit Sub

        Nastaveni.DruhSelected = cbxDruh.SelectedItem.ToString
        LoadMovies()
    End Sub
    Private Sub cbxZanr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxZanr.SelectionChanged, cbxRozliseni.SelectionChanged, cbxVidel.SelectionChanged
        If Filtrovat = False Then Exit Sub

        Dim Druh = Nastaveni.Druhy.FirstOrDefault(Function(a) a.Druh = Nastaveni.DruhSelected)
        If Druh Is Nothing Then
            Druh.ZanrSelected = Nothing
        Else
            If cbxZanr.SelectedIndex = 0 Then
                Druh.ZanrSelected = Nothing
            Else
                Druh.ZanrSelected = cbxZanr.SelectedItem.ToString
            End If
        End If
        Prefiltrovat()
    End Sub

    Private Sub Prefiltrovat()
        Dim Druh = Nastaveni.Druhy.FirstOrDefault(Function(a) a.Druh = Nastaveni.DruhSelected) 'načíst hlavní vybranou složku       
        If Druh Is Nothing And NactenoZeSouboru = "" Then 'databáze je prázná, není hlavní složka s filmy
            ListView1.ItemsSource = myMovies.OrderBy(Function(x) x.Theme).ThenBy(Function(y) y.Name)
            Me.Title = MeTitle & Space(30) & myMovies.Count & " videí" & NactenoZeSouboru
            Exit Sub
        End If

        Dim Movies As IEnumerable(Of clsMovie) = myMovies
        If NactenoZeSouboru = "" Then Movies = Movies.Where(Function(x) x.RootDir = Druh.Druh)
        If Not cbxZanr.SelectedIndex = 0 Then Movies = Movies.Where(Function(x) x.Theme = Druh.ZanrSelected)
        If Not cbxRozliseni.SelectedIndex = 0 Then Movies = Movies.Where(Function(x) x.Resolution = CType(cbxRozliseni.SelectedItem, Quality))
        If Not cbxVidel.SelectedIndex = 0 Then
            If cbxVidel.SelectedIndex = 1 Then
                Movies = Movies.Where(Function(x) x.Watched < Now.AddYears(1 - cbxVidel.SelectedIndex) And x.Watched > Now.AddYears(0 - cbxVidel.SelectedIndex))
            Else 'před rokem a více lety, před dvěma a více lety ....
                Movies = Movies.Where(Function(x) x.Watched < Now.AddYears(1 - cbxVidel.SelectedIndex))
            End If
        End If
        ListView1.ItemsSource = Movies.OrderBy(Function(x) x.Theme).ThenBy(Function(y) y.Name)
        Me.Title = MeTitle & Space(30) & Movies.Count & " videí" & NactenoZeSouboru

        'předat extra hodnotu do DateConverteru přes vteřiny
        If Not Movies.Count = 0 Then
            Movies.Where(Function(x) x.Watched.Second = 1).ToList.ForEach(Sub(x) x.Watched = New Date(x.Watched.Year, x.Watched.Month, x.Watched.Day, x.Watched.Hour, x.Watched.Minute, 0))
            Dim latestWatched = Movies.Max(Function(y) y.Watched) 'naposledy shlédnuto
            'naposledy shlédnuto bude mít ve vteřině číslo 1
            Movies.Where(Function(x) x.Watched = latestWatched).ToList.ForEach(Sub(x) x.Watched = New Date(x.Watched.Year, x.Watched.Month, x.Watched.Day, x.Watched.Hour, x.Watched.Minute, 1))
        End If

        StartUpdateVisible(Movies)
    End Sub

#End Region

#Region " Sorting "

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Dim Header As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)
        If Header Is Nothing Then Exit Sub
        If Header.Column Is Nothing Then Exit Sub
        If Header.Tag Is Nothing Then Header.Tag = "D"
        If Header.Tag.ToString = "D" Then
            Header.Tag = "A"
        Else
            Header.Tag = "D"
        End If
        Select Case Header.Column.Header.ToString()
            Case "Žánr"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Theme)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Theme)
                End If
            Case "Video"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Name)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Name)
                End If
            Case "Délka"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Length)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Length)
                End If
            Case "Rozlišení"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Resolution)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Resolution)
                End If
            Case "Zhlédnuto"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Watched)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Watched)
                End If
            Case "Vytvořeno"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Created)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Created)
                End If
            Case "Kontejner"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Extension)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Extension)
                End If
            Case "GiB"
                If Header.Tag.ToString = "D" Then
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderBy(Function(x) x.Size)
                Else
                    ListView1.ItemsSource = ListView1.ItemsSource.Cast(Of clsMovie).OrderByDescending(Function(x) x.Size)
                End If
        End Select
    End Sub

#End Region

#Region " Najít "

    Private Sub btnFind_Click(sender As Object, e As RoutedEventArgs) Handles btnFind.Click
        Najit()
    End Sub

    Private Sub txtFind_KeyUp(sender As Object, e As KeyEventArgs) Handles txtFind.KeyUp
        If e.Key = Key.Enter Then Najit()
    End Sub

    Private Sub Najit()
        Dim Movie As clsMovie = ListView1.ItemsSource.Cast(Of clsMovie).FirstOrDefault(Function(x) x.Name.ToLower.Contains(txtFind.Text.ToLower) And x.Found = False)
        If IsNothing(Movie) Then
            myMovies.Where(Function(x) x.Found = True).ToList.ForEach(Sub(x) SetFalse(x))
        Else
            ListView1.SelectedItem = Movie
            ListView1.ScrollIntoView(Movie)
            Movie.Found = True
        End If
    End Sub

    Private Sub SetFalse(Movie As clsMovie)
        Movie.Found = False
    End Sub

#End Region

#Region " Export "

    Private Sub miExport_Click(sender As Object, e As RoutedEventArgs) Handles miExport.Click
        If ListView1.Items.Count = 0 Then Exit Sub
        Dim ofdMain As New Microsoft.Win32.SaveFileDialog
        ofdMain.Title = "Zvolte místo a název souboru"
        ofdMain.Filter = "Comma-separated values (UTF-8)|*.csv"
        ofdMain.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        ofdMain.FileName = "MoviesUTF8"
        If ofdMain.ShowDialog Then
            If myFile.Delete(ofdMain.FileName, True, False) Then
                Dim sText As String = "Žánr,Film,Rozlišení,Zhlédnuto,Uloženo,Kontejner,GiB" & Chr(10)
                For Each Movie In ListView1.ItemsSource.Cast(Of clsMovie)()
                    sText &= Movie.Theme & "," & Movie.Name & "," & Movie.Resolution.ToString & "," & Movie.Watched.ToShortDateString & "," & Movie.Created.ToShortDateString & "," & Movie.Extension & ",""" & Movie.Size & """" & Chr(10)
                Next
                Dim FileStream As New System.IO.StreamWriter(ofdMain.FileName)
                FileStream.WriteLine(sText)
                FileStream.Close()
                System.Diagnostics.Process.Start(myFile.Path(ofdMain.FileName))
            End If
        End If
    End Sub

#End Region

#Region " Serializace "

    Private Sub miSave_Click(sender As Object, e As RoutedEventArgs) Handles miSave.Click
        If ListView1.Items.Count = 0 Then Exit Sub
        Dim ofdMain As New Microsoft.Win32.SaveFileDialog
        ofdMain.Title = "Zvolte místo a název souboru"
        ofdMain.Filter = "pyramidak Movie Browser|*.pmb"
        ofdMain.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        ofdMain.FileName = "Movies"
        If ofdMain.ShowDialog Then
            Dim xmlStream = New clsSerialization(myMovies, Me).WriteXml()
            Call (New clsEncryption("", Me)).EncryptStream(xmlStream, ofdMain.FileName)
        End If
    End Sub

    Private Sub miLoad_Click(sender As Object, e As RoutedEventArgs) Handles miLoad.Click
        Dim ofdMain As New Microsoft.Win32.OpenFileDialog
        ofdMain.Title = "Zvolte soubor k načtení"
        ofdMain.Filter = "pyramidak Movie Browser|*.pmb"
        ofdMain.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        ofdMain.CheckFileExists = True
        If ofdMain.ShowDialog Then LoadFile(ofdMain.FileName, "", True)
    End Sub

    Private Sub LoadFile(filename As String, password As String, Optional External As Boolean = False)
        NactenoZeSouboru = ""
        Dim xmlStream = New clsEncryption(password, Me).DecryptFile(filename)
        Dim StreamDelka As Long = xmlStream.Length
        myMovies = CType(New clsSerialization(myMovies, Me).ReadXml(xmlStream), clsMovies)
        If Not StreamDelka = 0 And External Then
            NactenoZeSouboru = " databáze " & myFile.Name(filename)
            LoadFilters()
            Prefiltrovat()
        End If
    End Sub

    Private Sub miModify_Click(sender As Object, e As RoutedEventArgs) Handles miModify.Click
        If Not NactenoZeSouboru = "" Then
            Dim Dialog As New wpfDialog(Me, "Máte načtenou databázi. Před použitím opravy datumů Zhlédnuto na disku podle databáze musíte načíst aktuální data z disku.", MeTitle, wpfDialog.Ikona.varovani, "Zavřít")
            Dialog.ShowDialog()
            Exit Sub
        End If
        'Zapíše datumy zhlédnuto filmů z databáze k aktuální databázi filmu zobrazených
        Dim ofdMain As New Microsoft.Win32.OpenFileDialog
        ofdMain.Title = "Zvolte soubor k načtení dat zhlédnuto"
        ofdMain.Filter = "pyramidak Movie Browser|*.pmb"
        ofdMain.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        ofdMain.CheckFileExists = True
        If ofdMain.ShowDialog = False Then Exit Sub
        Dim xmlStream = New clsEncryption("", Me).DecryptFile(ofdMain.FileName)
        If xmlStream.Length = 0 Then Exit Sub

        Dim Zdroj As New clsMovies
        Zdroj = CType(New clsSerialization(Zdroj, Me).ReadXml(xmlStream), clsMovies)
        If Zdroj.Count = 0 Then Exit Sub

        Dim Movies = ListView1.ItemsSource.OfType(Of clsMovie)
        Dim Pocet As Integer
        For Each Movie In Movies
            Dim Found = Zdroj.FirstOrDefault(Function(a) myFile.Name(a.Path) = myFile.Name(Movie.Path) And a.Theme = Movie.Theme)
            If Found IsNot Nothing Then
                If Movie.Watched.Date <> Found.Watched.Date Then
                    Movie.SetWatched(Found.Watched)
                    Movie.Watched = Found.Watched
                    Pocet += 1
                End If
            End If
        Next

        Dim wDialog As New wpfDialog(Me, "Počet změn v datumech Zhlednuto: " & Pocet, "Provedené změny", wpfDialog.Ikona.ok, "OK")
        wDialog.ShowDialog()
    End Sub

#End Region

#Region " timZJS "
    Private Sub timZJS_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles timZJS.Tick
        Try
            If ZJShareMem.DataExists Then
                If ZJShareMem.Peek() = "ZJS:" & Application.ExeName & ":EXIT" Then
                    Try
                        Me.Close()
                    Catch
                        End
                    End Try
                End If
            End If
        Catch
            timZJS.Stop()
        End Try
    End Sub

#End Region

#Region " MKVtoolnix "

#Region " Update Visible "

    Private Sub StartUpdateVisible(Movies As IEnumerable(Of clsMovie)) 'MKVToolNix extrakce délky videa
        VisibleItems = Movies.Where(Function(a) a.Length Is Nothing OrElse a.Length = "0?:??").ToList 'AND a.Extension = ".mkv"
        If Not VisibleItems.Count = 0 Then
            bgwDelka = New System.ComponentModel.BackgroundWorker
            bgwDelka.RunWorkerAsync()
        End If
    End Sub

#End Region

#Region " Cover "
    Private WithEvents bgwCover As System.ComponentModel.BackgroundWorker
    Private SelectedItem As clsMovie

    Private Sub bgwCover_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgwCover.DoWork
        If myFile.Delete(TempCover, False, False) Then
            Dim Proces As New Process()
            Proces.StartInfo.FileName = myFile.Join(MKVextract, "mkvextract.exe")
            Proces.StartInfo.Arguments = Chr(34) & SelectedItem.Path & Chr(34) & " attachments " & Chr(34) & "1:" & TempCover & Chr(34)
            Proces.StartInfo.CreateNoWindow = True
            Proces.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            Proces.Start()
            Proces.WaitForExit()
        End If
    End Sub

    Private Sub bgwCover_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgwCover.RunWorkerCompleted
        If myFile.Exist(TempCover) Then
            ShowCover(TempCover)
        Else
            Dim FolderCover As String = myFile.Join(myFile.Path(SelectedItem.Path), "cover.jpg")
            If myFile.Exist(FolderCover) Then ShowCover(FolderCover)
        End If
    End Sub

    Private Sub ShowCover(File As String)
        Dim Img As New BitmapImage
        Img.BeginInit()
        Img.CacheOption = BitmapCacheOption.OnLoad
        Img.CreateOptions = BitmapCreateOptions.IgnoreImageCache
        Img.UriSource = New Uri(File)
        Img.EndInit()
        imgCover.Source = Img
    End Sub

#End Region

#Region " Délka a rozlišení "

    Private WithEvents bgwDelka As System.ComponentModel.BackgroundWorker
    Private VisibleItems As New List(Of clsMovie)
    Private Video As clsDirectShow
    Private Sub bgwDelka_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgwDelka.DoWork
        If VisibleItems(0).Extension = ".mkv" Then
            If MKVexists AndAlso myFile.Delete(TempInfo, False, False) Then
                Dim Proces As New Process()
                Proces.StartInfo.FileName = myFile.Join(MKVextract, "mkvinfo.exe")
                Proces.StartInfo.Arguments = Chr(34) & VisibleItems(0).Path & Chr(34) & " -r " & Chr(34) & TempInfo & Chr(34)
                Proces.StartInfo.CreateNoWindow = True
                Proces.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                Proces.Start()
                Proces.WaitForExit()
            End If
        Else
            Video = New clsDirectShow(VisibleItems(0).Path)
        End If
    End Sub

    Private Sub bgwDelka_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgwDelka.RunWorkerCompleted
        Dim VideoWidth As Integer
        If VisibleItems(0).Extension = ".mkv" Then
            If MKVexists And myFile.Exist(TempInfo) AndAlso Not VisibleItems.Count = 0 Then
                Dim Info As String = myString.FromFile(TempInfo)
                'Duration videa
                Dim Starts As Integer = Info.IndexOf("Duration")
                If Not Starts = -1 Then
                    Starts = Info.IndexOf(" ", Starts)
                    VisibleItems(0).Length = Info.Substring(Starts + 1, 5)
                Else 'nový český překlad
                    Starts = Info.IndexOf("Trvání")
                    If Not Starts = -1 Then
                        Starts = Info.IndexOf(" ", Starts)
                        VisibleItems(0).Length = Info.Substring(Starts + 1, 5)
                    Else 'stará verze
                        Starts = Info.IndexOf("Délka")
                        If Starts = -1 Then Info.IndexOf("Length")
                        If Not Starts = -1 Then
                            Starts = Info.IndexOf("(", Starts)
                            VisibleItems(0).Length = Info.Substring(Starts + 1, 5)
                        End If
                    End If
                End If

                'Rozlišení videa
                Dim DisplayWidth As String = "320"
                Starts = Info.IndexOf("Pixel width")
                If Not Starts = -1 Then
                    Starts = Info.IndexOf(" ", Starts + 10)
                    Dim Ends As Integer = Info.IndexOf(Chr(13), Starts)
                    DisplayWidth = Info.Substring(Starts + 1, Ends - Starts - 1)
                Else
                    Starts = Info.IndexOf("Display width")
                    If Not Starts = -1 Then
                        Starts = Info.IndexOf(" ", Starts + 10)
                        Dim Ends As Integer = Info.IndexOf(Chr(13), Starts)
                        DisplayWidth = Info.Substring(Starts + 1, Ends - Starts - 1)
                    Else 'český překlad
                        Starts = Info.IndexOf("Šířka pixelu")
                        If Not Starts = -1 Then
                            Starts = Info.IndexOf(" ", Starts + 10)
                            Dim Ends As Integer = Info.IndexOf(Chr(13), Starts)
                            DisplayWidth = Info.Substring(Starts + 1, Ends - Starts - 1)
                        End If
                    End If
                End If
                If IsNumeric(DisplayWidth) Then VideoWidth = CInt(DisplayWidth)
            End If
        Else
            VisibleItems(0).Length = Video.Delka
            VideoWidth = Video.Sirka
        End If

        If VisibleItems.Count > 0 Then
            If Not VisibleItems(0).Resolution = Quality._3D Then
                If VideoWidth >= 700 And VideoWidth < 1200 Then
                    VisibleItems(0).Resolution = Quality.DVD
                End If
                If VideoWidth >= 1200 And VideoWidth < 1900 Then
                    VisibleItems(0).Resolution = Quality.HDready
                End If
                If VideoWidth >= 1900 And VideoWidth < 3800 Then
                    VisibleItems(0).Resolution = Quality.FullHD
                End If
                If VideoWidth >= 3800 And VideoWidth < 3900 Then
                    VisibleItems(0).Resolution = Quality.UltraHD
                End If
            End If

            VisibleItems.Remove(VisibleItems(0))
            If Not VisibleItems.Count = 0 Then
                bgwDelka = New System.ComponentModel.BackgroundWorker
                bgwDelka.RunWorkerAsync()
            End If
        End If
    End Sub

#End Region

#Region " Instalovaný? "
    Private Sub SearchUninstall()
        Dim mThread As New System.Threading.ThreadStart(AddressOf CheckUninsFolder)
        Dim oThread As New System.Threading.Thread(mThread)
        oThread.Start()
    End Sub

    Private Sub CheckUninsFolder()
        CheckUninsRegister("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") 'x64
        CheckUninsRegister("SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall") 'x32
    End Sub

    Private Sub CheckUninsRegister(ByVal RegPath As String)
        For iRoot = 2 To 4 Step 2
            Dim RegKeys() As String = myRegister.QueryKeys(CType(iRoot, HKEY), RegPath)
            For Each oneKey As String In RegKeys
                'Application.MKVextract globální proměnná kvůli vláknu
                If LCase(oneKey) = MKVextract Then
                    MKVextract = myFolder.Path(myRegister.GetValue(CType(iRoot, HKEY), RegPath & "\" & oneKey, "UninstallString", "mkvtoolnix"))
                    MKVexists = myFile.Exist(myFile.Join(MKVextract, "mkvinfo.exe"))
                    Exit Sub
                End If
            Next
        Next
    End Sub


#End Region

#End Region

End Class


#Region " DateConverter "

<ValueConversion(GetType(DateTime), GetType(Brush))>
Public Class clsDateConverter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim Date1 = DirectCast(value, Date)
        If Date1.Second = 1 Then
            Return Brushes.Orange
        ElseIf Date1 < Now.AddYears(-3) Then
            Return Brushes.Yellow
        ElseIf Date1 < Now.AddYears(-2) Then
            Return Brushes.GreenYellow
        ElseIf Date1 < Now.AddYears(-1) Then
            Return Brushes.LightGreen
        Else
            Return Brushes.LightGray
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

#End Region