using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAIManager : MonoBehaviour
{
    protected GameController _gameController;

    public void SetGameController(GameController gameCont)
    {
        _gameController = gameCont;
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    virtual public void OnRaceStart()
    {

    }

    // Update is called once per frame
    public virtual void UpdateController()
    {
        
    }
}
