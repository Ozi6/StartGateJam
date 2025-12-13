using System.Collections.Generic;
using UnityEngine;

public class MarketLogic
{

    public Dictionary<string, int> marketPrices = new Dictionary<string, int>() {

        {"Archer", 10 },
        {"Viking", 5 },
        {"Giant", 20},
        {"Scout", 15},
        {"Wizard", 50 }
    } ;

    public bool BuyUnit()
    {
        return false;
    }
    
    public bool SellUnit(Person person)
    {
        //player.gold += person.Give;
        return true;
    }
}
