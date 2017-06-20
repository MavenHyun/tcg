using System;
using System.Collections;
using System.Collections.Generic;

static class Constants
{

    /*throws, bats left or right*/
    public const int LEFT = 1;
    public const int RIGHT = 2;
    public const int BOTH = 3;

    /*fielder positions*/
    public const int SP = 10;
    public const int RP = 11;

    public const int CAT = 2;
    public const int FIR = 3;
    public const int SEC = 4;
    public const int THI = 5;
    public const int SS = 6;
    public const int LF = 7;
    public const int CF = 8;
    public const int RF = 9;
    public const int DH = 0;

    /*pitch types*/
    public const int FASTBALL = 0;
    public const int OFFSPEED = 1;
    public const int BREAKING = 2;
    public const int REVERSE = 3;

    /*pitch movements*/
    public const int STRAIGHT = 0;
    public const int ONE_LEFT = 1;
    public const int ONE_RIGHT = 2;
    public const int ONE_UP = 3;
    public const int ONE_DOWN = 4;
    public const int ONE_LEFTDOWN = 5;
    public const int ONE_RIGHTDOWN = 6;
    public const int TWO_DOWN = 7;
    public const int TWO_LEFT_ONE_DOWN = 8;
    public const int TWO_RIGHT_ONE_DOWN = 9;
    public const int ONE_LEFT_TWO_DOWN = 10;

}

public class Card
{
    public string card_name { get; protected set; }
    public int card_id { get; protected set; }
    public int card_season { get; protected set; }
    public string card_team { get; protected set; }
}

public class Manager : Card
{
    public int card_ability { get; protected set; }

    public Manager(string name, string team, int id, int season, int ability)
    {
        card_id = id;
        card_name = name;
        card_season = season;
        card_team = team;
        card_ability = ability;
    }
}

public class Pitcher : Card
{
    public string card_pos { get; protected set; }
    public int card_throws { get; protected set; }
    public string card_arsenal { get; protected set; }
    public int card_pp { get; protected set; }
    public int card_ability { get; protected set; }

    public Pitcher()
    {

    }

    public Pitcher(string name, string team, int id, int season, string pos, int throws, int pp, string arsenal)
    {
        card_id = id;
        card_name = name;
        card_season = season;
        card_team = team;
        card_pos = pos;
        card_throws = throws;
        card_pp = pp;
        card_ability = 0; /*temp*/
        card_arsenal = arsenal; 
    }
}

public class Fielder : Card
{
    public string card_pos { get; protected set; }
    public int card_throws { get; protected set; }
    public int card_bats { get; protected set; }
    public int card_bp { get; protected set; }
    public int card_con { get; protected set; }
    public int card_def { get; protected set; }
    public int card_ability { get; protected set; }
    public string card_pos0 { get; set; }

    public Fielder()
    {

    }

    public Fielder(string name, string team, int id, int season, string pos, int bats, int con, int bp,  int def, string pos0)
    {
        card_id = id;
        card_name = name;
        card_season = season;
        card_team = team;
        card_pos = pos;
        card_bats = bats;
        card_bp = bp;
        card_con = con;
        card_def = def;
        card_pos0 = pos0;
        card_ability = 0; /*temp*/
    }
}

public class Pitch : Card
{
    public string card_type { get; protected set; }
    public int card_cost { get; set; }
    public int card_move { get; protected set; }
    public int card_effect { get; protected set; }

    public Pitch()
    {

    }

    public Pitch(string name, int id, string type, int cost, int move, int effect)
    {
        card_id = id;
        card_name = name;
        card_type = type;
        card_cost = cost;
        card_move = move;
        card_effect = effect;
    }
}

public class Situation
{
    public string d_type { get; set; }
    public bool[] zone0 = new bool[3];
    public bool[] zone1 = new bool[3];
    public bool[] zone2 = new bool[3];

    public string[] f_holder = new string[10];

    public int p_id { get; set; }
    public string p_name { get; set; }

    public int b_id { get; set; }
    public string b_name { get; set; }
    public int b_bats { get; set; }
    public int b_con { get; set; }

    public bool[] bases = new bool[4];

    public string game_msg { get; set; }

    public int number { get; set; }

    public bool start_game { get; set; }
    public bool go_inning { get; set; }
    public bool go_atbat0 { get; set; }
    public bool go_atbat { get; set; }
    public bool go_sign { get; set; }
    public bool go_cont { get; set; }
    public bool go_result { get; set; }
    public bool go_final { get; set; }
    public bool go_final2 { get; set; }
    public bool go_final3 { get; set; }

    public bool bonus { get; set; }
    public bool locked { get; set; }
    public bool init { get; set; }
    public bool game_end { get; set; }

    public int def_pos { get; set; }
    public int cost { get; set; }
    public int bp_mod { get; set; }

    public bool inning { get; set; }
    public int inning_count { get; set; }
    public int inning_runs { get; set; }

    public int runs { get; set; }
    public int hits { get; set; }
    public int walks { get; set; }
    public int pitch_count { get; set; }

    public int opp_runs { get; set; }
    public int opp_hits { get; set; }
    public int opp_walks { get; set; }
    public int opp_pitch_count { get; set; }

    public int strike { get; set; }
    public int ball { get; set; }
    public int outs { get; set; }

    public int choose { get; set; }

    public Situation()
    {
        start_game = false;

        go_inning = false;
        go_atbat = false;
        go_atbat0 = false;
        go_sign = false;

        go_cont = false;
        go_result = false;
        go_final = false;
        go_final2 = false;
        go_final3 = false;

        bonus = false;
        def_pos = 0; cost = 0; bp_mod = 0;
        locked = false; init = true;  game_end = false;
        inning_count = 1; inning = true;
        runs = 0; hits = 0; walks = 0;
        pitch_count = 0;
        strike = 0;  ball = 0; outs = 0;
        choose = 0;
    }

    public void SwapRuns()
    {
        int temp = runs; runs = opp_runs; opp_runs = temp;
        temp = hits; hits = opp_hits; opp_hits = temp;
        temp = walks; walks = opp_walks; opp_walks = temp;
        temp = pitch_count; pitch_count = opp_pitch_count; opp_pitch_count = temp;
        bonus = !(bonus);
    }

    public void SaveFH(Fielder[] f, Stack<Pitcher> p)
    {
        f_holder[0] = p.Peek().card_name;

        for (int i = 2; i < 10; i++)
        {
            f_holder[i] = f[i].card_name;
        }
    }

    public void SaveBase(string [] z)
    {
        for (int i = 1; i < 4; i++)
        {
            if(z[i] != null)
            {
                bases[i] = true;
            }
            else
            {
                bases[i] = false;
            }

        }
    }

    public void ResetValues()
    {
        game_msg = null;
        def_pos = 0;
        cost = 0;
        choose = 0;
        bp_mod = 0;
    }
}
