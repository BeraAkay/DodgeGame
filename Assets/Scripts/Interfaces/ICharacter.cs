using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    public void ApplyDamage(float value);
    public void ApplyHeal(float value);
    public void ModifyMSFlat(float value);
    public void ModifyMSMult(float value);
    public void SetRooted(bool value);
    public void SetStunned(bool value);
    public void SetStasis(bool value);
    public void Dash(AbilityManager.AbilityInput input);
    public void Blink(AbilityManager.AbilityInput input);

    public void SetInvincible(bool invincible);
    public void SetGodMode(bool godMode);

    public void AddToBuffs(string id, AbilityManager.EffectInfo effectInfo);
    public void AddToDebuffs(string id, AbilityManager.EffectInfo effectInfo);

    public void RemoveFromBuffs(string id);
    public void RemoveFromDebuffs(string id);

    public void RemoveAllDebuffs();

    public AbilityManager.EffectInfo HasBuff(string id);
    public AbilityManager.EffectInfo HasDebuff(string id);


}
