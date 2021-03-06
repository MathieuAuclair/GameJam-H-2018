﻿using System.Collections.Generic;

public class ScoreBoard : PersistentRAIISingleton<ScoreBoard>
{
	readonly IDictionary<int, float> scores = new Dictionary<int, float>();
	readonly IDictionary<int, string> nameMapping = new Dictionary<int, string>();
    int maxScorePlayerId = -1;

	public static IDictionary<int, float> GetScores()
    {
        return Instance._GetScores();
    }

	IDictionary<int, float> _GetScores()
    {
        return scores;
	}

    public static void Reset()
    {
        Instance._Reset();
    }

    void _Reset()
    {
        scores.Clear();
    }

	public static void IncreaseScore(int playerKey, float gain)
    {
        Instance._IncreaseScore(playerKey, gain);
    }

	void _IncreaseScore(int playerKey, float gain)
    {
        if (scores.ContainsKey(playerKey))
        {
            scores[playerKey] += gain;
        }
        else
        {
            scores[playerKey] = gain;
        }

        if (!scores.ContainsKey(maxScorePlayerId) || scores[playerKey] > scores[maxScorePlayerId])
        {
            maxScorePlayerId = playerKey;
        }
    }

    public static int GetLeading()
    {
        return Instance._GetLeading();
    }

    int _GetLeading()
    {
		return maxScorePlayerId;
    }

	public static float GetScore(int playerId)
    {
        return Instance._GetScore(playerId);
    }

	float _GetScore(int playerId)
    {
        if (!scores.ContainsKey(playerId))
        {
            scores[playerId] = 0;
        }
        return scores[playerId];
	}

	public static string GetName(int playerId)
	{
		return Instance._GetName(playerId);
	}

	string _GetName(int playerId)
	{
		if (!nameMapping.ContainsKey(playerId))
		{
			nameMapping[playerId] = "";
		}
		return nameMapping[playerId];
	}

	public static void SetName(int playerId, string name)
	{
		Instance._SetName(playerId, name);
	}

	void _SetName(int playerId, string _name)
	{
		nameMapping [playerId] = _name;
	}

	public static KeyValuePair<int, float> GetLeadingWithScore()
    {
        return Instance._GetLeadingWithScore();
    }

	KeyValuePair<int, float> _GetLeadingWithScore()
    {
		return new KeyValuePair<int, float>(maxScorePlayerId, scores[maxScorePlayerId]);
    }
}
