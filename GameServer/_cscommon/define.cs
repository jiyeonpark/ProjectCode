using System;

namespace WCS
{
    public class define
    {
        public static readonly string Version = "1.0.0";
        public static int TAG_FROM_ERROR_CODE(int error_code)
        {
            return ((error_code >> 17) << 17);
        }

        public enum eServerType
        {
            none            = 0x00000000,
            gmmanager       = 0x00000001,
            gmobserver      = 0x00000002,
            database        = 0x00000004,
            match           = 0x00000008,
            scheduler       = 0x00000010,
            login           = 0x00000020,
            lobby           = 0x00000040,
            community       = 0x00000200,
            game            = 0x00001000,
            all             = 0x0FFFFFFF,
        }

        public static readonly string[] SERVER_TAG_NAME = { "", "gmmanager", "gmobserver", "database", "match", "scheduler", "login", "lobby", "community", "game" };

        public static readonly int DEFAULT_EXCEPTION_POOL = 512;
        public static readonly int MAX_GAMEOPTION_BUFFER = 512;         // database size, Ŭ���̾�Ʈ �ɼ� ���� ���� �ִ� ũ��
        public static readonly int MAX_QUESTHISTORY_BUFFER = 4 * 300;      // database size, ����Ʈ �����丮 ���� �ִ� ũ�� (sizeof(int) * max history)

        public static readonly int MAX_TEAM_SLOT = 1;              // database size, ������ �ִ� ĳ���� ��
        public static readonly int DEFINE_SIZE_INT32 = 4;              // int ũ�� ����
        public static readonly int MAIL_ITEM_COUNT = 5;


        public static readonly int MAX_LINEUP_MEMBER = 6;                // �������� �����
        public static readonly int MAX_BATTER_MEMBER = 5;                // ���� ��
        public static readonly int MAX_FENCE_COUNT = 16;                 // �潺 �Ѽ�

        public static readonly int RANK_SLOT_MAX = 100;
        public static readonly int RANK_SCROLL_SLOT_MAX = 10;


    }

    public enum MatchState
    {
        None,
        MatchStart,
        MatchCancel,
    }

    public enum PoolingState
    {
        None,
        Pooling,
        BattleStart,
        MatchCancel
       
    }
    public enum GameState
    {
        None = -1,

        Start,
        Lobby,
        InGame,
    }

    public enum ResourceState
    {
        None = -1,

        Common,     // ����
        OutGame,    // �ƿ����ӿ�
        InGame,     // �ΰ��ӿ�
    }

    public enum TeamPosition
    {
        None = -1,
        Away,
        Home,
        Max
    }

    public enum PlayTurn
    {
        None = -1,

        Defence,
        Offense,

        Max
    }

    public enum PlayResult
    {
        None = -1,

        Win,
        Lose,
        Draw,

        Abandon,    // ����
    }

    public enum BallState
    {
        None,
        PitchReady,     // �غ�
        PitchCompleted, // �غ�Ϸ�
        PitchingCall,   // Input �Է� ����
        Pitching,       // ������ ���� ������ ����
        PitArrive,
        Hit,
        HitArrive,
    }

    public enum TeamPlayerPos
    {
        None = -1,

        B1,
        //B2,
        B3,
        //SS,
        LF,
        CF,
        RF,

        Pitcher,
        Hitter,

        RunH,
        Run1B,
        Run2B,
        Run3B,

        Max,
    }

    public enum RunnerType
    {
        Run_Hitter,
        Run_1B,
        Run_2B,
        Run_3B,
    }

    public enum DefPosType
    {
        move = 0,
        fix_pos1,
        fix_pos2,
        fix_pos3,
        fix_pos4,
        fix_pos5,
    }

    public enum FenceIdx
    {
        OUT = 0,
        B1,
        B2,
        B3,
        HOMERUN,
        Random
    }

    public enum BallResult
    {
        None = 0,
        Strike,
        Ball,
        Hit,
        Out,
        Foul,
        Homerun,
    }

    public enum UIResult
    {
        Strike = 0,
        StrikeOut,
        Ball,
        BaseOnBalls,
        HitByPitch,
        Hit,
        Double,
        Triple,
        Homerun,
        Foul,
        Out,
        Change,
        Score,
        Bottom,
        Top,
        PlayBall,
        GameSet,
        Timer,
        Inning,

        Max,
    }

    public enum UniformIdx
    {
        Cap = 0,
        Helmet,
        Body,

        Max,
    }

    public enum TeamIdx
    {
        Aqua = 0,
        Bulls,
        Deers,
        Dragons,
        Fireball,
        Marines,
        Stars,
        Storm,
        Thunder,
        Twins,
        Unicorns,
        Volcano,
        Wizards,

        Max,
    }

    public enum UserNationFlag
    {
        None = -1,
        Australia,
        Brazil,
        Canada,
        China,
        Colombia,
        Cuba,
        Dominicana,
        Espana,
        Islael,
        Italy,
        Japan,
        Korea,
        Maxico,
        Nederland,
        Panama,
        PuertoRico,
        Taiwan,
        US,
        Venezuela,
        SouthAfrica,

        Max
    }

    public enum LeagueType
    {
        None = -1,
        Beginner = 1,
        Rookie,
        Single,
        Double,
        Triple,
        SemiPro,
        Pro,

        Max,
    }

    public enum InGameState
    {
        none = 0,
        ready,              // �غ�
        wait,               // ���
        join_complate,      // ���� ��� ����
        load_complate,      // �ε� ���ø�
        play,               // �÷���
        game_end,           // ���			
        finish,             // ��� (����/����)
        max
    };
}

