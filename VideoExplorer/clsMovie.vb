Imports System.ComponentModel
Imports System.Collections.ObjectModel

#Region " Movie Item "

Public Class clsMovie
    Implements INotifyPropertyChanged

    Private bFound, bExist As Boolean
    Private sName, sExtension, sPath, sTheme, sLength, sRootDir As String
    Private eRes As Quality
    Private dCreated, dWatched As Date
    Private lSize As Double

#Region " Get/Set "

    <Xml.Serialization.XmlIgnore()> Public Property Exist() As Boolean
        Get
            Return bExist
        End Get
        Set(ByVal value As Boolean)
            bExist = value
        End Set
    End Property

    <Xml.Serialization.XmlIgnore()> Public Property Found() As Boolean
        Get
            Return bFound
        End Get
        Set(ByVal value As Boolean)
            bFound = value
        End Set
    End Property

    Public Property Length() As String
        Get
            Return sLength
        End Get
        Set(ByVal value As String)
            sLength = value
            OnPropertyChanged("Length")
        End Set
    End Property

    Public Property Watched() As Date
        Get
            Return dWatched
        End Get
        Set(ByVal value As Date)
            dWatched = value
            OnPropertyChanged("Watched")
        End Set
    End Property

    Public Property Created() As Date
        Get
            Return dCreated
        End Get
        Set(ByVal value As Date)
            dCreated = value
            OnPropertyChanged("Created")
        End Set
    End Property

    Public Property Resolution() As Quality
        Get
            Return eRes
        End Get
        Set(ByVal value As Quality)
            eRes = value
            OnPropertyChanged("Resolution")
        End Set
    End Property

    Public Property Name() As String
        Get
            Return sName
        End Get
        Set(ByVal value As String)
            sName = value
            OnPropertyChanged("Name")
        End Set
    End Property

    Public Property Theme() As String
        Get
            Return sTheme
        End Get
        Set(ByVal value As String)
            sTheme = value
            OnPropertyChanged("Theme")
        End Set
    End Property

    Public Property Path() As String
        Get
            Return sPath
        End Get
        Set(ByVal value As String)
            sPath = value
        End Set
    End Property

    Public Property RootDir() As String
        Get
            Return sRootDir
        End Get
        Set(ByVal value As String)
            sRootDir = value
        End Set
    End Property

    Public Property Size() As Double
        Get
            Return lSize
        End Get
        Set(ByVal value As Double)
            lSize = value
            OnPropertyChanged("Size")
        End Set
    End Property

    Public Property Extension() As String
        Get
            Return sExtension
        End Get
        Set(ByVal value As String)
            sExtension = value
            OnPropertyChanged("Extension")
        End Set
    End Property
#End Region

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

    Sub New()
    End Sub

    Sub New(fullname As String, rootdir As String, Optional watched As Date = Nothing)
        bExist = True
        sPath = fullname
        sRootDir = rootdir
        sExtension = myFile.Extension(fullname)
        lSize = Math.Round(myFile.Size(fullname) / 1024 / 1024 / 1024, 1)
        dCreated = myFile.DateChange(fullname)
        If watched = DateTime.MinValue Then
            dWatched = myFile.DateOpen(fullname)
        Else
            dWatched = watched
        End If
        sTheme = myFolder.Name(myFile.Path(fullname))
        Dim name As String = myFile.Name(fullname, False)
        Dim Str() As String = Split(name, "_")
        If Str.Length > 1 Then
            Select Case UCase(Str(1))
                Case "3D"
                    eRes = Quality._3D
                Case "DVD"
                    eRes = Quality.DVD
                Case "HD"
                    eRes = Quality.HDready
                Case "FHD"
                    eRes = Quality.FullHD
                Case "UHD"
                    eRes = Quality.UltraHD
                Case Else
                    eRes = Quality.low
            End Select
        End If
        If eRes = Quality.low Then
            If name.Contains("3D") Then
                eRes = Quality._3D
            ElseIf name.Contains("DVD") Then
                eRes = Quality.DVD
            ElseIf name.Contains("720") Then
                eRes = Quality.HDready
            ElseIf name.Contains("1080") Then
                eRes = Quality.FullHD
            ElseIf name.Contains("2160") Then
                eRes = Quality.UltraHD
            Else
                eRes = Quality.low
            End If
        End If
        sName = myFile.GetCleanseName(name)
    End Sub

    Public Sub Refresh()
        Name = myFile.GetCleanseName(myFile.Name(sPath, False))
        Extension = myFile.Extension(sPath)
        Size = Math.Round(myFile.Size(sPath) / 1024 / 1024 / 1024, 1)
        Created = myFile.DateChange(sPath)
        Theme = myFolder.Name(myFile.Path(sPath))
    End Sub

    Public Sub SetWatched(Datum As Date)
        If myFile.DateSetOpen(sPath, Datum) Then Watched = Datum
    End Sub

End Class

#End Region

#Region " Movie List "
Public Class clsMovies
    Inherits ObservableCollection(Of clsMovie)

    Sub New()
    End Sub

    Sub New(fullnames() As String, rootdir As String)
        Me.Clear()
        Add(fullnames, rootdir)
    End Sub

    Overloads Sub Add(fullname As String, rootdir As String)
        Dim sName As String = myFile.GetCleanseName(myFile.Name(fullname, False))
        Dim Movie As clsMovie = Me.FirstOrDefault(Function(x) sName.ToLower.Contains(x.Name.ToLower))
        If Movie Is Nothing Then
            Dim Dates As clsSetting.clsMovieDates = Nastaveni.MoviesRemoved.GetAndRemoveMovie(fullname)
            If Dates Is Nothing Then
                Me.Add(New clsMovie(fullname, rootdir))
            Else
                Me.Add(New clsMovie(fullname, rootdir, Dates.Watched))
            End If
        Else
            Movie.RootDir = rootdir
            Movie.Path = fullname
            Movie.Exist = True
        End If
    End Sub

    Overloads Sub Add(fullnames() As String, rootdir As String)
        If IsNothing(fullnames) Then Exit Sub
        For Each fullname As String In fullnames
            Add(fullname, rootdir)
        Next
    End Sub
    Overloads Sub Remove(Movie As clsMovie)
        Nastaveni.MoviesRemoved.Add(Movie.Name, Movie.Created, Movie.Watched)
        MyBase.Remove(Movie)
    End Sub

End Class

#End Region

#Region " Nastavení "
Public Class clsSetting

    Public MoviesRemoved As New clsMovieRemoved
    Private selDruh As String
    Public Property DruhSelected As String
        Get
            Return selDruh
        End Get
        Set(ByVal value As String)
            selDruh = value
        End Set
    End Property

#Region " Druh "
    Public Class clsDruh
        Private sDruh As String
        Private sTypy As String
        Private sZanr As String
        Public Property Druh() As String
            Get
                Return sDruh
            End Get
            Set(ByVal value As String)
                sDruh = value
            End Set
        End Property
        Public Property Pripony() As String
            Get
                Return sTypy
            End Get
            Set(ByVal value As String)
                sTypy = value
            End Set
        End Property

        Public Property ZanrSelected() As String
            Get
                Return sZanr
            End Get
            Set(ByVal value As String)
                sZanr = value
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(name As String, typ As String)
            sDruh = name
            sTypy = typ
        End Sub
    End Class

    Public Class clsDruhy
        Inherits ObservableCollection(Of clsDruh)

        Sub New()
        End Sub

        Overloads Sub Add(druh As String, pripony As String)
            If druh = "" Then Exit Sub
            If pripony = "" Then pripony = "*.mkv;*.mpg;*.mp4;*.wmv;*.avi;*.mov"
            Me.Add(New clsDruh(druh, pripony))
        End Sub
    End Class

    Private cDruhy As New clsDruhy
    Public Property Druhy As clsDruhy
        Get
            Return cDruhy
        End Get
        Set(ByVal value As clsDruhy)
            cDruhy = value
        End Set
    End Property

#End Region

#Region " Folder "

    Public Class clsFolder
        Private sPath As String
        Private sDruh As String

        Public Property Folder() As String
            Get
                Return sPath
            End Get
            Set(ByVal value As String)
                sPath = value
            End Set
        End Property

        Public Property Druh() As String
            Get
                Return sDruh
            End Get
            Set(ByVal value As String)
                sDruh = value
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(path As String, druh_ As String)
            sPath = path
            sDruh = druh_
        End Sub
    End Class

    Public Class clsFolders
        Inherits ObservableCollection(Of clsFolder)

        Sub New()
        End Sub

        Overloads Sub Add(path As String, druh As String)
            If path = "" Then Exit Sub
            Me.Add(New clsFolder(path, druh))
        End Sub
    End Class

    Private Folders As New clsFolders
    Public Property Slozky() As clsFolders
        Get
            Return Folders
        End Get
        Set(ByVal value As clsFolders)
            Folders = value
        End Set
    End Property

#End Region

#Region " Date Item "

    Public Class clsMovieDates
        Private sName As String
        Private dCreated, dWatched As Date

#Region " Get/Set "

        Public Property Watched() As Date
            Get
                Return dWatched
            End Get
            Set(ByVal value As Date)
                dWatched = value
            End Set
        End Property

        Public Property Created() As Date
            Get
                Return dCreated
            End Get
            Set(ByVal value As Date)
                dCreated = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return sName
            End Get
            Set(ByVal value As String)
                sName = value
            End Set
        End Property
#End Region

        Sub New()
        End Sub

        Sub New(fullname As String)
            sName = myFile.GetCleanseName(myFile.Name(Name, False))
            dCreated = myFile.DateChange(fullname)
            dWatched = myFile.DateOpen(fullname)
        End Sub

        Sub New(name As String, created As Date, watched As Date)
            sName = myFile.GetCleanseName(name)
            dCreated = created
            dWatched = watched
        End Sub
    End Class

#End Region

#Region " Date List "

    Public Class clsMovieRemoved
        Inherits Collection(Of clsMovieDates)
        Sub New()
        End Sub

        Sub New(fullnames() As String)
            Me.Clear()
            Add(fullnames)
        End Sub
        Overloads Sub Add(sName As String, dCreated As Date, dWatched As Date)
            Dim Movie As clsMovieDates = Me.FirstOrDefault(Function(x) sName.ToLower.Contains(x.Name.ToLower))
            If Movie Is Nothing Then
                Me.Add(New clsMovieDates(sName, dCreated, dWatched))
            End If
        End Sub

        Overloads Sub Add(fullname As String)
            Dim sName As String = myFile.GetCleanseName(myFile.Name(fullname, False))
            Dim Movie As clsMovieDates = Me.FirstOrDefault(Function(x) sName.ToLower.Contains(x.Name.ToLower))
            If Movie Is Nothing Then
                Me.Add(New clsMovieDates(fullname))
            End If
        End Sub

        Overloads Sub Add(fullnames() As String)
            If IsNothing(fullnames) Then Exit Sub
            For Each fullname As String In fullnames
                Add(fullname)
            Next
        End Sub

        Public Function GetMovie(fullname As String) As clsMovieDates
            Dim sName As String = myFile.GetCleanseName(myFile.Name(fullname, False)) 'čisté jméno.
            Dim Dates As clsSetting.clsMovieDates = Nastaveni.MoviesRemoved.FirstOrDefault(Function(x) sName.ToLower.Contains(x.Name.ToLower)) 'existuje v záloze?
            Return Dates
        End Function

        Public Function GetAndRemoveMovie(fullname As String) As clsMovieDates
            Dim Movie As clsSetting.clsMovieDates = GetMovie(fullname)
            If Movie IsNot Nothing Then Nastaveni.MoviesRemoved.Remove(Movie)
            Return Movie
        End Function

    End Class

#End Region

End Class

#End Region
