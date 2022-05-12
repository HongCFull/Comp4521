using UnityEngine;

public enum GridInfo
{
    SkillA,
    SkillB,
    SkillC,
    SkillD,
    Defense,
    Heal,
    PickUp,
    Obstacle
}

[CreateAssetMenu(fileName = "newCombatTerrain",menuName = "createNewCombatTerrain")]
public class CombatTerrain :ScriptableObject
{
    [SerializeField] private string terrainName;
    [SerializeField] private GameObject terrainPrefab;
    [Tooltip("This pivot will be assigned to grid[0,0]")]
    [SerializeField] private GameObject topLeftPivotPoint;
    
    [Tooltip("How many grids in the x axis")]
    [SerializeField] [Range(0,Mathf.Infinity)] private int width;

    [Tooltip("How many grids in the z axis")]
    [SerializeField] [Range(0,Mathf.Infinity)] private int height;
    [HideInInspector] public const float BlockWidth = 1.5f;

    [Header("Distribution settings")] 
    [SerializeField] private int skillAWeight;
    [SerializeField] private int skillBWeight;
    [SerializeField] private int skillCWeight;
    [SerializeField] private int skillDWeight;
    [SerializeField] private int defenseWeight;
    [SerializeField] private int healWeight;

    public GridInfo[,] gridInfos { get; private set; }

    private GridInfo[] stretchedGridInfo;
    private void Awake()
    {
        gridInfos = new GridInfo[height,width];
        stretchedGridInfo = new GridInfo[height * width];
        
        InitStretchedGridInfo();
        RandomizeGridInfo();
    }
    
    void InitStretchedGridInfo()
    {
        int totalWeightSum = skillAWeight + skillBWeight + skillCWeight + skillDWeight + defenseWeight + healWeight;
        int numOfSkillA = height * width* skillAWeight / totalWeightSum;
        int numOfSkillB = height * width* skillBWeight / totalWeightSum;
        int numOfSkillC = height * width* skillCWeight / totalWeightSum;
        int numOfSkillD = height * width* skillDWeight / totalWeightSum;
        int numOfDefense = height * width* defenseWeight / totalWeightSum;
        int numOfHeal = height * width* healWeight / totalWeightSum;

        for (int i = 0; i < height * width; i++) {
            if (numOfSkillA > 0) {
                stretchedGridInfo[i] = GridInfo.SkillA;
                numOfSkillA--;
            }else if (numOfSkillB > 0) {
                stretchedGridInfo[i] = GridInfo.SkillB;
                numOfSkillB--;
            }else if (numOfSkillC > 0) {
                stretchedGridInfo[i] = GridInfo.SkillC;
                numOfSkillC--;
            }else if (numOfSkillD > 0) {
                stretchedGridInfo[i] = GridInfo.SkillD;
                numOfSkillD--;
            }else if (numOfDefense > 0) {
                stretchedGridInfo[i] = GridInfo.Defense;
                numOfDefense--;
            }else if (numOfHeal > 0) {
                stretchedGridInfo[i] = GridInfo.Heal;
                numOfHeal--;
            }else {
                stretchedGridInfo[i] = GridInfo.SkillA;
            }
        }
    }

    private void RandomizeGridInfo()
    {
        RandomizeStretchedGridInfo();
        for (int h = 0; h < height; h++) {
            for (int w = 0; w < width; w++) {
                gridInfos[h, w] = stretchedGridInfo[w * h + h];
            }
        }
    }

    private void RandomizeStretchedGridInfo() {
        var rng = new System.Random();
        rng.Shuffle(stretchedGridInfo);
    }
}
