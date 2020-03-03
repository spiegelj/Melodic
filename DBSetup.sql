-- One-time setup.  
USE Melodic
GO

CREATE TABLE Song(
	SongId int IDENTITY PRIMARY KEY,
	Title varchar(255) NOT NULL,
	Artist varchar(255) NULL,
	Lyrics varchar(max) NULL
)

CREATE TABLE Playlist(
	PlaylistId int IDENTITY PRIMARY KEY,
	Name varchar(255) NOT NULL UNIQUE,
	Description varchar(max) NULL)

CREATE TABLE PlaylistSong(
	PlaylistSongId int IDENTITY PRIMARY KEY,
	PlaylistId int NOT NULL CONSTRAINT [FK_PlaylistSong_PlaylistId] FOREIGN KEY(PlaylistId)
		REFERENCES Playlist(PlaylistId),
	SongId int NOT NULL CONSTRAINT [FK_PlaylistSong_SongId] FOREIGN KEY(SongId)
		REFERENCES Song(SongId)
	)


GO
CREATE TABLE MelodicLog(
	LogId int IDENTITY PRIMARY KEY,
	EventTime datetime NOT NULL,
	Message varchar(max) NOT NULL
)

GO
-- Start on basic CRUD
-- Update or insert the song if it is or is not located by ID, respectively
CREATE PROCEDURE SaveSong
	@SongId int,
    @Title varchar(255),
	@Artist varchar(255),
	@Lyrics varchar(255)
AS
MERGE Song T
    USING (SELECT @SongId AS SongId, @Title AS Title, @Artist AS Artist, @Lyrics AS Lyrics) S
ON (S.SongId = T.SongId)
WHEN MATCHED
    THEN UPDATE SET 
        T.Title = S.Title,
		T.Artist = S.Artist,
		T.Lyrics = S.Lyrics
WHEN NOT MATCHED
    THEN INSERT (Title, Artist, Lyrics)
         VALUES (S.Title, S.Artist, S.Lyrics);

GO

-- Delete a song
CREATE PROCEDURE DeleteSong
	@SongId int
AS

DECLARE @AffectedRows int = 0
DELETE FROM Song WHERE SongId = @SongId
SET @AffectedRows = @@ROWCOUNT

SELECT @AffectedRows


GO

-- DROP PROCEDURE SavePlaylist
-- Update or insert the song if it is or is not located by ID, respectively
CREATE PROCEDURE SavePlaylist
	@PlaylistId int,
    @Name varchar(255),
	@Description varchar(255),
	@SongList varchar(max)
AS

-- Update the playlist definition.  Not using MERGE because we may need to grab the identity on insert.
-- While there is a surrogate key, we also need to ensure uniqueness of the name
IF EXISTS(SELECT 1 FROM Playlist WHERE Name = @Name)
	-- Don't allow another playlist with an existing name
	SELECT @PlaylistId = PlaylistId
		FROM Playlist
		WHERE Name = @Name

IF NOT EXISTS(SELECT 1 FROM Playlist WHERE PlaylistId = @PlaylistId)
BEGIN
	-- Didn't match by provided ID or name
	INSERT INTO Playlist (Name, Description)
		VALUES (@Name, @Description)
	SET @PlaylistId = @@IDENTITY
END
ELSE
	UPDATE Playlist
		SET Name = @Name,
			Description = @Description
		WHERE PlaylistId = @PlaylistId

PRINT @PlaylistId

	SELECT @PlaylistId AS PlaylistId, songId AS SongId
		FROM OPENJSON(@SongList)
		WITH (songId int)

-- Update the songs in playlist
;WITH cteSongId AS
(
	SELECT @PlaylistId AS PlaylistId, songId AS SongId
		FROM OPENJSON(@SongList)
		WITH (songId int)
)
MERGE PlaylistSong T
    USING (SELECT @PlaylistId AS PlaylistId, SongId
				FROM cteSongId ) S
ON (S.PlaylistId = T.PlaylistId
	AND S.SongId = T.SongId)
WHEN NOT MATCHED BY TARGET
    THEN INSERT (PlaylistId, SongId)
         VALUES (S.PlaylistId, S.SongId)
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;

SELECT @PlaylistId



GO
CREATE PROCEDURE InsertPlaylist
	@Name varchar(255),
	@Description varchar(max) = NULL
AS

IF NOT EXISTS(SELECT 1 FROM Playlist WHERE Name = @Name AND Description = @Description)
	INSERT INTO Playlist (Name, Description)
		VALUES (@Name, @Description)


GO
CREATE PROCEDURE InsertPlaylistSong
	@PlaylistId int,
	@SongId int
AS

IF NOT EXISTS(SELECT 1 FROM PlaylistSong WHERE PlaylistId = @PlaylistId AND SongId = @SongId)
	INSERT INTO PlaylistSong (PlaylistID, SongId)
		VALUES (@PlaylistID, @SongId)


GO
CREATE PROCEDURE GetLibrary
AS 
SELECT SongId, Title, Artist, Lyrics
	FROM Song


--DROP PROCEDURE GetSong
GO
CREATE PROCEDURE GetSong
	@SongId AS int
AS
SELECT SongId, Title, Artist, Lyrics
	FROM Song
	WHERE SongId = @SongId


GO
-- Get all playlists a song is a member of
CREATE PROCEDURE GetSongPlaylist
	@SongId int
AS
SELECT P.PlaylistId, P.Name, P.Description
	FROM Playlist P
		JOIN PlaylistSong PS ON PS.PlaylistId = P.PlaylistId
		JOIN Song S ON S.SongId = PS.SongId
			AND S.SongId = @SongId


--DROP PROCEDURE GetPlaylists
GO
CREATE PROCEDURE GetPlaylists
AS
SELECT PlaylistId, Name, Description
	FROM Playlist


GO
CREATE PROCEDURE GetPlaylist
	@PlaylistId int
AS
SELECT PlaylistId, Name, Description
	FROM Playlist
	WHERE PlaylistId = @PlaylistId


GO
CREATE PROCEDURE GetPlaylistSongs
	@PlaylistId int
AS
SELECT S.SongId, S.Title, S.Artist, S.Lyrics
	FROM Song S
		JOIN PlaylistSong PS ON PS.SongId = S.SongId
		JOIN Playlist P ON P.PlaylistId = PS.PlaylistId
	WHERE PS.PlaylistId = @PlaylistId




-- Mainly a helper proc for working in the DB directly
-- Note a grown-up app would put more stringent checks on duplication
GO
CREATE PROCEDURE InsertPlaylistSongByName
	@PlaylistName varchar(255),
	@SongTitle varchar(255)
AS

-- See if we can find the song and playlist
;WITH cteKeys AS
(
	SELECT TOP 1 S.SongId, P.PlaylistId -- "Stringent checks" would imply uniqueness of song, for example
		FROM Song S
			JOIN Playlist P ON Title = @SongTitle 
				AND Name = @PlaylistName
	--Non-traditional join.  In this case we want the cartesian product of what's left from what we do filter by.
)
INSERT INTO PlaylistSong (PlaylistId, SongId)
SELECT K.PlaylistId, K.SongId
	FROM cteKeys K
		LEFT JOIN PlaylistSong PS ON PS.PlaylistId = K.PlaylistId
			AND PS.SongId = K.SongId
	WHERE PS.PlaylistSongId IS NULL	-- Only insert if it doesn't already exist


-- Populate Examples
EXEC InsertSong	'Yesterday', 'The Beatles', 'Yesterday all my troubles seemed so far away...'
EXEC InsertSong	'Do Do Do Da Da Da', 'The Police', '...Poets, priests and politicians have words to say for their positions'
EXEC InsertSong 'Smells Like Teen Spirit', 'Nirvana', 'With the lights out, it''s less dangerous'
EXEC InsertSong 'Seven Nation Army', 'The White Stripes', 'I''m going to Witchita'
EXEC InsertSong 'Thrift Shop', 'Macklemore', 'Poppin tags'
EXEC InsertSong 'Feel It Still', 'Portugal.  The Man', 'I''m a rebel just for kicks'
EXEC InsertSong 'Sympathy for the Devil', 'The Rolling Stones', 'Please allow me to introduce myself'

EXEC InsertPlaylist 'Classics', 'Hard to argue these were at least once pretty huge'
EXEC InsertPlaylist 'Grunge', 'Flannel-clad rockers'
EXEC InsertPlaylist '21st Century', 'Not modern, but not old'

EXEC InsertPlaylistSongByName 'Yesterday', 'Classics'
EXEC InsertPlaylistSongByName 'Yesterday', 'Pop'
EXEC InsertPlaylistSongByName 'Sympathy for the Devil', 'Classics'
EXEC InsertPlaylistSongByName 'Feel it Still', '21st Century'

-- DELETE FROM Song
-- DELETE FROM Playlist
SELECT * FROM Song
SELECT * FROM Playlist
SELECT PS.*, P.Name, S.Title
	FROM PlaylistSong PS
		JOIN Playlist P ON P.PlaylistId = PS.PlaylistId
		JOIN Song S ON S.SongId = PS.SongId

