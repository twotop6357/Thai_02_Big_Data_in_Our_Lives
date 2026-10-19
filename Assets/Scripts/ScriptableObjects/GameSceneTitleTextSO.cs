using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSceneTitleTextSO", menuName = "Text/CreateGameSceneTitleTextSO", order = 0)]
public class GameSceneTitleTextSO : ScriptableObject
{
    public string groupSelectTitleText;
    public string groupSelectSubtitleText;
    public string otherSelectTitleText;
    public string otherSelectSubtitleText;
}
