using System.Collections.Generic;
using UnityEngine;

public class MarketLogic
{

    public Dictionary<string, int> marketPrices = new Dictionary<string, int>() {

        {"Crook", 10 },
        {"Archer", 20 },
        {"Knight", 30 },
        {"Giant", 40 },
        {"Scout", 50 },
    } ;

    public bool buyUnit()
    {
        return false;
    }

    public bool sellUnit(Person person)
    {
        //player.gold += person.GivenGold;
        return true;
    }
}
