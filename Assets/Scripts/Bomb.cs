using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Candy{
    override public void Die() {
        base.m_board.ActivateBombPowerUp(this.getBoardPosition());
        base.Die();
    }
}
