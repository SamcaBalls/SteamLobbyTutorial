using UnityEngine;
using Mirror;

public class HideLocalPlayerModel : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel; // v�echny meshe, co se maj� skr�t

    public override void OnStartLocalPlayer()
    {
        // skryj model jen pro sebe
        if (playerModel != null)
            playerModel.SetActive(false);
    }
}
