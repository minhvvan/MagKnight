using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeStatNPC : BaseNPCController
{
    protected override void InteractExit()
    {
        base.InteractExit();

        UIManager.Instance.ShowUpgradeStatUI();
    }
}
