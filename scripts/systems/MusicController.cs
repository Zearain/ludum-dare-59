namespace LudumDare59.Systems;

using Godot;

public partial class MusicController : Node
{
    private const float SilentVolumeDb = -60.0f;
    private const float GameplayVolumeDb = -6.0f;
    private const float CrossfadeDurationSeconds = 1.5f;
    private const float FailFadeOutDurationSeconds = 1.0f;

    private const string IntroLoopPath = "res://assets/music/Dani Stob - A New Begining - Loop.wav";
    private const string MidLoopPath = "res://assets/music/Dani Stob - Beyond The Stars - Loop.wav";
    private const string FinalLoopPath = "res://assets/music/Dani Stob - Unstoppable - Loop.wav";
    private const string VictoryPath = "res://assets/music/Dani Stob - Victory Fanfare.wav";

    private AudioStreamPlayer _playerA = null!;
    private AudioStreamPlayer _playerB = null!;
    private AudioStreamPlayer _activePlayer = null!;
    private AudioStreamPlayer _inactivePlayer = null!;

    private AudioStream _introLoop = null!;
    private AudioStream _midLoop = null!;
    private AudioStream _finalLoop = null!;
    private AudioStream _victory = null!;

    private Tween? _transitionTween;
    private MusicCue _currentCue = MusicCue.None;

    public override void _Ready()
    {
        _playerA = GetNode<AudioStreamPlayer>("PlayerA");
        _playerB = GetNode<AudioStreamPlayer>("PlayerB");
        _activePlayer = _playerA;
        _inactivePlayer = _playerB;

        _playerA.Finished += OnPlayerAFinished;
        _playerB.Finished += OnPlayerBFinished;

        _introLoop = LoadTrack(IntroLoopPath);
        _midLoop = LoadTrack(MidLoopPath);
        _finalLoop = LoadTrack(FinalLoopPath);
        _victory = LoadTrack(VictoryPath);

        _playerA.VolumeDb = SilentVolumeDb;
        _playerB.VolumeDb = SilentVolumeDb;
    }

    public override void _ExitTree()
    {
        if (_playerA is not null)
        {
            _playerA.Finished -= OnPlayerAFinished;
        }

        if (_playerB is not null)
        {
            _playerB.Finished -= OnPlayerBFinished;
        }
    }

    public void OnSectorLoaded(int sectorIndex)
    {
        MusicCue nextCue = GetSectorCue(sectorIndex);
        bool shouldCrossfade = (_currentCue, nextCue) is (MusicCue.StageOneTwo, MusicCue.StageThreeFive)
            or (MusicCue.StageThreeFive, MusicCue.Final);
        PlayCue(nextCue, shouldCrossfade);
    }

    public void OnRunFinished(bool completed)
    {
        if (completed)
        {
            PlayCue(MusicCue.Victory, shouldCrossfade: false);
            return;
        }

        FadeToSilence();
    }

    private void PlayCue(MusicCue cue, bool shouldCrossfade)
    {
        if (_currentCue == cue && _activePlayer.Playing)
        {
            return;
        }

        CancelTransition();

        AudioStream stream = GetStreamForCue(cue);
        if (shouldCrossfade && _activePlayer.Playing)
        {
            StartCrossfade(stream);
            _currentCue = cue;
            return;
        }

        _inactivePlayer.Stop();
        _inactivePlayer.VolumeDb = SilentVolumeDb;

        _activePlayer.Stream = stream;
        _activePlayer.VolumeDb = GameplayVolumeDb;
        _activePlayer.Play();
        _currentCue = cue;
    }

    private void StartCrossfade(AudioStream nextStream)
    {
        _inactivePlayer.Stream = nextStream;
        _inactivePlayer.VolumeDb = SilentVolumeDb;
        _inactivePlayer.Play();

        _transitionTween = CreateTween();
        _transitionTween.SetParallel(true);
        _transitionTween.TweenProperty(_activePlayer, "volume_db", SilentVolumeDb, CrossfadeDurationSeconds);
        _transitionTween.TweenProperty(_inactivePlayer, "volume_db", GameplayVolumeDb, CrossfadeDurationSeconds);
        _transitionTween.SetParallel(false);
        _transitionTween.TweenCallback(Callable.From(FinalizeCrossfade));
    }

    private void FinalizeCrossfade()
    {
        AudioStreamPlayer previousActive = _activePlayer;
        _activePlayer = _inactivePlayer;
        _inactivePlayer = previousActive;

        _inactivePlayer.Stop();
        _inactivePlayer.VolumeDb = SilentVolumeDb;
        _transitionTween = null;
    }

    private void FadeToSilence()
    {
        CancelTransition();
        if (!_activePlayer.Playing)
        {
            _currentCue = MusicCue.None;
            return;
        }

        _transitionTween = CreateTween();
        _transitionTween.TweenProperty(_activePlayer, "volume_db", SilentVolumeDb, FailFadeOutDurationSeconds);
        _transitionTween.TweenCallback(Callable.From(StopAllPlayback));
    }

    private void StopAllPlayback()
    {
        _playerA.Stop();
        _playerB.Stop();
        _playerA.VolumeDb = SilentVolumeDb;
        _playerB.VolumeDb = SilentVolumeDb;
        _transitionTween = null;
        _currentCue = MusicCue.None;
    }

    private void CancelTransition()
    {
        _transitionTween?.Kill();
        _transitionTween = null;
    }

    private static MusicCue GetSectorCue(int sectorIndex)
    {
        if (sectorIndex <= 1)
        {
            return MusicCue.StageOneTwo;
        }

        if (sectorIndex <= 4)
        {
            return MusicCue.StageThreeFive;
        }

        return MusicCue.Final;
    }

    private AudioStream GetStreamForCue(MusicCue cue)
    {
        return cue switch
        {
            MusicCue.StageOneTwo => _introLoop,
            MusicCue.StageThreeFive => _midLoop,
            MusicCue.Final => _finalLoop,
            MusicCue.Victory => _victory,
            _ => _introLoop,
        };
    }

    private static AudioStream LoadTrack(string path)
    {
        AudioStream? stream = GD.Load<AudioStream>(path);
        if (stream is null)
        {
            GD.PushError($"Failed to load music track: {path}");
            return new AudioStreamWav();
        }

        return stream;
    }

    private void OnPlayerAFinished()
    {
        HandlePlayerFinished(_playerA);
    }

    private void OnPlayerBFinished()
    {
        HandlePlayerFinished(_playerB);
    }

    private void HandlePlayerFinished(AudioStreamPlayer player)
    {
        if (player != _activePlayer)
        {
            return;
        }

        if (_currentCue is MusicCue.StageOneTwo or MusicCue.StageThreeFive or MusicCue.Final)
        {
            _activePlayer.Play();
        }
    }

    private enum MusicCue
    {
        None,
        StageOneTwo,
        StageThreeFive,
        Final,
        Victory,
    }
}
