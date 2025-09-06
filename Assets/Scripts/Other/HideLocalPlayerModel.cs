using UnityEngine;
using Mirror;

public class HideLocalPlayerModel : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel; // všechny meshe, co se mají skrýt

    public override void OnStartLocalPlayer()
    {
        // skryj model jen pro sebe
        if (playerModel != null)
            playerModel.SetActive(false);
    }
}
