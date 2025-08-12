public static class GameConstants
{
    // Scene Names
    public const string SCENE_LOSE = "Lose";
    public const string SCENE_REAL = "SceneReal";
    public const string SCENE_REAL_1 = "SceneReal 1";
    public const string SCENE_MENU = "Menu";

    // UI Formats
    public const string TIMER_FORMAT = "{0:00}:{1:00}";
    public const string ALIVE_TEXT_FORMAT = "Alive: {0}";

    // Animation Parameters
    public const string ANIM_VICTORY = "Victory";
    public const string ANIM_ATTACK = "Attack";
    public const string ANIM_RUNNING = "Running";
    public const string ANIM_DEATH = "Death";
    public const string ANIM_IDLE = "Idle";

    // Tags
    public const string TAG_AI = "AI";
    public const string TAG_PLAYER = "Player";

    // AI Settings
    public const float AI_DETECTION_RANGE = 50f;
    public const float AI_MAX_DISTANCE = 200f;
    public const float AI_TARGET_SWITCH_COOLDOWN = 1.5f;
    public const float ATTACK_COOLDOWN = 1.5f;
    public const float HAMMER_DELAY = 0.5f;
    public const float HAMMER_SPEED = 30f;
    public const float ATTACK_ANGLE = 60f;
    public const float AI_SPEED = 1.5f;
    public const float AI_ACCELERATION = 4f;
    public const float AI_ANGULAR_SPEED = 120f;

    // Player Settings
    public const float PLAYER_MOVE_SPEED = 6f;
    public const float GRAVITY = -9.81f;
    public const float STEP_OFFSET = 0.4f;
    public const float DETECT_RANGE = 2f;
    public const int MAX_HEALTH = 3;
    public const float GROUND_CHECK = -2f;

    // Scaling Settings
    public const int SCALE_STEP = 5;
    public const float SCALE_AMOUNT = 0.1f;
    public const float KILL_SCALE_AMOUNT = 0.3f;

    // UI Settings
    public const float UI_HEAD_OFFSET_Y = 0.3f;
    public const float HEAD_OFFSET_Y = 1.5f;
    public const float EDGE_PADDING = 50f;
    public const float MIN_VIEWPORT = 0.05f;
    public const float MAX_VIEWPORT = 0.95f;
    public const int MAX_INDICATORS = 7;

    // Pool Settings
    public const int AI_NAME_MIN = 100;
    public const int AI_NAME_MAX = 999;
    public const float MIN_SPAWN_DISTANCE = 15f;
    public const int HAMMER_POOL_SIZE = 5;
    public const int AI_PER_TYPE = 10;
    public const int MAX_SPAWN_ATTEMPTS = 20;

    // Combat Settings
    public const float ATTACK_RESET_DELAY = 1f;
    public const float HAMMER_RETURN_DELAY = 0.5f;
    public const float ATTACK_RANGE = 2.5f;
    public const float STOPPING_DISTANCE = 2f;

    // Coroutine Names
    public const string SPAWN_COROUTINE = "SpawnAIInAreas";

    // Prefab Names
    public const string AI_NAME_PREFIX = "AI_";
}