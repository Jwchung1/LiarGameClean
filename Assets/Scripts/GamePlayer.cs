using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GamePlayer
{
    private string name_;
    private string color_;
    private int score_;

    public GamePlayer(string name, string color)
    {
        this.name_ = name;
        this.color_ = color;
        this.score_ = 0;
    }

    public void InitializePlayer(string name)
    {
        this.name_ = name;
    }

    public void SetScore(int score)
    {
        this.score_ = score;
    }
    public void AddScore(int score)
    {
        this.score_ += score;
    }
    public string GetName()
    {
        return this.name_;
    }

    public int GetScore()
    {
        return this.score_;
    }

    public string GetColor()
    {
        return this.color_;
    }
}
