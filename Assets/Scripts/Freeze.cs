using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : Candy {

    public override void Die() {
        base.m_board.ActivateFreezePowerUp();
        base.Die();
    }
}
