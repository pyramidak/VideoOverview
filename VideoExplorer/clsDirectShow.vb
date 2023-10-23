Imports DirectShowLib
Imports DirectShowLib.DES
Imports System.Runtime.InteropServices

Public Class clsDirectShow

    Public Sekund As TimeSpan
    Public Delka As String
    Public Rozliseni As String
    Public Sirka As Integer
    Public Vyska As Integer
    Public SnimkovaFrekvence As Integer

    Sub New(FileName As String)
        Dim mediaDet = CType(New MediaDet(), IMediaDet)
        Dim frameRate As Double
        Try
            DsError.ThrowExceptionForHR(mediaDet.put_Filename(FileName))
            Dim index As Integer = 0
            Dim type As Guid
            type = Guid.Empty

            While index < 1000 AndAlso type <> MediaType.Video
                mediaDet.put_CurrentStream(index)
                mediaDet.get_StreamType(type)
                index += 1
            End While

            mediaDet.get_FrameRate(frameRate)
            SnimkovaFrekvence = CInt(frameRate)
        Catch ex As Exception
            SnimkovaFrekvence = 25
            Sirka = 320 : Vyska = 240
            Delka = ""
            Exit Sub
        End Try

        Try
            Dim mediaTyp = New AMMediaType()
            mediaDet.get_StreamMediaType(mediaTyp)
            Dim videoInfo = CType(Marshal.PtrToStructure(mediaTyp.formatPtr, GetType(VideoInfoHeader)), VideoInfoHeader)
            DsUtils.FreeAMMediaType(mediaTyp)
            Sirka = videoInfo.BmiHeader.Width
            Vyska = videoInfo.BmiHeader.Height
            Rozliseni = Sirka.ToString & "x" & Vyska.ToString
        Catch ex As Exception
            Sirka = 320 : Vyska = 240
            Rozliseni = "?x?"
        End Try

        Try
            Dim mediaLength As Double
            mediaDet.get_StreamLength(mediaLength)
            Dim frameCount = CInt((frameRate * mediaLength))
            Dim duration = frameCount / frameRate
            Sekund = TimeSpan.FromSeconds((CLng(duration)))
            Delka = Sekund.ToString("hh\:mm")
        Catch ex As Exception
            Delka = ""
        End Try

    End Sub

End Class
