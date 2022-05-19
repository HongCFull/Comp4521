using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[CreateAssetMenu(fileName = "MoveSetOnGrid Setting",menuName = "ScriptableObjects/New MoveSetOnGrid Setting")]
public class MoveSetOnGrid : ScriptableObject
{
    public enum MoveSetType
    {
        /// <summary>
        /// Obstacle has to be the LAST value!
        /// </summary>
    
        Melee,
        Directional,
        AOE,
        UltimateSkill,
        Defense,
        Heal,
        PickUp,
        Obstacle
    }
    
    [SerializeField] private Sprite meleeSprite;
    [SerializeField] private Sprite directionalSprite;
    [SerializeField] private Sprite AOESprite;
    [SerializeField] private Sprite ultimateSkillSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite healSprite;
    [SerializeField] private Sprite pickUpSprite;
    [SerializeField] private Sprite obstacleSprite;

    public Sprite GetSpriteByMoveSetType(MoveSetType type)
    {
        switch (type)
        {
            case MoveSetType.Melee:
                return meleeSprite;
            
            case MoveSetType.Directional:
                return directionalSprite;
            
            case MoveSetType.AOE:
                return AOESprite;
            
            case MoveSetType.UltimateSkill:
                return ultimateSkillSprite;
            
            case MoveSetType.Defense:
                return defenseSprite;
            
            case MoveSetType.Heal:
                return healSprite;

            case MoveSetType.PickUp:
                return pickUpSprite;
            
            case MoveSetType.Obstacle:
                return obstacleSprite;
        }
        return null;
    }

}
