using System.Collections;
using System.Collections.Generic;
using TFarm.Transition;
using UnityEngine;

namespace TFarm.Save
{
    public class DataSlot
    {
        /// <summary>
        /// Load Process (GUID is string)
        /// </summary>
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();

        #region To Display Load Process (How many data has been loaded)
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "Year/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "Month/" + timeData.timeDict["gameDay"] + "Day/";
                }
                else return string.Empty;
            }
        }

        public string DataScene
        {
            get
            {
                var key = TransitionManager.Instance.GUID;
                if (dataDict.ContainsKey(key))
                {
                    var transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "01.Field" => "Farm",
                        "02.Home" => "Home",
                        _ => string.Empty
                    };
                }
                else return string.Empty;
            }
        }
        #endregion
    }
}
