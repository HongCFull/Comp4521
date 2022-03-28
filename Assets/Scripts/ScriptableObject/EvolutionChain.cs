using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewMonsterEvolutionChain", menuName = "ScriptableObjects/NewMonsterEvolutionChain", order = 1)]
public class EvolutionChain : ScriptableObject
{
   
   [Header("Monster stages in this evolution chain")]
   [SerializeField] private MonsterInfo monsterStage1Info;
   [SerializeField] private MonsterInfo monsterStage2Info;
   [SerializeField] private MonsterInfo monsterStage3Info;
   
   [Header("Evolution Info")] 
   [SerializeField] [Range(1,3)] private int evolutionStage=1;
   [SerializeField] [Range(1,100)] private int monsterLevel=1;
   [SerializeField] [Range(0,10000f)] private float monsterExperience;
   private float exp;
   private float lv;
   
   //Move it to evolution chain
   public int MonsterLevel => monsterLevel;
   public float MonsterExperience => monsterExperience;
   public float GainExperience(float exp) => monsterExperience += exp;

   //TODO: Unlock the next evolution stage etc. if next stage is reached
   
   
   
   /// <summary>
   /// Get corresponding MonsterInfo of the input stage 
   /// </summary>
   /// <param name="stage"> stage in {1,2,3}</param>
   /// <returns></returns>
   public MonsterInfo GetMonsterInfoAtCurrentStage()
   {
      switch (evolutionStage) {
         case 1:
            return monsterStage1Info;
         case 2:
            return monsterStage2Info;
         case 3:
            return monsterStage3Info;
      }
      return null;
   }
   
   /// <summary>
   /// Get corresponding MonsterInfo of the input stage 
   /// </summary>
   /// <param name="stage"> stage in {1,2,3}</param>
   /// <returns></returns>
   public MonsterInfo GetMonsterInfoAtStage(int stage)
   {
      switch (stage) {
         case 1:
            return monsterStage1Info;
         case 2:
            return monsterStage2Info;
         case 3:
            return monsterStage3Info;
      }
      return null;
   }
}
