using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatPanel : MonoBehaviour
{
    Combatant Combatant;

    public TMP_Text NameText;

    public Slider HPSlider;
    public TMP_Text HPText;

    public GameObject PowerPipsParent;
    public GameObject CharmPipsParent;
    public GameObject MagicPipsParent;

    public void InitPanel(Combatant combatant)
    {
        Combatant = combatant;

        NameText.text = Combatant.Profile.Character.CharacterName;

        HPSlider.maxValue = Combatant.Profile.MaxHP;
        HPSlider.value = Combatant.HP;
        HPText.text = $"{Combatant.HP}/{Combatant.Profile.MaxHP}";

        SetStat(PowerPipsParent, Combatant.Profile.Power, GameManager.instance.PowerColour);
        SetStat(CharmPipsParent, Combatant.Profile.Charm, GameManager.instance.CharmColour);
        SetStat(MagicPipsParent, Combatant.Profile.Magic, GameManager.instance.MagicColour);
    }

    public void HPChange(int newHP)
    {
        new TweenValue(GameManager.instance.HPChangeAnimationTime, HPSlider.value, newHP, (newValue) => HPSlider.value = newValue, Easing.outSine);
        HPText.text = $"{Combatant.HP}/{Combatant.Profile.MaxHP}";
    }

    public void SetStat(GameObject parent, int statValue, Color statColour)
    {
        Image[] pips = parent.GetComponentsInChildren<Image>();

        for (int i = 0; i < statValue; i++)
        {
            pips[i].color = statColour;
        }
    }
}
