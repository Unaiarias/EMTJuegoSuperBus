using UnityEngine;

public class RhythmKeyboardInput : MonoBehaviour
{
    [SerializeField] private RhythmGameManager game;

    private void Update()
    {
        //Button Lane0 → OnClick → game.Hit(0)

        if (Input.GetKeyDown(KeyCode.A)) game.Hit(0);
        if (Input.GetKeyDown(KeyCode.S)) game.Hit(1);
        if (Input.GetKeyDown(KeyCode.D)) game.Hit(2);
        if (Input.GetKeyDown(KeyCode.F)) game.Hit(3);
    }
} 